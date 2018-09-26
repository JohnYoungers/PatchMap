using PatchMap.Attributes;
using System.Collections.Generic;

namespace PatchMap.Tests.Models
{
    public class PatchableCircularReferenceItem
    {
        public string Code { get; set; }
        public string Description { get; set; }

        public List<PatchableCircularReferenceItem> Children { get; set; } = new List<PatchableCircularReferenceItem>();
    }
}
