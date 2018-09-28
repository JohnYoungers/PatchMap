using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Mapping
{
    public class FieldMapConversionResult<T>
    {
        public T Value { get; set; }
        public string FailureReason { get; set; }
        public bool Succeeded { get => string.IsNullOrEmpty(FailureReason); }
    }
}
