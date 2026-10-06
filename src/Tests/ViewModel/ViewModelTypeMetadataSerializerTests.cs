using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CheckTestOutput;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class ViewModelTypeMetadataSerializerTests
    {
        private static IViewModelSerializationMapper mapper = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();

        string GetSerializedString(Action<Utf8JsonWriter> action)
        {
            var buffer = new System.IO.MemoryStream();
            using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
            {
                action(writer);
            }
            return Encoding.UTF8.GetString(buffer.ToArray());
        }

#if DotNetCore
        [DataTestMethod]
        [DataRow(typeof(bool), "'Boolean'")]
        [DataRow(typeof(int?), "{'type':'nullable','inner':'Int32'}")]
        [DataRow(typeof(long[][]), "[['Int64']]")]
        [DataRow(typeof(Type), "'BIRU4_N26iJFtraq'")]   // unknown types should produce SHA1 hash
        [DataRow(typeof(object), "{'type':'dynamic'}")]
        [DataRow(typeof(Dictionary<string, string>), "[\"mYJ-JSJqbfS0WzaT\"]")]
        [DataRow(typeof(IDictionary<string, string>), "[\"mYJ-JSJqbfS0WzaT\"]")]
        [DataRow(typeof(Dictionary<int, int>), "[\"ARub2Kd-bMbK-pc-\"]")]
        [DataRow(typeof(Dictionary<char, object>), "[\"qAoMsjWKyLUAdXNa\"]")]
        [DataRow(typeof(IDictionary<int, int>), "[\"ARub2Kd-bMbK-pc-\"]")]
        [DataRow(typeof(Dictionary<object, object>), "[\"2OVnROeV9--Kq47N\"]")]
        [DataRow(typeof(IDictionary<object, object>), "[\"2OVnROeV9--Kq47N\"]")]
        [DataRow(typeof(List<KeyValuePair<string, string>>), "[\"mYJ-JSJqbfS0WzaT\"]")]
        [DataRow(typeof(List<KeyValuePair<int, int>>), "[\"ARub2Kd-bMbK-pc-\"]")]
        [DataRow(typeof(List<KeyValuePair<object, object>>), "[\"2OVnROeV9--Kq47N\"]")]
        [DataRow(typeof(IList<KeyValuePair<string, string>>), "[\"mYJ-JSJqbfS0WzaT\"]")]
        [DataRow(typeof(IList<KeyValuePair<int, int>>), "[\"ARub2Kd-bMbK-pc-\"]")]
        [DataRow(typeof(IList<KeyValuePair<object, object>>), "[\"2OVnROeV9--Kq47N\"]")]
        // these hashes are dependent on the target framework - the latest update of hashes is updated to net60
        public void ViewModelTypeMetadata_TypeName(Type type, string expected)
        {
            var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper);
            var dependentObjectTypes = new HashSet<Type>();
            var dependentEnumTypes = new HashSet<Type>();
            var typeName = GetSerializedString(json => typeMetadataSerializer.WriteTypeIdentifier(json, type, dependentObjectTypes, dependentEnumTypes));
            Assert.AreEqual(expected.Replace("'", "\""), typeName);
        }
