using PatchMap.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchMap.Tests.Models
{
    public class PatchableItem
    {
        public int IntValue { get; set; }
        public int? NullableIntValue { get; set; }
        public Guid? NullableGuidValue { get; set; }
        public string StringValue { get; set; }

        [PatchIgnore]
        public string ThisShouldNotGenerateAPatch { get; set; }

        public List<string> StringList { get; set; } = new List<string>();

        public List<PatchableCircularReferenceItem> SubItems { get; set; } = new List<PatchableCircularReferenceItem>();

        [PatchRecursively]
        public PatchableStandardSubItem CantUpdateInOnePatch { get; set; }

        public PatchableCircularReferenceItem MustUpdateInOnePatch { get; set; }

        public List<PatchableCircularReferenceItem> ListOfMustUpdateInOnePatch { get; set; } = new List<PatchableCircularReferenceItem>();
    }
}
