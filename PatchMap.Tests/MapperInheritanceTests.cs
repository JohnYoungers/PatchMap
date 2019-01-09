using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Tests
{
    [TestClass]
    public class MapperInheritanceTests
    {
        Mapper<IInterfaceExample, InterfaceImplementation, SampleContext> mapper;

        [TestInitialize]
        public void BeforeEach()
        {
            mapper = new Mapper<IInterfaceExample, InterfaceImplementation, SampleContext>();
            mapper.AddMap(vm => vm.FieldA, db => db.FieldA);
            mapper.AddMap(vm => vm.Address.AddressLine1, db => db.Address1);
        }

        [TestMethod]
        public void FromSource()
        {
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

        [TestMethod]
        public void FromValue()
        {
            var operations = new[]
            {
                PatchOperation<InterfaceImplementationViewModel>.Create(i => i.FieldA, "A"),
                PatchOperation<InterfaceImplementationViewModel>.Create(i => i.Address.AddressLine1, "B"),
            };
            var target = new InterfaceImplementation();
            mapper.Map(operations, target, new SampleContext());

            Assert.AreEqual("A", target.FieldA);
            Assert.AreEqual("B", target.Address1);
        }
    }
}
