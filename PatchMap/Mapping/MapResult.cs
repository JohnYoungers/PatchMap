using System.Collections.Generic;
using System.Linq;

namespace PatchMap.Mapping
{
    public class MapResult<TTarget, TContext>
    {
        public TContext Context { get; set; }
        public List<MapResultFailure<TTarget, TContext>> Failures { get; set; } = new List<MapResultFailure<TTarget, TContext>>();

        public MapResult(TContext context)
        {
            Context = context;
        }

        internal void AddFailure(FieldMap<TTarget, TContext> map, PatchOperation patchOperation, MapResultFailureType failureType, string? reason = null)
        {
            Failures.Add(new MapResultFailure<TTarget, TContext>(map, patchOperation, failureType, reason));
        }

        public bool Succeeded { get => !Failures.Any(); }
    }
}
