using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Mapping
{
    public enum MapResultFailureType
    {
        JsonPatchValueNotParsable,
        ValueConversionFailed,
        Required,
        ValueIsNotValid
    }

    public class MapResultFailure<TTarget, TContext>
    {
        public FieldMap<TTarget, TContext> Map { get; set; }
        public PatchOperation PatchOperation { get; set; }
        public MapResultFailureType FailureType { get; set; }
        public string? Reason { get; set; }

        public MapResultFailure(FieldMap<TTarget, TContext> map, PatchOperation patchOperation, MapResultFailureType failureType, string? reason)
        {
            Map = map;
            PatchOperation = patchOperation;
            FailureType = failureType;
            Reason = reason;
        }
    }
}
