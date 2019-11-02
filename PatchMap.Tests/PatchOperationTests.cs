using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PatchMap.Tests
{
    [TestClass]
    public class PatchOperationTests
    {
        [TestMethod]
        public void ToStringRecursively()
        {
            var prop1 = typeof(DateTime).GetProperty(nameof(DateTime.Day));
            var prop2 = typeof(DateTime).GetProperty(nameof(DateTime.Hour));
            var prop3 = typeof(DateTime).GetProperty(nameof(DateTime.Minute));

            var op = new PatchOperation(new PatchOperationPropertyPath(prop1)
            {
                Next = new PatchOperationPropertyPath(prop2)
                {
                    Next = new PatchOperationPropertyPath(prop3)
                }
            }, PatchOperationTypes.replace, null);

            Assert.AreEqual("Day/Hour/Minute", op.PropertyPath.ToString());
        }

        [TestMethod]
        public void ManualCreate_FromEntity()
        {
            var model = new PatchableItem
            {
                IntValue = 2,
                CantUpdateInOnePatch = new PatchableStandardSubItem { Code = "A" }
            };
            var op1 = PatchOperation.Create(model, i => i.IntValue);
            var op2 = PatchOperation.Create(model, i => i.CantUpdateInOnePatch.Code);
            var op3 = PatchOperation.Create(model, i => i.CantUpdateInOnePatch.Child.Code);
            var op4 = PatchOperation.Create(null as PatchableItem, i => i.IntValue);

            Assert.AreEqual("IntValue", op1.PropertyPath.ToString());
            Assert.AreEqual(2, op1.Value);
            Assert.AreEqual("CantUpdateInOnePatch/Code", op2.PropertyPath.ToString());
            Assert.AreEqual("A", op2.Value);
            Assert.AreEqual("CantUpdateInOnePatch/Child/Code", op3.PropertyPath.ToString());
            Assert.AreEqual(null, op3.Value);
            Assert.AreEqual("IntValue", op4.PropertyPath.ToString());
            Assert.AreEqual(0, op4.Value);
        }

        [TestMethod]
        public void ManualCreate_FromValue()
        {
            var op1 = PatchOperation<PatchableItem>.Create(i => i.IntValue, 2);
            var op2 = PatchOperation<PatchableItem>.Create(i => i.CantUpdateInOnePatch.Code, "A");

            Assert.AreEqual("IntValue", op1.PropertyPath.ToString());
            Assert.AreEqual(2, op1.Value);
            Assert.AreEqual("CantUpdateInOnePatch/Code", op2.PropertyPath.ToString());
            Assert.AreEqual("A", op2.Value);
        }

        [TestMethod]
        public void ObjectParse_Basic()
        {
            var stringList = new List<string> { "abc" };
            var model = new PatchableItem
            {
                IntValue = 5,
                StringList = stringList
            };

            var operations = model.ToPatchOperations().OrderBy(p => p.PropertyPath.Property.Name).ToList();

            var intValueOp = operations.FirstOrDefault(o => o.PropertyPath.Property.Name == nameof(PatchableItem.IntValue));
            Assert.AreEqual(PatchOperationTypes.replace, intValueOp.Operation);
            Assert.AreEqual(5, intValueOp.Value);

            var nullableIntValueOp = operations.FirstOrDefault(o => o.PropertyPath.Property.Name == nameof(PatchableItem.NullableIntValue));
            Assert.AreEqual(PatchOperationTypes.replace, nullableIntValueOp.Operation);
            Assert.AreEqual(null, nullableIntValueOp.Value);

            var stringValueOp = operations.FirstOrDefault(o => o.PropertyPath.Property.Name == nameof(PatchableItem.StringValue));
            Assert.AreEqual(PatchOperationTypes.replace, stringValueOp.Operation);
            Assert.AreEqual(null, stringValueOp.Value);

            var stringListOp = operations.FirstOrDefault(o => o.PropertyPath.Property.Name == nameof(PatchableItem.StringList));
            Assert.AreEqual(PatchOperationTypes.replace, stringListOp.Operation);
            Assert.AreEqual(stringList, stringListOp.Value);

            var subItemsOp = operations.FirstOrDefault(o => o.PropertyPath.Property.Name == nameof(PatchableItem.SubItems));
            Assert.AreEqual(PatchOperationTypes.replace, subItemsOp.Operation);
            Assert.AreEqual(0, (subItemsOp.Value as List<PatchableCircularReferenceItem>).Count);

            var cantUpdateInOnePatchOps = operations.Where(o => o.PropertyPath.Property.Name == nameof(PatchableItem.CantUpdateInOnePatch)).ToList();
            Assert.IsTrue(cantUpdateInOnePatchOps.Count > 0);
            Assert.IsTrue(cantUpdateInOnePatchOps.All(o => o.Operation == PatchOperationTypes.replace));
            Assert.IsTrue(cantUpdateInOnePatchOps.All(o => o.Value == null || o.Value is List<PatchableCircularReferenceItem>));//Children collection will be instantiated as new list

            //Ignored
            Assert.IsTrue(!operations.Any(p => p.PropertyPath.Property.Name == nameof(PatchableItem.ThisShouldNotGenerateAPatch)));
        }

        [TestMethod]
        public void ObjectParse_Collection()
        {
            var subItems = new List<PatchableCircularReferenceItem>
                {
                    new PatchableCircularReferenceItem
                    {
                        Code = "A",
                        Description = "AA"
                    },
                    new PatchableCircularReferenceItem
                    {
                        Code = "B",
                        Description = "BB",
                        Children = new List<PatchableCircularReferenceItem>
                        {
                            new PatchableCircularReferenceItem
                            {
                                Code = "C",
                                Description = "CC"
                            }
                        }
                    }
            };

            var model = new PatchableItem { SubItems = subItems };

            var operations = model.ToPatchOperations();
            var subItemOperations = operations.Where(o => o.PropertyPath.Property.Name == nameof(PatchableItem.SubItems)).ToList();

            Assert.AreEqual(PatchOperationTypes.replace, subItemOperations[0].Operation);
            Assert.AreEqual(nameof(PatchableItem.SubItems), subItemOperations[0].PropertyPath.Property.Name);
            Assert.AreEqual(subItems, subItemOperations[0].Value);
        }

        [TestMethod]
        public void ObjectParse_RecursiveProperty()
        {
            var child = new PatchableCircularReferenceItem
            {
                Code = "C",
                Description = "CC"
            };
            var model = new PatchableItem
            {
                CantUpdateInOnePatch = new PatchableStandardSubItem
                {
                    Code = "B",
                    Description = "BB",
                    Child = child
                }
            };

            var operations = model.ToPatchOperations();
            Assert.IsTrue(operations.All(o => o.Operation == PatchOperationTypes.replace));

            var subItemOperations = operations.Where(o => o.PropertyPath.Property.Name == nameof(PatchableItem.CantUpdateInOnePatch)).ToList();

            var codeOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Code));
            Assert.AreEqual("B", codeOp.Value);

            var descriptionOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Description));
            Assert.AreEqual("BB", descriptionOp.Value);

            var childCodeOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableStandardSubItem.Child) && o.PropertyPath.Next?.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Code));
            Assert.AreEqual("C", childCodeOp.Value);

            var childDescriptionOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableStandardSubItem.Child) && o.PropertyPath.Next?.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Description));
            Assert.AreEqual("CC", childDescriptionOp.Value);

            var childChildrenOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableStandardSubItem.Child) && o.PropertyPath.Next?.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Children));
            Assert.AreEqual(0, (childChildrenOp.Value as List<PatchableCircularReferenceItem>).Count);
        }

        [TestMethod]
        public void ObjectParse_RecursiveProperty_Null()
        {
            var model = new PatchableItem();
            var subItemOperations = model.ToPatchOperations().Where(o => o.PropertyPath.Property.Name == nameof(PatchableItem.CantUpdateInOnePatch)).ToList();

            var codeOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Code));
            Assert.AreEqual(null, codeOp.Value);

            var descriptionOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Description));
            Assert.AreEqual(null, descriptionOp.Value);

            var childCodeOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableStandardSubItem.Child) && o.PropertyPath.Next?.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Code));
            Assert.AreEqual(null, childCodeOp.Value);

            var childDescriptionOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableStandardSubItem.Child) && o.PropertyPath.Next?.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Description));
            Assert.AreEqual(null, childDescriptionOp.Value);

            var childChildrenOp = subItemOperations.First(o => o.PropertyPath.Next?.Property.Name == nameof(PatchableStandardSubItem.Child) && o.PropertyPath.Next?.Next?.Property.Name == nameof(PatchableCircularReferenceItem.Children));
            Assert.AreEqual(0, (childChildrenOp.Value as List<PatchableCircularReferenceItem>).Count);
        }
    }
}
