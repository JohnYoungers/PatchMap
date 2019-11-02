using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PatchMap.Tests.JsonPatchParsing
{
    [TestClass]
    public class SystemTextJson_InvalidTests : General_InvalidTests
    {
        [TestInitialize]
        public void BeforeEach()
        {
            JsonPatch.ValueIsParsable = (o) => o is JsonElement;
            JsonPatch.ParseValue = (o, t) => JsonSerializer.Deserialize(((JsonElement)o).GetRawText(), t);
        }
    }
}
