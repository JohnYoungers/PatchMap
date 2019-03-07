using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchMap.Tests
{
    [TestClass]
    public class JsonPatchParsing_ValidTests
    {
        [TestMethod]
        public void ValuesParse()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "intvalue", value = 1 },
                new JsonPatch { op = PatchOperationTypes.replace, path = "nullableintvalue", value = 1 },
                new JsonPatch { op = PatchOperationTypes.replace, path = "nullableintvalue", value = null },
                new JsonPatch { op = PatchOperationTypes.replace, path = "stringvalue", value = "Hello" },
                new JsonPatch { op = PatchOperationTypes.replace, path = "stringvalue", value = null },
                new JsonPatch { op = PatchOperationTypes.replace, path = "nullableguidvalue", value = "4e86c79f-8f12-40b2-9e22-180041bf4864" },
                new JsonPatch { op = PatchOperationTypes.replace, path = "SubItems/Item A", value = JToken.Parse("{ Code: 'A', Description: 'B'}")}
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(1, operations[0].Value);
            Assert.AreEqual(true, operations[0].JsonPatchValueParsed);
            Assert.AreEqual(1, operations[1].Value);
            Assert.AreEqual(true, operations[1].JsonPatchValueParsed);
            Assert.AreEqual(null, operations[2].Value);
            Assert.AreEqual(true, operations[2].JsonPatchValueParsed);
            Assert.AreEqual("Hello", operations[3].Value);
            Assert.AreEqual(true, operations[3].JsonPatchValueParsed);
            Assert.AreEqual(null, operations[4].Value);
            Assert.AreEqual(true, operations[4].JsonPatchValueParsed);
            Assert.AreEqual(new Guid("4e86c79f-8f12-40b2-9e22-180041bf4864"), operations[5].Value);
            Assert.AreEqual(true, operations[5].JsonPatchValueParsed);
            Assert.AreEqual("A", (operations[6].Value as PatchableCircularReferenceItem).Code);
            Assert.AreEqual("B", (operations[6].Value as PatchableCircularReferenceItem).Description);
            Assert.AreEqual(true, operations[6].JsonPatchValueParsed);
        }

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

        [TestMethod]
        public void ParsesArray()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "StringList", value = JArray.Parse("['A', 'B']") },
                new JsonPatch { op = PatchOperationTypes.replace, path = "SubItems", value = JArray.Parse("[{ Code: 'A', Description: 'B'}, { Code: 'C', Description: 'D'}]") },
            };

            var operations = patches.ToPatchOperations<PatchableItem>();
            Assert.AreEqual(2, operations.Count);
            CollectionAssert.AreEqual(new List<string> { "A", "B" }, (List<string>)operations[0].Value);

            var op2Values = (List<PatchableCircularReferenceItem>)operations[1].Value;
            Assert.AreEqual("A", op2Values[0].Code);
            Assert.AreEqual("D", op2Values[1].Description);
        }
    }
}
