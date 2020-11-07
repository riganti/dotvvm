using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Tests.Common.ViewModel
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
        [DataRow(typeof(SampleEnum), "{'type':'enum','values':{'Zero':0,'One':1,'Two':2}}")]
        [DataRow(typeof(long[][]), "[['Int64']]")]
        [DataRow(typeof(Type), "'t38YPaufyA26odb7HXM9a+aghdA='")]   // unknown types should produce SHA1 hash
        public void ViewModelTypeMetadata_TypeName(Type type, string expected)
        {
            var typeMetadataSerializer = new ViewModelTypeMetadataSerializer();
            var result = typeMetadataSerializer.GetTypeIdentifier(type);
            Assert.AreEqual(expected.Replace("'", "\""), result.ToString(Formatting.None));
        }

        [TestMethod]        
        public void ViewModelTypeMetadata_TypeMetadata()
        {
            var typeMetadataSerializer = new ViewModelTypeMetadataSerializer();
            var result = typeMetadataSerializer.SerializeTypeMetadata(new[]
            {
                mapper.GetMap(typeof(TestViewModel)),
                mapper.GetMap(typeof(NestedTestViewModel))
            });

            var expected = @"{
    'hXgHohhzHL2SzMTI0/aQFP42rV8=': {
        'ChildFirstRequest': {
            'type': 'GqKC8CuCoDNJdqzKjKrxePbUZr8=',
            'post': 'no',
            'update': 'firstRequest'
        },
        'ClientToServer': {
            'type': 'String',
            'update': 'no'
        },
        'NestedList': {
            'type': [
                'GqKC8CuCoDNJdqzKjKrxePbUZr8='
            ]
        },
        'property ONE': {
            'type': 'Guid'
        },
        'property TWO': {
            'type': [
                {
                    'type': 'nullable',
                    'inner': {
                        'type': 'enum',
                        'values': {
                            'Zero': 0,
                            'One': 1,
                            'Two': 2
                        }
                    }
                }
            ]
        },
        'ServerToClient': {
            'type': 'String',
            'post': 'no',
            'validationRules': [
                {
                    'ruleName': 'required',
                    'errorMessage': 'ServerToClient is required!',
                    'parameters': []
                }
            ]
        }
    },
    'GqKC8CuCoDNJdqzKjKrxePbUZr8=': {
        'Ignored': {
            'type': 'Boolean',
            'post': 'no',
            'update': 'no'
        },
        'InPathOnly': {
            'type': 'Int32',
            'post': 'pathOnly',
            'validationRules': [
                {
                    'ruleName': 'required',
                    'errorMessage': 'The InPathOnly field is required.',
                    'parameters': []
                },
                {
                    'ruleName': 'range',
                    'errorMessage': 'range error',
                    'parameters': [
                        0,
                        10
                    ]
                }
            ]
        }
    }
}";
            Assert.AreEqual(JToken.Parse(expected.Replace("'", "\"")).ToString(Formatting.None), result.ToString(Formatting.None));
        }




        enum SampleEnum
        {
            Zero = 0,
            One = 1,
            Two = 2
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
