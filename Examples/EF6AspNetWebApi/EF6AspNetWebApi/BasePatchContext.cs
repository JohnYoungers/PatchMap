using EF6AspNetWebApi.Data;
using PatchMap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi
{
    public class BasePatchContext
    {
        public List<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
        public ExampleContext DbContext { get; set; }
        public bool IsNew { get; set; }

        public void AddValidationResult(PatchOperation operation, string message)
        {
            AddValidationResult(message, operation.PropertyTree.ToString());
        }

        public void AddValidationResult(string message, params string[] properties)
        {
            ValidationResults.Add(new ValidationResult(message, properties));
        }
    }
}
