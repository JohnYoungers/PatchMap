using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Mapping
{
    public delegate FieldMapConversionResult<object> ConversionMethod<in TTarget, in TContext>(TTarget target, TContext ctx, object value);
    public delegate FieldMapConversionResult<TTargetProp> ConversionMethod<in TTarget, in TContext, in TSourceProp, TTargetProp>(TTarget target, TContext ctx, TSourceProp value);
    public delegate void FieldPostMapMethod<TTarget, TContext>(TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation);

    public delegate string LabelMethod<in TTarget, in TContext>(TTarget target, TContext ctx);
    public delegate bool EnabledMethod<in TTarget, in TContext>(TTarget target, TContext ctx);
    public delegate bool RequiredMethod<in TTarget, in TContext>(TTarget target, TContext ctx);

}
