using System;
using System.Reflection;

namespace PatchMap
{
    public class PatchOperationPropertyPath
    {
        public PropertyInfo Property { get; set; }
        public string? CollectionKey { get; set; }
        public PatchOperationPropertyPath? Next { get; set; }

        public PatchOperationPropertyPath(PropertyInfo property)
        {
            Property = property;
        }

        public override string ToString()
        {
            return Property.Name
                + (!string.IsNullOrEmpty(CollectionKey) ? $"[{CollectionKey}]" : "")
                + ((Next != null) ? "/" + Next : "");
        }
    }
}
