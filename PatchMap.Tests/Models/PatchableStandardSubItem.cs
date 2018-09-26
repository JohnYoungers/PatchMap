using PatchMap.Attributes;

namespace PatchMap.Tests.Models
{
    public class PatchableStandardSubItem
    {
        public string Code { get; set; }
        public string Description { get; set; }

        [PatchRecursively]
        public PatchableCircularReferenceItem Child { get; set; }
    }
}
