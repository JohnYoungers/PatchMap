using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace EFCoreWebApi
{
    public class PatchCommandResult
    {
        public bool IsNew { get; set; }
        public string EntityId { get; set; }
        public List<ValidationResult> ValidationResults { get; set; }

        public bool Succeeded => ValidationResults?.Any() != true;
        public virtual object GetEntity() => null;
    }

    public class PatchCommandResult<T> : PatchCommandResult
    {
        public T Entity { get; set; }

        public override object GetEntity() => Entity;
    }
}
