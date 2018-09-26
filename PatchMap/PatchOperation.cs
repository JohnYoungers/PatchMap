using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PatchMap
{
    public class PatchOperation
    {
        public PatchOperationTypes Operation { get; set; }

        public JsonPatch JsonPatch { get; set; }
        public bool JsonPatchValueParsed { get; set; }
        public object Value { get; set; }

        public PatchOperationPropertyTree PropertyTree { get; set; }

        public override string ToString()
        {
            return PropertyTree?.ToString() + ": " + Value;
        }
    }

    public static class PatchOperationExtensions
    {
        public static List<PatchOperation> ToPatchOperations(this object model)
        {
            return (model == null) ? new List<PatchOperation>() : BuildFromObject(model, new Stack<PropertyInfo>());
        }
        private static List<PatchOperation> BuildFromObject(object model, Stack<PropertyInfo> stack)
        {
            var results = new List<PatchOperation>();
            foreach (var property in model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                    .Where(p => p.CanRead
                                                                && p.GetCustomAttribute(typeof(System.Runtime.Serialization.IgnoreDataMemberAttribute)) == null
                                                                && p.GetCustomAttribute(typeof(Attributes.PatchIgnoreAttribute)) == null))
            {
                stack.Push(property);
                var parseEntireObject = (property.GetCustomAttribute(typeof(Attributes.PatchRecursivelyAttribute)) == null);
                var isCollection = typeof(IEnumerable).IsAssignableFrom(property.PropertyType);
                var value = property.GetValue(model);

                if (!property.PropertyType.IsValueType && !isCollection && !parseEntireObject)
                {
                    results.AddRange(BuildFromObject(value ?? Activator.CreateInstance(property.PropertyType), stack));
                }
                else
                {
                    var propertyTree = new PatchOperationPropertyTree();
                    var currentBranch = propertyTree;
                    foreach (var p in stack.Reverse())
                    {
                        if (currentBranch.Property == null)
                        {
                            currentBranch.Property = p;
                        }
                        else
                        {
                            currentBranch.Next = new PatchOperationPropertyTree { Property = p };
                            currentBranch = currentBranch.Next;
                        }
                    }

                    results.Add(new PatchOperation
                    {
                        Operation = PatchOperationTypes.replace,
                        PropertyTree = propertyTree,
                        Value = value
                    });
                }

                stack.Pop();
            }

            return results;
        }
    }
}
