using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using EFCoreWebApi.Data;
using PatchMap;
using PatchMap.Mapping;

namespace EFCoreWebApi
{
    public abstract class BasePatchCommand<TSource, TTarget, TContext> : BaseCommand
        where TSource : class
        where TTarget : class
        where TContext : BaseContext, new()
    {
        protected static Mapper<TSource, TTarget, TContext> mapper;

        public BasePatchCommand(ExampleContext dbContext) : base(dbContext) { }

        protected static void InitializeMapper()
        {
            mapper = new Mapper<TSource, TTarget, TContext>
            {
                MapTargetHasChanged = (TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation, object originalValue, object newValue) =>
                {
                    return ctx.IsNew || !Equals(originalValue, newValue);
                },
                MapTargetIsRequired = (TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation) =>
                {
                    var targetProperty = map.TargetField.Last();
                    var entityDefinition = ctx.DbContext.Model.FindEntityType(targetProperty.DeclaringType.FullName.ToString());
                    var propertyDefinition = entityDefinition.FindProperty(targetProperty.Name);

                    return !propertyDefinition.IsNullable;
                },
                MapTargetValueIsValid = (TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation, object value) =>
                {
                    if (value != null)
                    {
                        var targetProperty = map.TargetField.Last();
                        var entityDefinition = ctx.DbContext.Model.FindEntityType(targetProperty.DeclaringType.FullName.ToString());
                        var propertyDefinition = entityDefinition.FindProperty(targetProperty.Name);

                        if (propertyDefinition.ClrType == typeof(string))
                        {
                            var maxLength = propertyDefinition.FindAnnotation("MaxLength")?.Value as int?;
                            if (maxLength.HasValue && (value as string).Length > maxLength)
                            {
                                return new FieldMapValueValidResult { FailureReason = $"length must be equal to or less than {maxLength} characters" };
                            }
                        }
                        if ((propertyDefinition.ClrType == typeof(DateTimeOffset) || propertyDefinition.ClrType == typeof(DateTimeOffset?))
                             && (value as DateTimeOffset?) < SqlDateTime.MinValue.Value)
                        {
                            return new FieldMapValueValidResult { FailureReason = $"must be on or after {SqlDateTime.MinValue.Value.ToShortDateString()}" };
                        }
                    }

                    return new FieldMapValueValidResult { IsValid = true };
                }
            };
        }

        protected (TTarget dbItem, bool isNew) GetEntity<TId>(TId id, Func<IEnumerable<TTarget>> entitySearch, Func<TTarget> newFactory)
        {
            var isNew = Equals(id, default(TId));
            return ((isNew) ? newFactory() : EnsureExists(entitySearch().FirstOrDefault()), isNew);
        }

        protected TContext GenerateContext(bool isNew)
        {
            return new TContext
            {
                DbContext = DbContext,
                IsNew = isNew
            };
        }

        protected PatchCommandResult<T> GeneratePatchResult<T>(MapResult<TTarget, TContext> mapResult, Func<T> onSuccess)
        {
            if (mapResult.Succeeded && !mapResult.Context.ValidationResults.Any())
            {
                return new PatchCommandResult<T> { Entity = onSuccess() };
            }
            else
            {
                foreach (var f in mapResult.Failures)
                {
                    switch (f.FailureType)
                    {
                        case MapResultFailureType.Required:
                            mapResult.Context.AddValidationResult(f.PatchOperation, $"{f.Map.Label} is required");
                            break;
                        case MapResultFailureType.JsonPatchValueNotParsable:
                            mapResult.Context.AddValidationResult(f.PatchOperation, $"{f.PatchOperation.JsonPatch.value} is not a valid value for ${f.Map.Label}");
                            break;
                        default:
                            mapResult.Context.AddValidationResult(f.PatchOperation, $"{f.Map.Label} {f.Reason}");
                            break;
                    }
                }

                return new PatchCommandResult<T> { ValidationResults = mapResult.Context.ValidationResults };
            }
        }
    }
}
