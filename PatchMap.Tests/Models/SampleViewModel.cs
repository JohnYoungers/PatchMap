using PatchMap.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Tests.Models
{
    public class SampleSummaryViewModel
    {
        public int Id { get; set; }
        public string MultiWordProperty { get; set; }
        public string AssociatedEntityName { get; set; }

        public List<string> StringCollection { get; set; } = new List<string>();
    }

    public class SampleViewModel : SampleSummaryViewModel
    {
        public SampleSummaryViewModel Parent { get; set; }

        [PatchRecursively]
        public AddressViewModel Address { get; set; }
    }
}
