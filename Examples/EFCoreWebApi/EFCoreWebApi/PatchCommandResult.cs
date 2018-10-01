using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EFCoreWebApi
{
    public class PatchCommandResult<T>
    {
        public List<ValidationResult> ValidationResults { get; set; }
        public T Entity { get; set; }
    }
}
