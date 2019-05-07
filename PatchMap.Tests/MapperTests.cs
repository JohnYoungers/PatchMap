using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchMap.Exceptions;
using PatchMap.Mapping;
using PatchMap.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PatchMap.Tests
{
    [TestClass]
    public class MapperTests
    {
        Mapper<SampleViewModel, SampleEntity, SampleContext> mapper;
        SampleViewModel source;
        SampleEntity target;

        [TestInitialize]
        public void BeforeEach()
        {
            mapper = new Mapper<SampleViewModel, SampleEntity, SampleContext>();
            source = new SampleViewModel();
            target = new SampleEntity();
        }

        [TestMethod]
        public void BasicMapping()
        {
            target.AssociatedEntity = new SampleAssociatedEntity();

            mapper.AddMap(vm => vm.Id, db => db.Id);
            mapper.AddMap(vm => vm.MultiWordProperty, db => db.Multiwordproperty);
            mapper.AddMap(vm => vm.Address.AddressLine1, db => db.AddressLine1);
            mapper.AddMap(vm => vm.AssociatedEntityName, db => db.AssociatedEntity.Name);

            Assert.AreEqual("Id", (mapper.Mappings[0] as FieldMap<SampleEntity, SampleContext>).GenerateLabel(null, null));
            Assert.AreEqual("Multi Word Property", (mapper.Mappings[1] as FieldMap<SampleEntity, SampleContext>).GenerateLabel(null, null));

            source.Id = 5;
            source.MultiWordProperty = "ABC";
            source.Address = new AddressViewModel { AddressLine1 = "DEF" };
            source.AssociatedEntityName = "GHI";
            var ctx = new SampleContext();
            var results = mapper.Map(source.ToPatchOperations(), target, ctx);

            Assert.AreEqual(ctx, results.Context);
            Assert.AreEqual(true, results.Succeeded);
            Assert.AreEqual(5, target.Id);
            Assert.AreEqual("ABC", target.Multiwordproperty);
            Assert.AreEqual("DEF", target.AddressLine1);
            Assert.AreEqual("GHI", target.AssociatedEntity.Name);
        }

        [TestMethod]
        public void JsonValueNotParsable()
        {
            mapper.AddMap(vm => vm.Id, db => db.Id);

            var patch = new JsonPatch
            {
                path = "Id"
            };
            var operations = (new[] { patch }).ToPatchOperations<SampleViewModel>();
            var results = mapper.Map(operations, target, new SampleContext());

            void VerifyFailed()
            {
                Assert.AreEqual(false, results.Succeeded);
                Assert.AreEqual(1, results.Failures.Count);
                Assert.AreEqual(mapper.Mappings[0], results.Failures[0].Map);
                Assert.AreEqual(operations[0], results.Failures[0].PatchOperation);
                Assert.AreEqual(MapResultFailureType.JsonPatchValueNotParsable, results.Failures[0].FailureType);
                Assert.AreEqual(null, results.Failures[0].Reason);
            }

            VerifyFailed();

            patch.value = "Can't use a string for an int";
            operations = (new[] { patch }).ToPatchOperations<SampleViewModel>();
            results = mapper.Map(operations, target, new SampleContext());
            VerifyFailed();
        }

        [TestMethod]
        public void Converter()
        {
            mapper.AddMap(vm => vm.Parent, db => db.ParentId).HasConverter((SampleEntity t, SampleContext ctx, SampleSummaryViewModel value) =>
            {
                return value.Id == 6
                ? new FieldMapConversionResult<int?> { FailureReason = "6 is no good" }
                : new FieldMapConversionResult<int?> { Value = value.Id };
            });

            var results = mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(true, results.Succeeded);
            Assert.AreEqual(null, target.ParentId);

            source.Parent = new SampleSummaryViewModel { Id = 5 };
            results = mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(true, results.Succeeded);
            Assert.AreEqual(5, target.ParentId);

            source.Parent.Id = 6;
            var operations = source.ToPatchOperations();
            results = mapper.Map(operations, target, new SampleContext());
            Assert.AreEqual(false, results.Succeeded);
            Assert.AreEqual(1, results.Failures.Count);
            Assert.AreEqual(mapper.Mappings[0], results.Failures[0].Map);
            Assert.AreEqual(operations.FirstOrDefault(o => o.PropertyTree.ToString() == "Parent"), results.Failures[0].PatchOperation);
            Assert.AreEqual(MapResultFailureType.ValueConversionFailed, results.Failures[0].FailureType);
            Assert.AreEqual("6 is no good", results.Failures[0].Reason);
        }

        [TestMethod]
        public void Converter_ManualFieldUpdate()
        {
            bool changedRegistered = false;
            // This converter is manually updating the field itself: make sure our comparison uses the original value
            mapper.AddMap(vm => vm.Parent, db => db.ParentId).HasConverter((SampleEntity t, SampleContext ctx, SampleSummaryViewModel value) =>
            {
                t.ParentId = 5;
                return new FieldMapConversionResult<int?> { Value = 5 };
            }).HasPostMap((target, ctx, map, operation) => changedRegistered = true);

            source.Parent = new SampleSummaryViewModel { Id = 4 };
            var results = mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(true, results.Succeeded);
            Assert.AreEqual(true, changedRegistered);
        }

        [TestMethod]
        public void Enable()
        {
            mapper.AddMap(vm => vm.Id, db => db.Id).IsEnabled((t, ctx) => ctx.IsNew);

            source.Id = 2;
            mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(0, target.Id);

            mapper.Map(source.ToPatchOperations(), target, new SampleContext { IsNew = true });
            Assert.AreEqual(2, target.Id);
        }

        [TestMethod]
        public void Required()
        {
            mapper.AddMap(vm => vm.MultiWordProperty, db => db.Multiwordproperty).IsRequired((t, ctx) => ctx.IsNew);

            var operations = source.ToPatchOperations();
            var results = mapper.Map(operations, target, new SampleContext { IsNew = true });
            Assert.AreEqual(false, results.Succeeded);
            Assert.AreEqual(1, results.Failures.Count);
            Assert.AreEqual(mapper.Mappings[0], results.Failures[0].Map);
            Assert.AreEqual(operations.FirstOrDefault(o => o.PropertyTree.ToString() == "MultiWordProperty"), results.Failures[0].PatchOperation);
            Assert.AreEqual(MapResultFailureType.Required, results.Failures[0].FailureType);
            Assert.AreEqual(null, results.Failures[0].Reason);

            results = mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(true, results.Succeeded);
        }

        [TestMethod]
        public void MapperRequired()
        {
            mapper.MapTargetIsRequired = (SampleEntity target, SampleContext ctx, FieldMap<SampleEntity, SampleContext> map, PatchOperation operation) =>
            {
                return operation.PropertyTree.ToString() == "MultiWordProperty";
            };

            mapper.AddMap(vm => vm.MultiWordProperty, db => db.Multiwordproperty);
            mapper.AddMap(vm => vm.Address.AddressLine1, db => db.AddressLine1);

            var operations = source.ToPatchOperations();
            var results = mapper.Map(operations, target, new SampleContext());
            Assert.AreEqual(false, results.Succeeded);
            Assert.AreEqual(1, results.Failures.Count);
            Assert.AreEqual(operations.FirstOrDefault(o => o.PropertyTree.ToString() == "MultiWordProperty"), results.Failures[0].PatchOperation);
            Assert.AreEqual(mapper.Mappings[0], results.Failures[0].Map);
            Assert.AreEqual(MapResultFailureType.Required, results.Failures[0].FailureType);
            Assert.AreEqual(null, results.Failures[0].Reason);
        }

        [TestMethod]
        public void PostMap()
        {
            source.Id = 1;
            target.Id = 1;

            mapper.AddMap(vm => vm.Id, db => db.Id).HasPostMap((t, ctx, map, operation) =>
            {
                t.Id = 6;
            });
            mapper.AddMap(vm => vm.MultiWordProperty).HasPostMap((t, ctx, map, operation) =>
            {
                t.ParentId = 10;
            });

            mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(1, target.Id); //Value didn't change so postmap was not run
            Assert.AreEqual(10, target.ParentId); //PostMap always runs for maps that are not tied to a field on the target

            source.Id = 5;
            mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(6, target.Id);
        }

        [TestMethod]
        public void MapTargetHasChanged()
        {
            mapper.MapTargetHasChanged = (SampleEntity target, SampleContext ctx, FieldMap<SampleEntity, SampleContext> map, PatchOperation operation, object originalValue, object newValue) =>
            {
                return true;
            };

            source.Id = 1;
            target.Id = 1;

            mapper.AddMap(vm => vm.Id, db => db.Id).HasPostMap((t, ctx, map, operation) =>
            {
                t.Id = 6;
            });

            mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual(6, target.Id); //Postmap ran in this instance because we're always reporting fields as changed
        }

        [TestMethod]
        public void MapTargetValueIsValid()
        {
            mapper.MapTargetValueIsValid = (SampleEntity target, SampleContext ctx, FieldMap<SampleEntity, SampleContext> map, PatchOperation operation, object value) =>
            {
                return new FieldMapValueValidResult { FailureReason = Equals(value, 1) ? "Failed for 1" : "Failed for Other" };
            };

            mapper.AddMap(vm => vm.Id, db => db.Id);

            var operations = source.ToPatchOperations();
            var results = mapper.Map(operations, target, new SampleContext());
            Assert.AreEqual(false, results.Succeeded);
            Assert.AreEqual(1, results.Failures.Count);
            Assert.AreEqual(operations.FirstOrDefault(o => o.PropertyTree.ToString() == "Id"), results.Failures[0].PatchOperation);
            Assert.AreEqual(mapper.Mappings[0], results.Failures[0].Map);
            Assert.AreEqual(MapResultFailureType.ValueIsNotValid, results.Failures[0].FailureType);
            Assert.AreEqual("Failed for Other", results.Failures[0].Reason);

            source.Id = 1;
            results = mapper.Map(source.ToPatchOperations(), target, new SampleContext());
            Assert.AreEqual("Failed for 1", results.Failures[0].Reason);
        }

        [TestMethod]
        public void TargetChildElementNullIsProgrammingError()
        {
            mapper.AddMap(vm => vm.Id, db => db.AssociatedEntity.SampleEntityId);

            //Target.AssociatedEntity is null
            Assert.ThrowsException<ArgumentException>(() => mapper.Map(source.ToPatchOperations(), target, new SampleContext()));
        }

        [TestMethod]
        public void ThrowErrorOnMissingMapForJsonPatch()
        {
            var operations = source.ToPatchOperations();//load up some non-Json patches
            operations.AddRange((new[] { new JsonPatch {
                op = PatchOperationTypes.replace,
                path = "Address/City",
                value = 5
            } }).ToPatchOperations<SampleViewModel>());
            var context = new SampleContext();
            var ex = Assert.ThrowsException<JsonPatchParseException>(() => mapper.Map(operations, target, context));
            Assert.AreEqual("A map for Address/City has not been configured.", ex.Message);

            // No exception with flag enabled
            mapper.Map(operations, target, context, new MapConfiguration { AllowUnmappedOperations = true });
        }
    }
}
