using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EFCoreWebApi.Tests
{
    public static class PatchCommandResultTestExtensions
    {
        public static void AssertHasValidationResult<T>(this PatchCommandResult<T> result, string property, string message)
        {
            var matched = result.ValidationResults.Any(r => string.Equals(r.ErrorMessage, message) && r.MemberNames.Any(m => string.Equals(m, property)));
            if (!matched)
            {
                Debug.WriteLine("Actual Validation Results:");
                foreach (var vr in result.ValidationResults)
                {
                    Debug.WriteLine($"{string.Join(',', vr.MemberNames)}: {vr.ErrorMessage}");
                }
            }

            Assert.IsTrue(matched, $"Validation Result of '{property}: {message}' was not found.  Actual validations logged to output window");
        }
    }
}
