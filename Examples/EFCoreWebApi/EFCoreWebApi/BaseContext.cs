using EFCoreWebApi.Data;
using PatchMap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EFCoreWebApi
{
    public class BaseContext
    {
        public List<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
        public ExampleContext DbContext { get; set; }
        public bool IsNew { get; set; }

        public void AddValidationResult(PatchOperation operation, string message)
        {
            ValidationResults.Add(new ValidationResult(message, new[] { operation.PropertyTree.ToString() }));
        }
    }
}
