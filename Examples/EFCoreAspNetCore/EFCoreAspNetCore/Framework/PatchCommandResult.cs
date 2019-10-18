using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace EFCoreAspNetCore.Framework
{
    public class PatchCommandResult
    {
        public bool IsNew { get; set; }
        public string EntityId { get; set; }
        public List<ValidationResult> ValidationResults { get; set; }
        public virtual bool Succeeded => ValidationResults?.Any() != true;

        public PatchCommandResult() { }
        public PatchCommandResult(bool isNew, string entityId)
        {
            IsNew = isNew;
            EntityId = entityId;
        }

        public virtual object GetEntity() => null;
    }

    public class PatchCommandResult<T> : PatchCommandResult
    {
        public T Entity { get; set; }

        public PatchCommandResult() { }
        public PatchCommandResult(bool isNew, string entityId, T entity) : base(isNew, entityId)
        {
            Entity = entity;
        }

        public override object GetEntity() => Entity;
    }
}
