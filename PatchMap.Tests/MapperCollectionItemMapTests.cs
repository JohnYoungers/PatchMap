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
    public class MapperCollectionItemMapTests
    {
        Mapper<SampleViewModel, SampleEntity, SampleContext> mapper;
        SampleViewModel source;
        SampleEntity target;
        int fullCollectionChanged;
        int itemChanged;

        [TestInitialize]
        public void BeforeEach()
        {
            mapper = new Mapper<SampleViewModel, SampleEntity, SampleContext>();
            source = new SampleViewModel();
            target = new SampleEntity();
            fullCollectionChanged = 0;
            itemChanged = 0;

            mapper.AddMap(vm => vm.StringCollection).HasPostMap((SampleEntity t, SampleContext ctx, PatchOperation update) =>
            {
                fullCollectionChanged++;
            });
            mapper.AddMap(vm => vm.StringCollection).IsCollectionItem().HasPostMap((SampleEntity t, SampleContext ctx, PatchOperation update) =>
            {
                itemChanged++;
            });
        }

        [TestMethod]
        public void MapsFullCollection()
        {
            var patch = new JsonPatch
            {
                path = "StringCollection"
            };
            var operations = (new[] { patch }).ToPatchOperations<SampleViewModel>();
            mapper.Map(operations, target, new SampleContext());
            Assert.AreEqual(1, fullCollectionChanged);
            Assert.AreEqual(0, itemChanged);
        }

        [TestMethod]
        public void MapsCollectionItem()
        {
            var patch = new JsonPatch
            {
                path = "StringCollection/ABC123"
            };
            var operations = (new[] { patch }).ToPatchOperations<SampleViewModel>();
            mapper.Map(operations, target, new SampleContext());
            Assert.AreEqual(0, fullCollectionChanged);
            Assert.AreEqual(1, itemChanged);
        }
    }
}
