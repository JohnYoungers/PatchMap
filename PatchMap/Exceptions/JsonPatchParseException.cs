using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PatchMap.Exceptions
{
    [Serializable]
    public class JsonPatchParseException : Exception
    {
        public JsonPatch Patch { get; set; }

        public JsonPatchParseException(JsonPatch patch, string message) : base(message)
        {
            Patch = patch;
        }

        protected JsonPatchParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Patch = (JsonPatch)info.GetValue(nameof(Patch), typeof(JsonPatch));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Patch), Patch);
            base.GetObjectData(info, context);
        }
    }
}
