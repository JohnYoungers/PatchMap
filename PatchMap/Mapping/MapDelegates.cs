using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Mapping
{
    public delegate bool MapTargetIsRequiredMethod<TTarget, TContext>(TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation);
    public delegate FieldMapValueValidResult MapTargetValueIsValidMethod<TTarget, TContext>(TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation, object? value);
    public delegate bool MapTargetHasChangedMethod<TTarget, TContext>(TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation, object? originalValue, object? newValue);
    public delegate void PostMapMethod<in TTarget, in TContext>(TTarget target, TContext ctx);
}
