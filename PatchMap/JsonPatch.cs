using Newtonsoft.Json.Linq;
using PatchMap.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PatchMap
{
    [Serializable]
    public class JsonPatch
    {
        public PatchOperationTypes op { get; set; }
        public string path { get; set; }
        public object value { get; set; }
    }

    public static class JsonPatchExtensionMethods
    {
        public static List<PatchOperation> ToPatchOperations<T>(this IEnumerable<JsonPatch> patches) where T : class
        {
            return (patches == null) ? new List<PatchOperation>() : patches.Select(p => BuildFromJson<T>(p)).ToList();
        }

        private static PatchOperation BuildFromJson<T>(JsonPatch patch) where T : class
        {
            var result = new PatchOperation() { Operation = patch.op, JsonPatch = patch, JsonPatchValueParsed = true };

            var currentType = typeof(T);
            PropertyInfo currentProperty = null;
            PatchOperationPropertyTree lastCompiledProperty = null;

            var splitPath = ParseJsonPath(patch.path);
            for (var i = 0; i < splitPath.Count; i++)
            {
                bool isLastPart() => (i + 1 == splitPath.Count);
                string collectionKey = null;
                var part = splitPath[i];
                currentProperty = currentType.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (currentProperty == null)
                {
                    throw new JsonPatchParseException(patch, $"{currentType.Name} does not contain a property named {part}");
                }

                currentType = currentProperty.PropertyType;
                if (typeof(IEnumerable).IsAssignableFrom(currentType) && currentType != typeof(string))
                {
                    if (currentType.IsGenericType)
                    {
                        currentType = currentType.GetGenericArguments()[0];
                    }

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

                var compiledProperty = new PatchOperationPropertyTree() { Property = currentProperty, CollectionKey = collectionKey };
                if (lastCompiledProperty == null)
                {
                    result.PropertyTree = compiledProperty;
                }
                else
                {
                    lastCompiledProperty.Next = compiledProperty;
                }

                lastCompiledProperty = compiledProperty;
            }

            if (result.PropertyTree == null)
            {
                throw new JsonPatchParseException(patch, $"Path {patch.path} is not valid");
            }

            if (patch.value is JToken)
            {
                result.Value = (patch.value as JToken).ToObject(currentType);
            }
            else
            {
                var conversionType = Nullable.GetUnderlyingType(currentType) ?? currentType;
                try
                {
                    //Convert value to null if it's an empty string and the target type is not string
                    var objValue = (patch.value is string && conversionType != typeof(string) && string.IsNullOrEmpty(patch.value as string)) ? null : patch.value;

                    if (objValue == null && currentType != conversionType)
                    {
                        result.Value = null;
                    }
                    else if (conversionType == typeof(Guid) && objValue is string guid)
                    {
                        result.Value = Guid.Parse(guid);
                    }
                    else
                    {
                        result.Value = Convert.ChangeType(objValue, conversionType);
                    }
                }
                catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is ArithmeticException)
                {
                    result.JsonPatchValueParsed = false;
                }
            }

            return result;
        }

        private static List<string> ParseJsonPath(string fullPath)
        {
            var results = new List<string>();

            if (!string.IsNullOrEmpty(fullPath) && fullPath.Length > 0)
            {
                var chars = fullPath.ToCharArray();
                var currentSegment = new StringBuilder();
                for (int i = (chars[0] == '/' ? 1 : 0); i < chars.Length; i++)
                {
                    switch (chars[i])
                    {
                        case '\\':
                            if (chars.Length + 1 > i && chars[i + 1] == '/')
                            {
                                currentSegment.Append('/');
                                i++;
                            }
                            else
                            {
                                currentSegment.Append('\\');
                            }
                            break;
                        case '/':
                            results.Add(currentSegment.ToString());
                            currentSegment = new StringBuilder();
                            break;
                        default:
                            currentSegment.Append(chars[i]);
                            break;
                    }
                }

                if (currentSegment.Length > 0)
                {
                    results.Add(currentSegment.ToString());
                }
            }

            return results;
        }
    }
}
