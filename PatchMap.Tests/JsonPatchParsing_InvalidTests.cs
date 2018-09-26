using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchMap.Tests
{
    [TestClass]
    public class JsonPatchParsing_InvalidTests
    {
        [TestMethod]
        public void SimpleValueCannotBeParsed()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "intvalue", value = "wrong" },
                new JsonPatch { op = PatchOperationTypes.replace, path = "MustUpdateInOnePatch", value = "wrong" },
                new JsonPatch { op = PatchOperationTypes.replace, path = "intvalue", value = long.MaxValue },
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(false, operations[0].JsonPatchValueParsed);
            Assert.AreEqual(patches[0], operations[0].JsonPatch);
            Assert.AreEqual(false, operations[1].JsonPatchValueParsed);
            Assert.AreEqual(false, operations[2].JsonPatchValueParsed);
        }

        [TestMethod]
        public void InvalidPathThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "badpathvalue", value = null }
            };

            Assert.ThrowsException<ArgumentException>(() => patches.ToPatchOperations<PatchableItem>());
        }

        [TestMethod]
        public void AddOperationOnNonCollectionThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.add, path = "intvalue", value = null }
            };

            Assert.ThrowsException<ArgumentException>(() => patches.ToPatchOperations<PatchableItem>());
        }

        [TestMethod]
        public void RemoveOperationOnNonCollectionThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.remove, path = "intvalue" }
            };

            Assert.ThrowsException<ArgumentException>(() => patches.ToPatchOperations<PatchableItem>());
        }

        [TestMethod]
        public void InvalidCollectionPathMissingKeyThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.remove, path = "subitems", value = null }
            };

            Assert.ThrowsException<ArgumentException>(() => patches.ToPatchOperations<PatchableItem>());
        }

        [TestMethod]
        public void RemoveOperationShouldNotContainValue()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.remove, path = "subitems/a", value = new PatchableCircularReferenceItem() }
            };

            Assert.ThrowsException<ArgumentException>(() => patches.ToPatchOperations<PatchableItem>());
        }

        [TestMethod]
        public void PropertyMarkedMemberSerializationThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "CantUpdateInOnePatch", value = new PatchableCircularReferenceItem() },
            };

            Assert.ThrowsException<ArgumentException>(() => patches.ToPatchOperations<PatchableItem>());
        }

        [TestMethod]
        public void PathParsesAsIndividualPiecesOperations()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "MustUpdateInOnePatch/Code", value = "A" }
            };

            Assert.ThrowsException<ArgumentException>(() => patches.ToPatchOperations<PatchableItem>());
        }
    }
}
