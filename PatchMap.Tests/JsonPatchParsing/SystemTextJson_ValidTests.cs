using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PatchMap.Tests.JsonPatchParsing
{
    [TestClass]
    public class SystemTextJson_ValidTests : General_ValidTests
    {
        [TestInitialize]
        public void BeforeEach()
        {
            JsonPatch.ValueIsParsable = (o) => o is JsonElement;
            JsonPatch.ParseValue = (o, t) => JsonSerializer.Deserialize(((JsonElement)o).GetRawText(), t);
        }

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
                new JsonPatch { op = PatchOperationTypes.replace, path = "SubItems/Item A", value = JsonDocument.Parse(@"{ ""Code"": ""A"", ""Description"": ""B""}").RootElement }
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
        public void ParsesArray()
        {
            var patches = new List<JsonPatch>
            {
                new JsonPatch { op = PatchOperationTypes.replace, path = "StringList", value = JsonDocument.Parse(@"[""A"", ""B""]").RootElement },
                new JsonPatch { op = PatchOperationTypes.replace, path = "SubItems", value = JsonDocument.Parse(@"[{ ""Code"": ""A"", ""Description"": ""B""}, { ""Code"": ""C"", ""Description"": ""D""}]").RootElement },
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
