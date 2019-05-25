using PatchMap.Exceptions;
using PatchMap.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public MapResult<TTarget, TContext> Map(IEnumerable<PatchOperation> operations, TTarget target, TContext context)
        {
            return Map(operations, target, context, new MapConfiguration
            {
                AllowUnmappedOperations = false
            });
        }

        public MapResult<TTarget, TContext> Map(IEnumerable<PatchOperation> operations, TTarget target, TContext context, MapConfiguration configuration)
        {
            var result = new MapResult<TTarget, TContext> { Context = context };
            var addressedOperations = new List<PatchOperation>();

            bool mapProcessedWithChanges(FieldMap<TTarget, TContext> map)
            {
                var matchingOperations = operations.Where(op =>
                {
                    var prop = op.PropertyTree;
                    for (var i = 0; i < map.SourceField.Count; i++)
                    {
                        var sourceField = map.SourceField[i];

                        if (prop == null || !sourceField.PropertyType.IsAssignableFrom(prop.Property.PropertyType) || sourceField.Name != prop.Property.Name)
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

                bool mapResultedInUpdate = false;
                foreach (var operation in matchingOperations)
                {
                    addressedOperations.Add(operation);
                    if (operation.JsonPatch != null && !operation.JsonPatchValueParsed)
                    {
                        result.AddFailure(map, operation, MapResultFailureType.JsonPatchValueNotParsable);
                    }
                    else if (map.Enabled == null || map.Enabled(target, context))
                    {
                        object originalValue = null;
                        object targetObject = target;
                        PropertyInfo targetPropertyInfo = null;
                        if (map.TargetField.Any())
                        {
                            for (var i = 0; i < map.TargetField.Count; i++)
                            {
                                var pi = map.TargetField[i];
                                if (i < map.TargetField.Count - 1)
                                {
                                    targetObject = pi.GetValue(targetObject);

                                    if (targetObject == null)
                                    {
                                        throw new ArgumentException($"Could not set value for {pi.Name} on {pi.PropertyType.Name} because object was null");
                                    }
                                }
                                else
                                {
                                    originalValue = pi.GetValue(targetObject);
                                    targetPropertyInfo = pi;
                                }
                            }
                        }

                        var processedValue = map.ConvertValue(target, operation.Value, context);

                        if (!processedValue.Succeeded)
                        {
                            result.AddFailure(map, operation, MapResultFailureType.ValueConversionFailed, processedValue.FailureReason);
                        }
                        else
                        {
                            bool hasChanges = false;
                            bool valueIsMissing = processedValue.Value == null || (processedValue.Value is string s && string.IsNullOrEmpty(s));
                            bool? isRequiredFromContext = (valueIsMissing && map.Required != null)
                                ? map.Required(target, context)
                                : (bool?)null;
                            bool isTargetRequired = valueIsMissing && map.TargetField.Any() && MapTargetIsRequired(target, context, map, operation);

                            if (isRequiredFromContext == true || (isRequiredFromContext == null && isTargetRequired))
                            {
                                result.AddFailure(map, operation, MapResultFailureType.Required);
                            }
                            else if (map.TargetField.Any())
                            {
                                var valueCheck = MapTargetValueIsValid(target, context, map, operation, processedValue.Value);
                                if (valueCheck.IsValid)
                                {
                                    targetPropertyInfo.SetValue(targetObject, processedValue.Value);

                                    hasChanges = MapTargetHasChanged(target, context, map, operation, originalValue, processedValue.Value);
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
                                map.PostMap?.Invoke(target, context, map, operation);
                                mapResultedInUpdate = true;
                            }
                        }
                    }
                }

                return mapResultedInUpdate;
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
                            compoundMap.PostMap?.Invoke(target, context);
                        }

                        return hasChanges;
                    default:
                        return false;
                }
            }

            routeMap(this);

            if (!configuration.AllowUnmappedOperations)
            {
                var unaddressOperation = operations.FirstOrDefault(o => !addressedOperations.Contains(o) && o.JsonPatch != null);
                if (unaddressOperation != null)
                {
                    throw new JsonPatchParseException(unaddressOperation.JsonPatch, $"A map for {unaddressOperation.JsonPatch.path} has not been configured.");
                }
            }

            return result;
        }
    }
}