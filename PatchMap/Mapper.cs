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
            var mappedOperations = new Dictionary<Map<TTarget, TContext>, Queue<PatchOperation>>();

            bool pairOperationToMaps(Map<TTarget, TContext> map, PatchOperation op)
            {
                switch (map)
                {
                    case FieldMap<TTarget, TContext> patchMap:
                        var prop = op.PropertyPath;
                        for (var i = 0; i < patchMap.SourceField.Count; i++)
                        {
                            var sourceField = patchMap.SourceField[i];

                            if (prop == null || !sourceField.PropertyType.IsAssignableFrom(prop.Property.PropertyType) || sourceField.Name != prop.Property.Name)
                            {
                                return false;
                            }
                            if (i < patchMap.SourceField.Count - 1)
                            {
                                prop = prop.Next!;
                            }
                        }

                        if ((!patchMap.CollectionItem && !string.IsNullOrEmpty(prop.CollectionKey))
                            || (patchMap.CollectionItem && string.IsNullOrEmpty(prop.CollectionKey)))
                        {
                            return false;
                        }

                        if (!mappedOperations.ContainsKey(map))
                        {
                            mappedOperations.Add(map, new Queue<PatchOperation>());
                        }
                        mappedOperations[map].Enqueue(op);

                        return true;
                    case CompoundMap<TSource, TTarget, TContext> compoundMap:
                        var pairingResults = compoundMap.Mappings.Select(m => pairOperationToMaps(m, op)).ToArray();

                        return pairingResults.Any(mapped => mapped);
                    default:
                        return false;
                }
            }

            foreach (var op in operations)
            {
                var foundMap = pairOperationToMaps(this, op);
                if (!foundMap && op.JsonPatch != null && !configuration.AllowUnmappedOperations)
                {
                    throw new JsonPatchParseException(op.JsonPatch, $"A map for {op.JsonPatch.path} has not been configured.");
                }
            }

            var result = new MapResult<TTarget, TContext>(context);

            bool executeMap(Map<TTarget, TContext> map)
            {
                switch (map)
                {
                    case FieldMap<TTarget, TContext> patchMap:
                        bool mapResultedInUpdate = false;
                        foreach (var operation in (mappedOperations.ContainsKey(map) ? mappedOperations[map] : Enumerable.Empty<PatchOperation>()))
                        {
                            if (operation.JsonPatch != null && !operation.JsonPatchValueParsed)
                            {
                                result.AddFailure(patchMap, operation, MapResultFailureType.JsonPatchValueNotParsable);
                            }
                            else if (patchMap.Enabled == null || patchMap.Enabled(target, context))
                            {
                                object? originalValue = null;
                                object? targetObject = target;
                                PropertyInfo? targetPropertyInfo = null;
                                if (patchMap.TargetField.Any())
                                {
                                    for (var i = 0; i < patchMap.TargetField.Count; i++)
                                    {
                                        var pi = patchMap.TargetField[i];
                                        if (i < patchMap.TargetField.Count - 1)
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

                                var processedValue = patchMap.ConvertValue(target, operation.Value, context);

                                if (!processedValue.Succeeded)
                                {
                                    result.AddFailure(patchMap, operation, MapResultFailureType.ValueConversionFailed, processedValue.FailureReason);
                                }
                                else
                                {
                                    bool hasChanges = false;
                                    bool valueIsMissing = processedValue.Value == null || (processedValue.Value is string s && string.IsNullOrEmpty(s));
                                    bool? isRequiredFromContext = (valueIsMissing && patchMap.Required != null)
                                        ? patchMap.Required(target, context)
                                        : (bool?)null;
                                    bool isTargetRequired = valueIsMissing && patchMap.TargetField.Any() && MapTargetIsRequired(target, context, patchMap, operation);

                                    if (isRequiredFromContext == true || (isRequiredFromContext == null && isTargetRequired))
                                    {
                                        result.AddFailure(patchMap, operation, MapResultFailureType.Required);
                                    }
                                    else if (patchMap.TargetField.Any())
                                    {
                                        var valueCheck = MapTargetValueIsValid(target, context, patchMap, operation, processedValue.Value);
                                        if (valueCheck.IsValid)
                                        {
                                            targetPropertyInfo!.SetValue(targetObject, processedValue.Value);

                                            hasChanges = MapTargetHasChanged(target, context, patchMap, operation, originalValue, processedValue.Value);
                                        }
                                        else
                                        {
                                            result.AddFailure(patchMap, operation, MapResultFailureType.ValueIsNotValid, valueCheck.FailureReason);
                                        }
                                    }
                                    else
                                    {
                                        hasChanges = true;
                                    }

                                    if (hasChanges)
                                    {
                                        patchMap.PostMap?.Invoke(target, context, patchMap, operation);
                                        mapResultedInUpdate = true;
                                    }
                                }
                            }
                        }

                        return mapResultedInUpdate;
                    case CompoundMap<TSource, TTarget, TContext> compoundMap:
                        var mapResults = compoundMap.Mappings.Select(m => executeMap(m)).ToArray();
                        var hasChildChanges = mapResults.Any(changed => changed);
                        if (hasChildChanges)
                        {
                            compoundMap.PostMap?.Invoke(target, context);
                        }

                        return hasChildChanges;
                    default:
                        return false;
                }
            }

            executeMap(this);

            return result;
        }
    }
}