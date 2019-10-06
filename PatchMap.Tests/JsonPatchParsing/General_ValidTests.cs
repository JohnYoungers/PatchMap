using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Tests.JsonPatchParsing
{
    public abstract class General_ValidTests
    {
        [TestMethod]
        public void AllowInitialSlash()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "/intvalue", value = 1 },
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(1, operations[0].Value);
        }

        [TestMethod]
        public void PathParses()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "intvalue", value = 1 }
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(nameof(PatchableItem.IntValue), operations[0].PropertyTree.Property.Name);
        }

        [TestMethod]
        public void EmptyStringParsesToNullForNonString()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "nullableintvalue", value = "" },
                new JsonPatch { op = PatchOperationTypes.replace, path = "stringvalue", value = "" },
                new JsonPatch { op = PatchOperationTypes.replace, path = "IntValue", value = "" },
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(null, operations[0].Value);
            Assert.AreEqual("", operations[1].Value);
            Assert.AreEqual(null, operations[2].Value);
        }

        [TestMethod]
        public void PathParsesValidCollectionOperations()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.add, path = "subitems", value = new PatchableCircularReferenceItem() },
                new JsonPatch { op = PatchOperationTypes.replace, path = "subitems/hello world", value = new PatchableCircularReferenceItem() },
                new JsonPatch { op = PatchOperationTypes.remove, path = "subitems/hello world" }
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(nameof(PatchableItem.SubItems), operations[0].PropertyTree.Property.Name);
            Assert.AreEqual(null, operations[0].PropertyTree.CollectionKey);

            Assert.AreEqual(nameof(PatchableItem.SubItems), operations[1].PropertyTree.Property.Name);
            Assert.AreEqual("hello world", operations[1].PropertyTree.CollectionKey);

            Assert.AreEqual(nameof(PatchableItem.SubItems), operations[2].PropertyTree.Property.Name);
            Assert.AreEqual("hello world", operations[2].PropertyTree.CollectionKey);
        }

        [TestMethod]
        public void PathParsesCollectionsRecursively()
        {
            var subItem = new PatchableCircularReferenceItem();
            var patches = new List<JsonPatch>
            {
                new JsonPatch() { op = PatchOperationTypes.replace, path = "subitems/a/children/b/children/c", value = subItem }
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(nameof(PatchableItem.SubItems), operations[0].PropertyTree.Property.Name);
            Assert.AreEqual("a", operations[0].PropertyTree.CollectionKey);
            Assert.AreEqual(nameof(PatchableCircularReferenceItem.Children), operations[0].PropertyTree.Next.Property.Name);
            Assert.AreEqual("b", operations[0].PropertyTree.Next.CollectionKey);
            Assert.AreEqual(nameof(PatchableCircularReferenceItem.Children), operations[0].PropertyTree.Next.Next.Property.Name);
            Assert.AreEqual("c", operations[0].PropertyTree.Next.Next.CollectionKey);
            Assert.AreEqual(subItem, operations[0].Value);
        }

        [TestMethod]
        public void PathParsesCollectionWithEscapeCharacterInKey()
        {
            string escapedKey = @"key with \/ slash in it";
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = $"subitems/{escapedKey}/code", value = "d" }
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(nameof(PatchableItem.SubItems), operations[0].PropertyTree.Property.Name);
            Assert.AreEqual(escapedKey.Replace("\\", ""), operations[0].PropertyTree.CollectionKey);
        }

        [TestMethod]
        public void PathParsesAsIndividualPiecesOperations()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "CantUpdateInOnePatch/Code", value = "A" }
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(1, operations.Count);
        }

        [TestMethod]
        public void PathParsesValidWholeOperations()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "MustUpdateInOnePatch", value = new PatchableCircularReferenceItem() },
                new JsonPatch { op = PatchOperationTypes.replace, path = "ListOfMustUpdateInOnePatch", value = new List<PatchableCircularReferenceItem>() },
                new JsonPatch { op = PatchOperationTypes.replace, path = "SubItems/MyId", value = new PatchableCircularReferenceItem() }
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(3, operations.Count);
        }
    }
}
