using EF6AspNetWebApi.Data;
using PatchMap;
using PatchMap.Mapping;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi
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
                    return !ExampleContextMetadata.Tables[targetProperty.DeclaringType][targetProperty.Name].Nullable;
                },
                MapTargetValueIsValid = (TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation, object value) =>
                {
                    if (value != null)
                    {
                        var targetProperty = map.TargetField.Last();
                        var columnDefinition = ExampleContextMetadata.Tables[targetProperty.DeclaringType][targetProperty.Name];

                        var valueType = value.GetType();
                        if (valueType == typeof(string) && columnDefinition.MaxLength.HasValue && (value as string).Length > columnDefinition.MaxLength)
                        {
                            return new FieldMapValueValidResult { FailureReason = $"length must be equal to or less than {columnDefinition.MaxLength} characters" };
                        }
                        if ((valueType == typeof(DateTimeOffset) || valueType == typeof(DateTimeOffset?)) && (value as DateTimeOffset?) < SqlDateTime.MinValue.Value)
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

        protected PatchCommandResult<T> GeneratePatchResult<T>(MapResult<TTarget, TContext> mapResult, Func<(bool isNew, string id, T entity)> onSuccess)
        {
            if (mapResult.Succeeded && !mapResult.Context.ValidationResults.Any())
            {
                var (isNew, id, entity) = onSuccess();
                return new PatchCommandResult<T> { IsNew = isNew, EntityId = id, Entity = entity };
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
