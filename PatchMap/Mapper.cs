using PatchMap.Exceptions;
using PatchMap.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PatchMap
{
    public delegate bool MapTargetIsRequiredMethod<TTarget, TContext>(TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation);
    public delegate FieldMapValueValidResult MapTargetValueIsValidMethod<TTarget, TContext>(TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation, object value);
    public delegate bool MapTargetHasChangedMethod<TTarget, TContext>(TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation, object originalValue, object newValue);

    public class Mapper<TSource, TTarget, TContext> : CompoundMap<TSource, TTarget, TContext>
    {
        public MapTargetHasChangedMethod<TTarget, TContext> MapTargetHasChanged { get; set; }
            = (target, ctx, map, operation, oldValue, newValue) => !Equals(oldValue, newValue);
        public MapTargetIsRequiredMethod<TTarget, TContext> MapTargetIsRequired { get; set; }
            = (target, ctx, map, operation) => false;
        public MapTargetValueIsValidMethod<TTarget, TContext> MapTargetValueIsValid { get; set; }
            = (target, ctx, map, operation, value) => new FieldMapValueValidResult { IsValid = true };

        public MapResult<TTarget, TContext> Map(IEnumerable<PatchOperation> operations, TTarget target, TContext ctx)
        {
            var result = new MapResult<TTarget, TContext> { Context = ctx };
            var addressedOperations = new List<PatchOperation>();

            bool mapProcessedWithChanges(FieldMap<TTarget, TContext> map)
            {
                var operation = operations.FirstOrDefault(op =>
                {
                    var prop = op.PropertyTree;
                    for (var i = 0; i < map.SourceField.Count; i++)
                    {
                        var sourceField = map.SourceField[i];

                        if (prop == null || sourceField.DeclaringType != prop.Property.DeclaringType || sourceField.Name != prop.Property.Name)
                        {
                            return false;
                        }
                        if (i < map.SourceField.Count - 1)
                        {
                            prop = prop.Next;
                        }
                    }

                    if ((!map.CollectionItem && !string.IsNullOrEmpty(prop.CollectionKey))
                        || (map.CollectionItem && string.IsNullOrEmpty(prop.CollectionKey)))
                    {
                        return false;
                    }

                    return true;
                });

                if (operation != null)
                {
                    addressedOperations.Add(operation);
                    if (operation.JsonPatch != null && !operation.JsonPatchValueParsed)
                    {
                        result.AddFailure(map, operation, MapResultFailureType.JsonPatchValueNotParsable);
                    }
                    else if (map.Enabled == null || map.Enabled(target, ctx))
                    {
                        object originalValue = null;
                        var processedValue = map.ConvertValue(target, operation.Value, ctx);

                        if (!processedValue.Succeeded)
                        {
                            result.AddFailure(map, operation, MapResultFailureType.ValueConversionFailed, processedValue.FailureReason);
                            return false;
                        }
                        else
                        {
                            bool hasChanges = false;
                            bool valueIsMissing = processedValue.Value == null || (processedValue.Value is string s && string.IsNullOrEmpty(s));
                            bool? isRequiredFromContext = (valueIsMissing && map.Required != null)
                                ? map.Required(target, ctx)
                                : (bool?)null;
                            bool isTargetRequired = valueIsMissing && map.TargetField.Any() && MapTargetIsRequired(target, ctx, map, operation);

                            if (isRequiredFromContext == true || (isRequiredFromContext == null && isTargetRequired))
                            {
                                result.AddFailure(map, operation, MapResultFailureType.Required);
                            }
                            else if (map.TargetField.Any())
                            {
                                var valueCheck = MapTargetValueIsValid(target, ctx, map, operation, processedValue.Value);
                                if (valueCheck.IsValid)
                                {
                                    object obj = target;
                                    for (var i = 0; i < map.TargetField.Count; i++)
                                    {
                                        var pi = map.TargetField[i];
                                        if (i < map.TargetField.Count - 1)
                                        {
                                            obj = pi.GetValue(obj);

                                            if (obj == null)
                                            {
                                                throw new ArgumentException($"Could not set value for {pi.Name} on {pi.PropertyType.Name} because object was null");
                                            }
                                        }
                                        else
                                        {
                                            originalValue = pi.GetValue(obj);
                                            pi.SetValue(obj, processedValue.Value);
                                        }
                                    }

                                    hasChanges = MapTargetHasChanged(target, ctx, map, operation, originalValue, processedValue.Value);
                                }
                                else
                                {
                                    result.AddFailure(map, operation, MapResultFailureType.ValueIsNotValid, valueCheck.FailureReason);
                                }
                            }
                            else
                            {
                                hasChanges = true;
                            }

                            if (hasChanges)
                            {
                                map.PostMap?.Invoke(target, ctx, map, operation);
                            }

                            return hasChanges;
                        }
                    }
                }

                return false;
            }

            bool routeMap(Map<TTarget, TContext> map)
            {
                switch (map)
                {
                    case FieldMap<TTarget, TContext> patchMap:
                        return mapProcessedWithChanges(patchMap);
                    case CompoundMap<TSource, TTarget, TContext> compoundMap:
                        var mapResults = compoundMap.Mappings.Select(m => routeMap(m)).ToArray();
                        var hasChanges = mapResults.Any(changed => changed);
                        if (hasChanges)
                        {
                            compoundMap.PostMap?.Invoke(target, ctx);
                        }

                        return hasChanges;
                    default:
                        return false;
                }
            }

            routeMap(this);

            var unaddressOperation = operations.FirstOrDefault(o => !addressedOperations.Contains(o) && o.JsonPatch != null);
            if (unaddressOperation != null)
            {
                throw new JsonPatchParseException(unaddressOperation.JsonPatch, $"A map for {unaddressOperation.JsonPatch.path} has not been configured.");
            }

            return result;
        }
    }
}