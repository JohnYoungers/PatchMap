using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap.Mapping;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PatchMap.Tests
{
    [TestClass]
    public class MapperCompoundMapTests
    {
        Mapper<SampleViewModel, SampleEntity, SampleContext> mapper;
        SampleViewModel source;
        SampleEntity target;
        int anyCompoundItemChanged;
        int addressChanged;

        [TestInitialize]
        public void BeforeEach()
        {
            mapper = new Mapper<SampleViewModel, SampleEntity, SampleContext>();
            source = new SampleViewModel();
            target = new SampleEntity();
            anyCompoundItemChanged = 0;
            addressChanged = 0;

            mapper.AddCompoundMap(cm =>
            {
                cm.AddMap(vm => vm.Id, db => db.Id);
                cm.AddMap(vm => vm.MultiWordProperty, db => db.Multiwordproperty);
                cm.AddCompoundMap(cm2 =>
                {
                    cm2.AddMap(vm => vm.Address.AddressLine1, db => db.AddressLine1);
                    cm2.AddMap(vm => vm.Address.City, db => db.AddressCity);
                }, (t, ctx) =>
                {
                    addressChanged++;
                });
            }, (t, ctx) =>
            {
                anyCompoundItemChanged++;
            });
        }

        [TestMethod]
        public void PostMapOnlyOnChanges()
        {
            mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(0, anyCompoundItemChanged);
            Assert.AreEqual(0, addressChanged);
        }

        [TestMethod]
        public void Level1Change()
        {
            source.Id = 5;
            mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(1, anyCompoundItemChanged);
            Assert.AreEqual(0, addressChanged);
        }

        [TestMethod]
        public void Level2Change()
        {
            source.Address = new AddressViewModel { AddressLine1 = "A" };
            mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(1, anyCompoundItemChanged);
            Assert.AreEqual(1, addressChanged);
        }
    }
}
