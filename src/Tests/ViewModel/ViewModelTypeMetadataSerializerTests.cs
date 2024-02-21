﻿using System;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Testing;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class ViewModelTypeMetadataSerializerTests
    {
        private static ViewModelSerializationMapper mapper;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            mapper = new ViewModelSerializationMapper(new ViewModelValidationRuleTranslator(),
                new AttributeViewModelValidationMetadataProvider(),
                new DefaultPropertySerialization(),
                DotvvmConfiguration.CreateDefault());
        }

#if DotNetCore
        [DataTestMethod]
        [DataRow(typeof(bool), "'Boolean'")]
        [DataRow(typeof(int?), "{'type':'nullable','inner':'Int32'}")]
        [DataRow(typeof(long[][]), "[['Int64']]")]
        [DataRow(typeof(Type), "'Av/XciKNYBmL6ZsV'")]   // unknown types should produce SHA1 hash
        [DataRow(typeof(object), "{'type':'dynamic'}")]
        [DataRow(typeof(Dictionary<string, string>), "[\"C+Vo5gho7HtxCAAr\"]")]
        [DataRow(typeof(IDictionary<string, string>), "[\"C+Vo5gho7HtxCAAr\"]")]
        [DataRow(typeof(Dictionary<int, int>), "[\"VnTd1CsMIOOD62hn\"]")]
        [DataRow(typeof(Dictionary<char, object>), "[\"YVHqjWtxjfqkABaT\"]")]
        [DataRow(typeof(IDictionary<int, int>), "[\"VnTd1CsMIOOD62hn\"]")]
        [DataRow(typeof(Dictionary<object, object>), "[\"ReXn90XOeD3pn81A\"]")]
        [DataRow(typeof(IDictionary<object, object>), "[\"ReXn90XOeD3pn81A\"]")]
        [DataRow(typeof(List<KeyValuePair<string, string>>), "[\"C+Vo5gho7HtxCAAr\"]")]
        [DataRow(typeof(List<KeyValuePair<int, int>>), "[\"VnTd1CsMIOOD62hn\"]")]
        [DataRow(typeof(List<KeyValuePair<object, object>>), "[\"ReXn90XOeD3pn81A\"]")]
        [DataRow(typeof(IList<KeyValuePair<string, string>>), "[\"C+Vo5gho7HtxCAAr\"]")]
        [DataRow(typeof(IList<KeyValuePair<int, int>>), "[\"VnTd1CsMIOOD62hn\"]")]
        [DataRow(typeof(IList<KeyValuePair<object, object>>), "[\"ReXn90XOeD3pn81A\"]")]
        // these hashes are dependent on the target framework - the latest update of hashes is updated to net60
        public void ViewModelTypeMetadata_TypeName(Type type, string expected)
        {
            var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper);
            var dependentObjectTypes = new HashSet<Type>();
            var dependentEnumTypes = new HashSet<Type>();
            var result = typeMetadataSerializer.GetTypeIdentifier(type, dependentObjectTypes, dependentEnumTypes);
            Assert.AreEqual(expected.Replace("'", "\""), result.ToString(Formatting.None));
        }
#endif

        [TestMethod]
        public void ViewModelTypeMetadata_TypeMetadata()
        {
            CultureUtils.RunWithCulture("en-US", () =>
            {
                var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper);
                var result = typeMetadataSerializer.SerializeTypeMetadata(new[]
                {
                    mapper.GetMap(typeof(TestViewModel))
                });

                var checker = new OutputChecker("testoutputs");
                checker.CheckJsonObject(result);
            });
        }

        [TestMethod]
        public void ViewModelTypeMetadata_ValidationRules()
        {
            CultureUtils.RunWithCulture("en-US", () => {
                var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper);
                var result = typeMetadataSerializer.SerializeTypeMetadata(new[] { mapper.GetMap(typeof(TestViewModel)) });

                var rules = XAssert.IsType<JArray>(result[typeof(TestViewModel).GetTypeHash()]["properties"]["ServerToClient"]["validationRules"]);
                XAssert.Single(rules);
                Assert.AreEqual("required", rules[0]["ruleName"].Value<string>());
                Assert.AreEqual("ServerToClient is required!", rules[0]["errorMessage"].Value<string>());
            });
        }

        [TestMethod]
        public void ViewModelTypeMetadata_ValidationDisabled()
        {
            CultureUtils.RunWithCulture("en-US", () => {
                var config = DotvvmTestHelper.CreateConfiguration();
                config.ClientSideValidation = false;

                var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper, config);
                var result = typeMetadataSerializer.SerializeTypeMetadata(new[] { mapper.GetMap(typeof(TestViewModel)) });

                XAssert.Null(result[typeof(TestViewModel).GetTypeHash()]["properties"]["ServerToClient"]["validationRules"]);
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

            [JsonProperty("property TWO")]
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
