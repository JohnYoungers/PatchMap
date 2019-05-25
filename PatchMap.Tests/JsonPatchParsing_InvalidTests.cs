using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap.Exceptions;
using PatchMap.Tests.Models;
using System.Collections.Generic;

namespace PatchMap.Tests
{
    [TestClass]
    public class JsonPatchParsing_InvalidTests
    {
        [TestMethod]
        public void JsonPatchParseExceptionSerializes()
        {
            var ex = new JsonPatchParseException(new JsonPatch { path = "A/B/C" }, "My Message");

            string exceptionToString = ex.ToString();

            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var ms = new System.IO.MemoryStream())
            {
                bf.Serialize(ms, ex);
                ms.Seek(0, 0);
                ex = (JsonPatchParseException)bf.Deserialize(ms);
            }

            Assert.AreEqual(exceptionToString, ex.ToString());
            Assert.AreEqual("A/B/C", ex.Patch.path);
        }

        [TestMethod]
        public void SimpleValueCannotBeParsed()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "intvalue", value = "wrong" },
                new JsonPatch { op = PatchOperationTypes.replace, path = "MustUpdateInOnePatch", value = "wrong" },
                new JsonPatch { op = PatchOperationTypes.replace, path = "intvalue", value = long.MaxValue },
                new JsonPatch { op = PatchOperationTypes.replace, path = "intvalue", value = null },
                new JsonPatch { op = PatchOperationTypes.replace, path = "NullableGuidValue", value = "abc" }
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(false, operations[0].JsonPatchValueParsed);
            Assert.AreEqual(patches[0], operations[0].JsonPatch);
            Assert.AreEqual(false, operations[1].JsonPatchValueParsed);
            Assert.AreEqual(false, operations[2].JsonPatchValueParsed);
            Assert.AreEqual(false, operations[3].JsonPatchValueParsed);
            Assert.AreEqual(false, operations[4].JsonPatchValueParsed);
        }

        [TestMethod]
        public void InvalidPathThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "badpathvalue", value = null }
            };

            var ex = Assert.ThrowsException<JsonPatchParseException>(() => patches.ToPatchOperations<PatchableItem>());
            Assert.AreEqual("PatchableItem does not contain a property named badpathvalue", ex.Message);
        }

        [TestMethod]
        public void AddOperationOnNonCollectionThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.add, path = "intvalue", value = null }
            };

            var ex = Assert.ThrowsException<JsonPatchParseException>(() => patches.ToPatchOperations<PatchableItem>());
            Assert.AreEqual("add operation is only valid on collections", ex.Message);
        }

        [TestMethod]
        public void RemoveOperationOnNonCollectionThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.remove, path = "intvalue" }
            };

            var ex = Assert.ThrowsException<JsonPatchParseException>(() => patches.ToPatchOperations<PatchableItem>());
            Assert.AreEqual("remove operation is only valid on collections", ex.Message);
        }

        [TestMethod]
        public void InvalidCollectionPathMissingKeyThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.remove, path = "subitems", value = null }
            };

            var ex = Assert.ThrowsException<JsonPatchParseException>(() => patches.ToPatchOperations<PatchableItem>());
            Assert.AreEqual("remove operation is invalid on SubItems without the collection item's key", ex.Message);
        }

        [TestMethod]
        public void RemoveOperationShouldNotContainValue()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.remove, path = "subitems/a", value = new PatchableCircularReferenceItem() }
            };

            var ex = Assert.ThrowsException<JsonPatchParseException>(() => patches.ToPatchOperations<PatchableItem>());
            Assert.AreEqual("remove operation should not contain a value", ex.Message);
        }

        [TestMethod]
        public void PropertyMarkedMemberSerializationThrowsException()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "CantUpdateInOnePatch", value = new PatchableCircularReferenceItem() },
            };

            var ex = Assert.ThrowsException<JsonPatchParseException>(() => patches.ToPatchOperations<PatchableItem>());
            Assert.AreEqual("CantUpdateInOnePatch can not be updated in one patch", ex.Message);
        }

        [TestMethod]
        public void PathParsesAsIndividualPiecesOperations()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "MustUpdateInOnePatch/Code", value = "A" }
            };

            var ex = Assert.ThrowsException<JsonPatchParseException>(() => patches.ToPatchOperations<PatchableItem>());
            Assert.AreEqual("MustUpdateInOnePatch can only be updated in one patch", ex.Message);
        }
    }
}
