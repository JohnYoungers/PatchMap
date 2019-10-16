using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Mapping
{
    public class FieldMapConversionResult<T>
    {
        public T Value { get; set; } = default!;  // TODO: find a way to indicate this is nullable
        public string? FailureReason { get; set; }
        public bool Succeeded { get => FailureReason == null; }
    }
}
