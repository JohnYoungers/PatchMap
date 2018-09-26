using System;
using System.Reflection;

namespace PatchMap
{
    public class PatchOperationPropertyTree
    {
        public PropertyInfo Property { get; set; }
        public string CollectionKey { get; set; }

        public PatchOperationPropertyTree Next { get; set; }

        public override string ToString()
        {
            return Property?.Name
                + (!String.IsNullOrEmpty(CollectionKey) ? $"[{CollectionKey}]" : "")
                + ((Next != null) ? "/" + Next : "");
        }
    }
}
