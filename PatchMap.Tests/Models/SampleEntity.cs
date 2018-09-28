using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Tests.Models
{
    public class SampleEntity
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Multiwordproperty { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressCity { get; set; }

        public SampleAssociatedEntity AssociatedEntity { get; set; }
    }

    public class SampleAssociatedEntity
    {
        public int SampleEntityId { get; set; }
        public string Name { get; set; }
    }
}
