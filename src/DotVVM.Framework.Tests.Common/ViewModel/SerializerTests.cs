using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Tests.Common.ViewModel
{
    [TestClass]
    public class SerializerTests
    {
        static ViewModelJsonConverter CreateConverter(bool isPostback, JObject encryptedValues = null)
        {
            var config = DotvvmTestHelper.DefaultConfig;
            return new ViewModelJsonConverter(
                isPostback,
                config.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>(),
                config.ServiceProvider,
                encryptedValues
            ) {
                UsedSerializationMaps = new HashSet<ViewModelSerializationMap>()
            };
        }

        string Serialize<T>(T viewModel, out JObject encryptedValues, bool isPostback = false)
        {
            encryptedValues = new JObject();
            var settings = DefaultSerializerSettingsProvider.Instance.Settings;
            var serializer = JsonSerializer.Create(settings);
            serializer.Converters.Add(CreateConverter(isPostback, encryptedValues));

            var output = new StringWriter();
            serializer.Serialize(output, viewModel);
            return output.ToString();
        }

        T Populate<T>(string json, JObject encryptedValues = null)
        {
            var settings = DefaultSerializerSettingsProvider.Instance.Settings;
            var serializer = JsonSerializer.Create(settings);
            serializer.Converters.Add(CreateConverter(true, encryptedValues));

            //serializer.Populate(new StringReader(json), viewModel);
            return serializer.Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }

        [TestMethod]
        public void Support_NestedProtectedData()
        {
            var obj = new TestViewModelWithNestedProtectedData() {
                Root = new DataNode() {
                    Text = "Root",
                    EncryptedData = new DataNode() {
                        Text = "Encrypted1",
                        EncryptedData = new DataNode() {
                            Text = "Encrypted2",
                            EncryptedData = new DataNode() {
                                Text = "Encrypted3"
                            }
                        }
                    },
                    SignedData = new DataNode() {
                        Text = "Signed1",
                        SignedData = new DataNode() {
                            Text = "Signed2",
                            SignedData = new DataNode() {
                                Text = "Signed3"
                            }
                        }
                    }
                }
            };

            var serialized = Serialize(obj, out var encryptedValues, false);
            var deserialized = Populate<TestViewModelWithNestedProtectedData>(serialized, encryptedValues);
            Assert.AreEqual(serialized, Serialize(deserialized, out var _, false));
        }

        [TestMethod]
        public void Support_CollectionWithNestedProtectedData()
        {
            var obj = new TestViewModelWithCollectionOfNestedProtectedData() {
                Collection = new List<DataNode>() {
                    null,
                    new DataNode() { Text = "Element1", SignedData = new DataNode() { Text = "InnerSigned1" } },
                    null,
                    new DataNode() { Text = "Element2", SignedData = new DataNode() { Text = "InnerSigned2" } }
                }
            };

            var serialized = Serialize(obj, out var encryptedValues, false);
            var deserialized = Populate<TestViewModelWithCollectionOfNestedProtectedData>(serialized, encryptedValues);
            Assert.AreEqual(serialized, Serialize(deserialized, out var _, false));
        }

        [TestMethod]
        public void Support_CollectionOfCollectionsWithNestedProtectedData()
        {
            var obj = new TestViewModelWithCollectionOfNestedProtectedData() {
                Matrix = new List<List<DataNode>>() {
                    new List<DataNode>() {
                        new DataNode() { Text = "Element11", SignedData = new DataNode() { Text = "Signed11" } },
                        new DataNode() { Text = "Element12", SignedData = new DataNode() { Text = "Signed12" } },
                        new DataNode() { Text = "Element13", SignedData = new DataNode() { Text = "Signed13" } },
                    },
                    new List<DataNode>() {
                        new DataNode() { Text = "Element21", EncryptedData = new DataNode() { Text = "Encrypted21" } },
                        new DataNode() { Text = "Element22", EncryptedData = new DataNode() { Text = "Encrypted22" } },
                        new DataNode() { Text = "Element23", EncryptedData = new DataNode() { Text = "Encrypted23" } },
                    },
                    new List<DataNode>() {
                        new DataNode() { Text = "Element31", EncryptedData = new DataNode() { Text = "Encrypted31" } },
                        new DataNode() { Text = "Element32", EncryptedData = new DataNode() { Text = "Encrypted32" } },
                        new DataNode() { Text = "Element33", EncryptedData = new DataNode() { Text = "Encrypted33" } },
                    },
                }
            };

            var serialized = Serialize(obj, out var encryptedValues, false);
            var deserialized = Populate<TestViewModelWithCollectionOfNestedProtectedData>(serialized, encryptedValues);
            Assert.AreEqual(serialized, Serialize(deserialized, out var _, false));
        }

        [TestMethod]
        public void Support_NestedMixedProtectedData()
        {
            var obj = new TestViewModelWithNestedProtectedData() {
                Root = new DataNode() {
                    Text = "Root",
                    SignedData = new DataNode() {
                        Text = "Signed",
                        EncryptedData = new DataNode() {
                            Text = "Encrypted",
                        }
                    }
                }
            };

            var serialized = Serialize(obj, out var encryptedValues, false);
            var deserialized = Populate<TestViewModelWithNestedProtectedData>(serialized, encryptedValues);
            Assert.AreEqual(serialized, Serialize(deserialized, out var _, false));
        }


        [TestMethod]
        public void SupportTuples()
        {
            var obj = new TestViewModelWithTuples() {
                P1 = new Tuple<int, int, int, int>(9, 8, 7, 6),
                P2 = (5, 6, 7, 8),
                P3 = {
                    new KeyValuePair<int, int>(3, 4),
                    new KeyValuePair<int, int>(5, 6)
                },
                P4 = (
                    6,
                    new TestViewModelWithBind {
                        P1 = "X",
                        P2 = "Y",
                        ServerToClient = "Z",
                        ClientToServer = "Z"
                    }
                )
            };
            var obj2 = Populate<TestViewModelWithTuples>(Serialize(obj, out var _));

            Assert.AreEqual(obj.P1, obj2.P1);
            Assert.AreEqual(obj.P2, obj2.P2);
            Assert.IsTrue(obj.P3.SequenceEqual(obj2.P3));
            Assert.AreEqual(obj.P4.a, obj2.P4.a);
            Assert.AreEqual(obj.P4.b.P1, obj2.P4.b.P1);
            Assert.AreEqual(obj.P4.b.P2, obj2.P4.b.P2);
            Assert.AreEqual("default", obj2.P4.b.ServerToClient);
            Assert.AreEqual("default", obj2.P4.b.ClientToServer);
        }
    }

    public class DataNode
    {
        [Protect(ProtectMode.EncryptData)]
        public DataNode EncryptedData { get; set; }

        [Protect(ProtectMode.SignData)]
        public DataNode SignedData { get; set; }

        [Protect(ProtectMode.None)]
        public string Text { get; set; }
    }

    public class TestViewModelWithNestedProtectedData
    {
        public DataNode Root { get; set; }
    }

    public class TestViewModelWithCollectionOfNestedProtectedData
    {
        [Protect(ProtectMode.SignData)]
        public List<DataNode> Collection { get; set; }

        [Protect(ProtectMode.SignData)]
        public List<List<DataNode>> Matrix { get; set; }
    }

    public class TestViewModelWithTuples
    {
        public Tuple<int, int, int, int> P1 { get; set; }
        public (int a, int b, int c, int d) P2 { get; set; } = (1, 2, 3, 4);
        public List<KeyValuePair<int, int>> P3 { get; set; } = new List<KeyValuePair<int, int>>();
        public (int a, TestViewModelWithBind b) P4 { get; set; } = (1, new TestViewModelWithBind());
    }

    public class TestViewModelWithBind
    {
        [Bind(Name = "property ONE")]
        public string P1 { get; set; } = "value 1";
        [JsonProperty("property TWO")]
        public string P2 { get; set; } = "value 2";
        [Bind(Direction.ClientToServer)]
        public string ClientToServer { get; set; } = "default";
        [Bind(Direction.ServerToClient)]
        public string ServerToClient { get; set; } = "default";
    }
}
