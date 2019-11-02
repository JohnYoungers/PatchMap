using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PatchMap.Exceptions;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatchMap.Tests.JsonPatchParsing
{
    [TestClass]
    public class Newtonsoft_InvalidTests : General_InvalidTests
    {
        [TestInitialize]
        public void BeforeEach()
        {
            JsonPatch.ValueIsParsable = (o) => o is JToken;
            JsonPatch.ParseValue = (o, t) => (o as JToken).ToObject(t);
        }
    }
}
