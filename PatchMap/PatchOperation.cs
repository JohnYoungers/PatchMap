using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PatchMap
{
    public class PatchOperation
    {
        public PatchOperationTypes Operation { get; set; }
        public JsonPatch JsonPatch { get; set; }
        public bool JsonPatchValueParsed { get; set; }
        public PatchOperationPropertyTree PropertyTree { get; set; }
        public object Value { get; set; }

        public static PatchOperation Create<T, Y>(T source, Expression<Func<T, Y>> field)
        {
            var properties = new Stack<PatchOperationPropertyTree>();
            var memberExp = field.Body as MemberExpression ?? (field.Body as UnaryExpression)?.Operand as MemberExpression;
            while (memberExp != null)
            {
                properties.Push(new PatchOperationPropertyTree { Property = (PropertyInfo)memberExp.Member });
                memberExp = memberExp.Expression as MemberExpression;
            }

            var operation = new PatchOperation();
            PatchOperationPropertyTree currentProperty = null;
            object currentValue = source;
            while (properties.Any())
            {
                var prop = properties.Pop();

                if (currentProperty == null)
                {
                    operation.PropertyTree = prop;
                }
                else
                {
                    currentProperty.Next = prop;
                }

                currentProperty = prop;
                if (currentValue == null)
                {
                    currentValue = prop.Property.PropertyType.IsValueType
                        ? Activator.CreateInstance(prop.Property.PropertyType)
                        : null;
                }
                else
                {
                    currentValue = prop.Property.GetValue(currentValue);
                }
            }

            operation.Value = currentValue;
            return operation;
        }

        public override string ToString()
        {
            return PropertyTree?.ToString() + ": " + Value;
        }
    }

    public class PatchOperation<T> : PatchOperation
    {
        public static PatchOperation Create<Y>(Expression<Func<T, Y>> field, Y value)
        {
            var operation = Create(default(T), field);
            operation.Value = value;

            return operation;
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
