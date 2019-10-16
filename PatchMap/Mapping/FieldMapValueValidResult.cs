using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Mapping
{
    public class FieldMapValueValidResult
    {
        public bool IsValid { get; set; }
        public string? FailureReason { get; set; }
    }
}
