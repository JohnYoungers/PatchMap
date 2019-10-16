using PatchMap.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PatchMap
{
    public delegate bool ValueIsParsable(object? obj);
    public delegate object? ParseValue(object? obj, Type targetType);

    [Serializable]
    public class JsonPatch
    {
        public static ValueIsParsable ValueIsParsable { get; set; } = (o) => false;
        public static ParseValue ParseValue { get; set; } = (o, t) => o;

#pragma warning disable IDE1006 // Naming Styles
        public PatchOperationTypes op { get; set; }
        public string? path { get; set; }
        public object? value { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    public static class JsonPatchExtensionMethods
    {
        public static IEnumerable<PatchOperation> ToPatchOperations<T>(this IEnumerable<JsonPatch> patches) where T : class
        {
            return (patches == null) ? Enumerable.Empty<PatchOperation>() : patches.Select(p => BuildFromJson<T>(p));
        }

        private static PatchOperation BuildFromJson<T>(JsonPatch patch) where T : class
        {
            var currentType = typeof(T);
            PropertyInfo? currentProperty = null;
            PatchOperationPropertyPath? propertyTree = null;
            PatchOperationPropertyPath? lastCompiledProperty = null;

            var splitPath = ParseJsonPath(patch.path.AsSpan());
            for (var i = 0; i < splitPath.Count; i++)
            {
                bool isLastPart() => (i + 1 == splitPath.Count);
                string? collectionKey = null;
                var part = splitPath[i];

                currentProperty = currentType.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (currentProperty == null)
                {
                    throw new JsonPatchParseException(patch, $"{currentType.Name} does not contain a property named {part}");
                }

                currentType = currentProperty.PropertyType;
                if (typeof(IEnumerable).IsAssignableFrom(currentType) && currentType != typeof(string))
                {
                    if (patch.op == PatchOperationTypes.remove)
                    {
                        if (patch.value != null)
                        {
                            throw new JsonPatchParseException(patch, $"{patch.op.ToString()} operation should not contain a value");
                        }
                        if (isLastPart())
                        {
                            throw new JsonPatchParseException(patch, $"{patch.op.ToString()} operation is invalid on {currentProperty.Name} without the collection item's key");
                        }
                    }

                    if (!isLastPart())
                    {
                        collectionKey = splitPath[++i];
                        if (currentType.IsGenericType)
                        {
                            currentType = currentType.GetGenericArguments()[0];
                        }
                    }
                }
                else
                {
                    if (patch.op != PatchOperationTypes.replace)
                    {
                        throw new JsonPatchParseException(patch, $"{patch.op.ToString()} operation is only valid on collections");
                    }

                    if (currentProperty.GetCustomAttribute(typeof(Attributes.PatchRecursivelyAttribute)) == null)
                    {
                        if (!isLastPart())
                        {
                            throw new JsonPatchParseException(patch, $"{currentProperty.Name} can only be updated in one patch");
                        }
                    }
                    else if (!currentType.IsValueType && currentType != typeof(string) && isLastPart())
                    {
                        throw new JsonPatchParseException(patch, $"{currentProperty.Name} can not be updated in one patch");
                    }
                }

                var compiledProperty = new PatchOperationPropertyPath(currentProperty) { CollectionKey = collectionKey };
                if (lastCompiledProperty == null)
                {
                    propertyTree = compiledProperty;
                }
                else
                {
                    lastCompiledProperty.Next = compiledProperty;
                }

                lastCompiledProperty = compiledProperty;
            }

            if (propertyTree == null)
            {
                throw new JsonPatchParseException(patch, $"Path {patch.path} is not valid");
            }

            var jsonPatchValueParsed = true;
            object? value = null;
            if (JsonPatch.ValueIsParsable(patch.value))
            {
                value = JsonPatch.ParseValue(patch.value, currentType);
            }
            else
            {
                var conversionType = Nullable.GetUnderlyingType(currentType) ?? currentType;
                try
                {
                    // Convert value to null if it's an empty string and the target type is not string
                    var objValue = (patch.value is string && conversionType != typeof(string) && string.IsNullOrEmpty(patch.value as string)) ? null : patch.value;

                    if (objValue != null || currentType == conversionType)
                    {
                        if (conversionType == typeof(Guid) && objValue is string guid)
                        {
                            value = Guid.Parse(guid);
                        }
                        else
                        {
                            value = Convert.ChangeType(objValue, conversionType);
                        }
                    }
                }
                catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is ArithmeticException)
                {
                    jsonPatchValueParsed = false;
                }
            }

            return new PatchOperation(propertyTree, patch.op, value) { JsonPatch = patch, JsonPatchValueParsed = jsonPatchValueParsed };
        }

        private static List<string> ParseJsonPath(ReadOnlySpan<char> fullPath)
        {
            var results = new List<string>();

            if (fullPath.Length > 0)
            {
                var curStartIndex = fullPath[0] == '/' ? 1 : 0;
                var curParts = new List<string>();

                for (int i = curStartIndex; i < fullPath.Length; i++)
                {
                    if (fullPath[i] == '\\' && fullPath.Length + 1 > i && fullPath[i + 1] == '/')
                    {
                        curParts.Add(fullPath.Slice(curStartIndex, i - curStartIndex).ToString());
                        curStartIndex = ++i;
                    }
                    else if (fullPath[i] == '/' || i == fullPath.Length - 1)
                    {
                        var endOfString = i == fullPath.Length - 1;
                        results.Add(string.Concat(string.Concat(curParts), fullPath.Slice(curStartIndex, i - curStartIndex + (endOfString ? 1 : 0)).ToString()));
                        curParts.Clear();
                        curStartIndex = i + 1;
                    }
                }
            }

            return results;
        }
    }
}
