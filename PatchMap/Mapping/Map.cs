using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Mapping
{
    public abstract class Map<TTarget, TContext>
    {
        public PostMapMethod<TTarget, TContext> PostMap { get; set; }
    }
}
