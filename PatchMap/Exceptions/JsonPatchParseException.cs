using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Exceptions
{
    public class JsonPatchParseException : Exception
    {
        public JsonPatch Patch { get; set; }

        public JsonPatchParseException(JsonPatch patch, string message) : base(message)
        {
            Patch = patch;
        }
    }
}
