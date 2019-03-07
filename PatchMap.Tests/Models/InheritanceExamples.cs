using PatchMap.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Tests.Models
{
    public interface IInterfaceExample
    {
        string FieldA { get; set; }
        AddressViewModel Address { get; set; }
    }

    public class InterfaceBaseImplementationViewModel
    {
        public string FieldA { get; set; }
    }

    public class InterfaceImplementationViewModel : InterfaceBaseImplementationViewModel, IInterfaceExample
    {
        [PatchRecursively]
        public AddressViewModel Address { get; set; }
    }

    public class InterfaceImplementation
    {
        public string FieldA { get; set; }
        public string Address1 { get; set; }
    }
}
