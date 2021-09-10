﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using CheckTestOutput;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        [DataTestMethod]
        [DataRow(typeof(bool), "'Boolean'")]
        [DataRow(typeof(int?), "{'type':'nullable','inner':'Int32'}")]
        [DataRow(typeof(long[][]), "[['Int64']]")]
        [DataRow(typeof(Type), "'cH5MjuCiaO6mnhi5XcZwZcDjjzE='")]   // unknown types should produce SHA1 hash
        [DataRow(typeof(object), "{'type':'dynamic'}")]
        [DataRow(typeof(Dictionary<string, string>), "[\"la91iYgFuTBqaCldWgohuFgnQuY=\"]")]
        [DataRow(typeof(IDictionary<string, string>), "[\"la91iYgFuTBqaCldWgohuFgnQuY=\"]")]
        [DataRow(typeof(Dictionary<int, int>), "[\"Fj9UR9Ls097H7rqFancApm/DFX0=\"]")]
        [DataRow(typeof(Dictionary<char, object>), "[\"ODlWlZZ4i8dUWuGS4ZQGfg1utAI=\"]")]
        [DataRow(typeof(IDictionary<int, int>), "[\"Fj9UR9Ls097H7rqFancApm/DFX0=\"]")]
        [DataRow(typeof(Dictionary<object, object>), "[\"zJFI46P9nFJ5vtQ7DF8SnVBSmPg=\"]")]
        [DataRow(typeof(IDictionary<object, object>), "[\"zJFI46P9nFJ5vtQ7DF8SnVBSmPg=\"]")]
        [DataRow(typeof(List<KeyValuePair<string, string>>), "[\"la91iYgFuTBqaCldWgohuFgnQuY=\"]")]
        [DataRow(typeof(List<KeyValuePair<int, int>>), "[\"Fj9UR9Ls097H7rqFancApm/DFX0=\"]")]
        [DataRow(typeof(List<KeyValuePair<object, object>>), "[\"zJFI46P9nFJ5vtQ7DF8SnVBSmPg=\"]")]
        [DataRow(typeof(IList<KeyValuePair<string, string>>), "[\"la91iYgFuTBqaCldWgohuFgnQuY=\"]")]
        [DataRow(typeof(IList<KeyValuePair<int, int>>), "[\"Fj9UR9Ls097H7rqFancApm/DFX0=\"]")]
        [DataRow(typeof(IList<KeyValuePair<object, object>>), "[\"zJFI46P9nFJ5vtQ7DF8SnVBSmPg=\"]")]
        // these hashes are dependent on the target framework - the latest update of hashes is updated to net50
        public void ViewModelTypeMetadata_TypeName(Type type, string expected)
        {
            var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper);
            var dependentObjectTypes = new HashSet<Type>();
            var dependentEnumTypes = new HashSet<Type>();
            var result = typeMetadataSerializer.GetTypeIdentifier(type, dependentObjectTypes, dependentEnumTypes);
            Assert.AreEqual(expected.Replace("'", "\""), result.ToString(Formatting.None));
        }

        [TestMethod]        
        public void ViewModelTypeMetadata_TypeMetadata()
        {
            var typeMetadataSerializer = new ViewModelTypeMetadataSerializer(mapper);
            var result = typeMetadataSerializer.SerializeTypeMetadata(new[]
            {
                mapper.GetMap(typeof(TestViewModel))
            });

            var checker = new OutputChecker("testoutputs");
            checker.CheckJsonObject(result);
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
