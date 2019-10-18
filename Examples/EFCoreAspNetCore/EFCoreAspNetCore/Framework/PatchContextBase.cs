using EFCoreAspNetCore.Data;
using PatchMap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EFCoreAspNetCore.Framework
{
    public class PatchContextBase
    {
        public List<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
        public ExampleContext DbContext { get; set; }
        public bool IsNew { get; set; }

        public void AddValidationResult(PatchOperation operation, string message)
        {
            AddValidationResult(message, operation.PropertyPath.ToString());
        }

        public void AddValidationResult(string message, params string[] properties)
        {
            ValidationResults.Add(new ValidationResult(message, properties));
        }
    }
}
