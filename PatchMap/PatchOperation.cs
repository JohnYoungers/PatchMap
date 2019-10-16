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
        public JsonPatch? JsonPatch { get; set; }
        public bool JsonPatchValueParsed { get; set; }
        public PatchOperationPropertyPath PropertyPath { get; set; }
        public object? Value { get; set; }

        public static PatchOperation Create<T, Y>(T? entity, Expression<Func<T, Y>> field) where T : class
        {
            var properties = new Stack<PatchOperationPropertyPath>();
            var memberExp = field.Body as MemberExpression ?? (field.Body as UnaryExpression)?.Operand as MemberExpression;
            while (memberExp != null)
            {
                properties.Push(new PatchOperationPropertyPath((PropertyInfo)memberExp.Member));
                memberExp = memberExp.Expression as MemberExpression;
            }

            PatchOperationPropertyPath path = properties.Pop();

            static object? getNextValue(object? value, PropertyInfo pi) => value == null
                    ? (pi.PropertyType.IsValueType ? Activator.CreateInstance(pi.PropertyType) : null)
                    : pi.GetValue(value);
            object? currentValue = getNextValue(entity, path.Property);

            PatchOperationPropertyPath currentProperty = path;
            while (properties.Any())
            {
                var prop = properties.Pop();
                currentProperty.Next = prop;

                currentProperty = prop;
                currentValue = getNextValue(currentValue, prop.Property);
            }

            return new PatchOperation(path, PatchOperationTypes.replace, currentValue);
        }

        public PatchOperation(PatchOperationPropertyPath propertyPath, PatchOperationTypes operation, object? value)
        {
            PropertyPath = propertyPath;
            Operation = operation;
            Value = value;
        }

        public override string ToString()
        {
            return PropertyPath?.ToString() + ": " + Value;
        }
    }

    public static class PatchOperation<T> where T : class
    {
        public static PatchOperation Create<Y>(Expression<Func<T, Y>> field, Y value)
        {
            var operation = PatchOperation.Create(default, field);
            operation.Value = value;

            return operation;
        }
    }

    public static class PatchOperationExtensions
    {
        public static IEnumerable<PatchOperation> ToPatchOperations(this object model)
        {
            return (model == null) ? Enumerable.Empty<PatchOperation>() : BuildFromObject(model, new Stack<PropertyInfo>());
        }
        private static IEnumerable<PatchOperation> BuildFromObject(object model, Stack<PropertyInfo> stack)
        {
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
                    foreach (var op in BuildFromObject(value ?? Activator.CreateInstance(property.PropertyType), stack))
                    {
                        yield return op;
                    }
                }
                else
                {
                    var reversedStack = stack.Reverse().GetEnumerator();
                    reversedStack.MoveNext();
                    PatchOperationPropertyPath propertyTree = new PatchOperationPropertyPath(reversedStack.Current);
                    var currentBranch = propertyTree;
                    while (reversedStack.MoveNext())
                    {
                        currentBranch.Next = new PatchOperationPropertyPath(reversedStack.Current);
                        currentBranch = currentBranch.Next;
                    }

                    yield return new PatchOperation(propertyTree, PatchOperationTypes.replace, value);
                }

                stack.Pop();
            }
        }
    }
}
