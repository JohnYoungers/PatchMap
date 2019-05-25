using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap
{
    public class MapConfiguration
    {
        /// <summary>
        /// If true, JsonPatchParseException will not be thrown if an operation does not have a corresponding Map
        /// </summary>
        public bool AllowUnmappedOperations { get; set; }
    }
}
