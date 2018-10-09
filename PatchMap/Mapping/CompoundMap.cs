using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PatchMap.Mapping
{
    public delegate void PostMapMethod<in TTarget, in TContext>(TTarget target, TContext ctx);

    public class CompoundMap<TSource, TTarget, TContext> : Map<TTarget, TContext>
    {
        protected internal PostMapMethod<TTarget, TContext> PostMap { get; protected set; }

        public List<Map<TTarget, TContext>> Mappings { get; set; } = new List<Map<TTarget, TContext>>();

        public FieldMap<TTarget, TContext, TSourceProp, object> AddMap<TSourceProp>(Expression<Func<TSource, TSourceProp>> sourceFieldExp)
        {
            return AddMap<TSourceProp, object>(sourceFieldExp, null);
        }

        public FieldMap<TTarget, TContext, TSourceProp, TTargetProp> AddMap<TSourceProp, TTargetProp>(Expression<Func<TSource, TSourceProp>> sourceFieldExp, Expression<Func<TTarget, TTargetProp>> targetFieldExp)
        {
            var map = new FieldMap<TTarget, TContext, TSourceProp, TTargetProp>(sourceFieldExp, targetFieldExp);
            Mappings.Add(map);

            return map;
        }

        public CompoundMap<TSource, TTarget, TContext> AddCompoundMap(Action<CompoundMap<TSource, TTarget, TContext>> configuration, PostMapMethod<TTarget, TContext> postMap)
        {
            var map = new CompoundMap<TSource, TTarget, TContext>
            {
                PostMap = postMap ?? throw new ArgumentException($"{nameof(postMap)} is required, otherwise the maps can be flattened without needing a CompoundMap")
            };
            Mappings.Add(map);

            if (configuration == null)
            {
                throw new ArgumentException($"{nameof(configuration)} is required to specify what maps are defined in the CompoundMap");
            }
            else
            {
                configuration(map);
            }

            return map;
        }

        public CompoundMap<TSource, TTarget, TContext> HasPostMap(PostMapMethod<TTarget, TContext> postMap)
        {
            PostMap = postMap;
            return this;
        }
    }
}