#endif

        JsonObject SerializeMetadata(ViewModelTypeMetadataSerializer serializer, Type[] types, ISet<string>? ignoredTypes = null)
        {
            var json = GetSerializedString(writer => {
                writer.WriteStartObject();
                serializer.SerializeTypeMetadata(types.Select(mapper.GetMap), writer, "typeMetadata"u8, ignoredTypes);
                writer.WriteEndObject();
            });
            return JsonNode.Parse(json).AsObject()["typeMetadata"].AsObject();
        }

        [TestMethod]
        public void ViewModelTypeMetadata_TypeMetadata()
        {
            CultureUtils.RunWithCulture("en-US", () =>
            {
                var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper);

                var result = SerializeMetadata(typeMetadataSerializer, [typeof(TestViewModel)]);

                var checker = new OutputChecker("testoutputs");
                checker.CheckJsonObject(result);
            });
        }

        [TestMethod]
        public void ViewModelTypeMetadata_PolymorphicDependenciesAndKnownBase()
        {
            var serializer = new ViewModelTypeMetadataSerializer(mapper);
            var baseType = typeof(SerializerTests.ConcretePolymorphicBase);
            var intermediate = typeof(SerializerTests.ConcretePolymorphicIntermediate);
            var leaf = typeof(SerializerTests.ConcretePolymorphicLeaf);
            var result = SerializeMetadata(serializer, [baseType]);
            Assert.IsNotNull(result[baseType.GetTypeHash()]);
            Assert.IsNotNull(result[intermediate.GetTypeHash()]);
            Assert.IsNotNull(result[leaf.GetTypeHash()]);
            CollectionAssert.AreEquivalent(new[] { leaf.GetTypeHash() },
                result[baseType.GetTypeHash()]["derivedTypes"].AsArray().Select(n => n.GetValue<string>()).ToArray());
            var ancestry = result[leaf.GetTypeHash()]["baseTypes"].AsArray().Select(n => n.GetValue<string>()).ToArray();
            CollectionAssert.Contains(ancestry, baseType.GetTypeHash());
            CollectionAssert.Contains(ancestry, intermediate.GetTypeHash());
            var filtered = SerializeMetadata(serializer, [baseType], new HashSet<string> { baseType.GetTypeHash() });
            Assert.IsNull(filtered[baseType.GetTypeHash()]);
            Assert.IsNotNull(filtered[leaf.GetTypeHash()]);
        }

        [TestMethod]
        public void ViewModelTypeMetadata_PolymorphicRecursiveGraph()
        {
            var serializer = new ViewModelTypeMetadataSerializer(mapper);
            var result = SerializeMetadata(serializer, [typeof(RecursivePolymorphicBase)]);
            Assert.IsNotNull(result[typeof(RecursivePolymorphicCase).GetTypeHash()]);
            Assert.AreEqual(typeof(RecursivePolymorphicBase).GetTypeHash(),
                result[typeof(RecursivePolymorphicCase).GetTypeHash()]["properties"]["Child"]["type"].GetValue<string>());
        }

        [TestMethod]
        public void ViewModelTypeMetadata_CustomDiscriminatorDoesNotAddSyntheticProperties()
        {
            var serializer = new ViewModelTypeMetadataSerializer(mapper);
            var result = SerializeMetadata(serializer, [typeof(SerializerTests.CustomPolymorphicBase)]);
            var property = result[typeof(SerializerTests.CustomPolymorphicFirst).GetTypeHash()]["properties"]["kind"];
            Assert.IsNull(property);
            var selfResult = SerializeMetadata(serializer, [typeof(SerializerTests.CustomSelfPolymorphicBase)]);
            Assert.IsNull(selfResult[typeof(SerializerTests.CustomSelfPolymorphicBase).GetTypeHash()]["properties"]["kind"]);
        }

        [JsonDerivedType(typeof(RecursivePolymorphicCase))]
        public abstract class RecursivePolymorphicBase { }
        public class RecursivePolymorphicCase : RecursivePolymorphicBase
        {
            public RecursivePolymorphicBase Child { get; set; }
        }

        [TestMethod]
        public void ViewModelTypeMetadata_ValidationRules()
        {
            CultureUtils.RunWithCulture("en-US", () => {
                var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper);
                var result = SerializeMetadata(typeMetadataSerializer, [typeof(TestViewModel)]);

                var rules = XAssert.IsType<JsonArray>(result[typeof(TestViewModel).GetTypeHash()]["properties"]["ServerToClient"]["validationRules"]);
                XAssert.Single(rules);
                Assert.AreEqual("required", rules[0]["ruleName"].GetValue<string>());
                Assert.AreEqual("ServerToClient is required!", rules[0]["errorMessage"].GetValue<string>());
            });
        }

        [TestMethod]
        public void ViewModelTypeMetadata_ValidationDisabled()
        {
            CultureUtils.RunWithCulture("en-US", () => {
                var config = DotvvmTestHelper.CreateConfiguration();
                config.ClientSideValidation = false;

                var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper, config);
                var result = SerializeMetadata(typeMetadataSerializer, [typeof(TestViewModel)]);

                XAssert.Null(result[typeof(TestViewModel).GetTypeHash()]["properties"]["ServerToClient"]["validationRules"]);
            });
        }

        [TestMethod]
        public void ViewModelTypeMetadata_IgnoreNestedTypeMetadata()
        {
            CultureUtils.RunWithCulture("en-US", () =>
            {
                var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper);
                var result = SerializeMetadata(typeMetadataSerializer, [typeof(TestViewModel)],
                    ignoredTypes: new HashSet<string> { typeof(NestedTestViewModel).GetTypeHash() }
                );

                Assert.IsNotNull(result[typeof(TestViewModel).GetTypeHash()]);
                Assert.IsNull(result[typeof(NestedTestViewModel).GetTypeHash()]);
            });
        }

        [Flags]
        enum SampleEnum
        {
            Zero = 0,
            Two = 2,    // the order is mismatched intentionally - the serializer should fix it
            One = 1
        }

        class TestViewModel
        {
            [Bind(Name = "property ONE")]
            public Guid P1 { get; set; }

            [JsonPropertyName("property TWO")]
            public SampleEnum?[] P2 { get; set; }

            [Bind(Direction.ClientToServer)]
            public string ClientToServer { get; set; } = "default";

            [Bind(Direction.ServerToClient)]
            [Required(ErrorMessage = "ServerToClient is required!")]
            public string ServerToClient { get; set; } = "default";

            public List<NestedTestViewModel> NestedList { get; set; }

            [Bind(Direction.ServerToClientFirstRequest)]
            public NestedTestViewModel ChildFirstRequest { get; set; }

            public object ObjectProperty { get; set; }
        }

        class NestedTestViewModel
        {
            [Bind(Direction.None)]
            public bool Ignored { get; set; }

            [Bind(Direction.IfInPostbackPath)]
            [Required]
            [Range(0, 10, ErrorMessage = "range error")]
            public int InPathOnly { get; set; }

        }
    }

}
