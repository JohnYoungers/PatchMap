using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Web.Tests.Contexts
{
    public class AppFeatureContext
    {
        private readonly Dictionary<string, string> placeholders = new Dictionary<string, string>();
        public Dictionary<string, object> LookUp { get; set; } = new Dictionary<string, object>();

        public string PlaceholderValue(string key)
        {
            return placeholders["{" + key + "}"];
        }
        public void AddPlaceholderValue(string key, string value)
        {
            placeholders.Add("{" + key + "}", value);
        }
        public string PopulatePlaceholders(string text)
        {
            foreach (var kvp in placeholders)
            {
                text = text.Replace(kvp.Key, kvp.Value);
            }

            var remainingKeys = new Regex("{.*?}").Match(text);
            if (remainingKeys.Success)
            {
                Application.LogDebug<AppFeatureContext>("Potential missing key in context: " + remainingKeys.Value);
            }

            return text;
        }
    }
}
