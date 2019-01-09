using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Tests
{
    [TestClass]
    public class MapperTypeTests
    {
        [TestMethod]
        public void BuiltFromInterface()
        {
            var mapper = new Mapper<IInterfaceExample, InterfaceImplementation, SampleContext>();
            mapper.AddMap(vm => vm.FieldA, db => db.FieldA);
            mapper.AddMap(vm => vm.Address.AddressLine1, db => db.Address1);

            var source = new InterfaceImplementationViewModel
            {
                FieldA = "A",
                Address = new AddressViewModel { AddressLine1 = "B" }
            };

            var target = new InterfaceImplementation();
            mapper.Map(source.ToPatchOperations(), target, new SampleContext());

            Assert.AreEqual("A", target.FieldA);
            Assert.AreEqual("B", target.Address1);
        }
    }
}
