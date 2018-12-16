using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using EFCoreAspNetCore.Data;
using PatchMap;
using PatchMap.Mapping;

namespace EFCoreAspNetCore
{
    public abstract class PatchCommandBase<TSource, TTarget, TContext> : CommandBase
        where TSource : class
        where TTarget : class
        where TContext : PatchContextBase, new()
    {
        protected static readonly Mapper<TSource, TTarget, TContext> mapper = new Mapper<TSource, TTarget, TContext>();

        static PatchCommandBase()
        {
            mapper.MapTargetHasChanged = (TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation, object originalValue, object newValue) =>
            {
                return ctx.IsNew || !Equals(originalValue, newValue);
            };
            mapper.MapTargetIsRequired = (TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation) =>
            {
                var targetProperty = map.TargetField.Last();
                return !ExampleContextMetadata.Tables[targetProperty.DeclaringType][targetProperty.Name].Nullable;
            };
            mapper.MapTargetValueIsValid = (TTarget target, TContext ctx, FieldMap<TTarget, TContext> map, PatchOperation operation, object value) =>
            {
                if (value != null)
                {
                    var targetProperty = map.TargetField.Last();
                    var columnDefinition = ExampleContextMetadata.Tables[targetProperty.DeclaringType][targetProperty.Name];

                    if (columnDefinition.ClrType == typeof(string) && columnDefinition.MaxLength.HasValue && (value as string).Length > columnDefinition.MaxLength)
                    {
                        return new FieldMapValueValidResult { FailureReason = $"length must be equal to or less than {columnDefinition.MaxLength} characters" };
                    }
                    if ((columnDefinition.ClrType == typeof(DateTimeOffset) || columnDefinition.ClrType == typeof(DateTimeOffset?))
                         && (value as DateTimeOffset?) < SqlDateTime.MinValue.Value)
                    {
                        return new FieldMapValueValidResult { FailureReason = $"must be on or after {SqlDateTime.MinValue.Value.ToShortDateString()}" };
                    }
                }

                return new FieldMapValueValidResult { IsValid = true };
            };
        }

        public PatchCommandBase(ExampleContext dbContext) : base(dbContext) { }

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

        protected PatchCommandResult<T> GeneratePatchResult<T>(TTarget dbItem, MapResult<TTarget, TContext> mapResult, Func<(bool isNew, string id, T entity)> onSuccess)
        {
            if (mapResult.Succeeded && !mapResult.Context.ValidationResults.Any())
            {
                var (isNew, id, entity) = onSuccess();
                return new PatchCommandResult<T> { IsNew = isNew, EntityId = id, Entity = entity };
            }
            else
            {
                return GenerateValidationCommandResult<T>(dbItem, mapResult);
            }
        }

        protected async Task<PatchCommandResult<T>> GeneratePatchResultAsync<T>(TTarget dbItem, MapResult<TTarget, TContext> mapResult, Func<Task<(bool isNew, string id, T entity)>> onSuccess)
        {
            if (mapResult.Succeeded && !mapResult.Context.ValidationResults.Any())
            {
                var (isNew, id, entity) = await onSuccess().ConfigureAwait(false);
                return new PatchCommandResult<T> { IsNew = isNew, EntityId = id, Entity = entity };
            }
            else
            {
                return GenerateValidationCommandResult<T>(dbItem, mapResult);
            }
        }

        private PatchCommandResult<T> GenerateValidationCommandResult<T>(TTarget dbItem, MapResult<TTarget, TContext> mapResult)
        {
            foreach (var f in mapResult.Failures)
            {
                var label = f.Map.GenerateLabel(dbItem, mapResult.Context);
                switch (f.FailureType)
                {
                    case MapResultFailureType.Required:
                        mapResult.Context.AddValidationResult(f.PatchOperation, $"{label} is required");
                        break;
                    case MapResultFailureType.JsonPatchValueNotParsable:
                        mapResult.Context.AddValidationResult(f.PatchOperation, $"{f.PatchOperation.JsonPatch.value} is not a valid value for ${label}");
                        break;
                    default:
                        mapResult.Context.AddValidationResult(f.PatchOperation, $"{label} {f.Reason}");
                        break;
                }
            }

            return new PatchCommandResult<T> { ValidationResults = mapResult.Context.ValidationResults };
        }
    }
}
