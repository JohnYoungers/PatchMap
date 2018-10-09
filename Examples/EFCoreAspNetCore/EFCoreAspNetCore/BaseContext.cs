using EFCoreAspNetCore.Data;
using PatchMap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EFCoreAspNetCore
{
    public class BaseContext
    {
        public List<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
        public ExampleContext DbContext { get; set; }
        public bool IsNew { get; set; }

        public void AddValidationResult(PatchOperation operation, string message)
        {
            AddValidationResult(operation.PropertyTree.ToString(), message);
        }

        public void AddValidationResult(string property, string message)
        {
            ValidationResults.Add(new ValidationResult(message, new[] { property }));
        }
    }
}
