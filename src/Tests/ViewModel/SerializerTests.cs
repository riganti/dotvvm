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
using DotVVM.Framework.Testing;
using System.Text;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tests.ViewModel
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
            );
        }

        static string Serialize<T>(T viewModel, out JObject encryptedValues, bool isPostback = false)
        {
            encryptedValues = new JObject();
            var settings = DefaultSerializerSettingsProvider.Instance.Settings;
            var serializer = JsonSerializer.Create(settings);
            serializer.Converters.Add(CreateConverter(isPostback, encryptedValues));

            var output = new StringWriter();
            serializer.Serialize(output, viewModel);
            return output.ToString();
        }

        static T Populate<T>(string json, JObject encryptedValues = null)
        {
            var settings = DefaultSerializerSettingsProvider.Instance.Settings;
            var serializer = JsonSerializer.Create(settings);
            serializer.Converters.Add(CreateConverter(true, encryptedValues));

            //serializer.Populate(new StringReader(json), viewModel);
            return serializer.Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }

        static (T vm, JObject json) SerializeAndDeserialize<T>(T viewModel, bool isPostback = false)
        {
            var json = Serialize<T>(viewModel, out var encryptedValues, isPostback);
            var viewModel2 = Populate<T>(json, encryptedValues);
            return (viewModel2, JObject.Parse(json));
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
        [DataRow(null)]
        [DataRow(new byte[] { })]
        [DataRow(new byte[] { 1 })]
        [DataRow(new byte[] { 1, 2, 3 })]
        public void CustomJsonConverters_ByteArray(byte[] array)
        {
            using var stream = new MemoryStream();
            // Serialize array
            using (var writer = new JsonTextWriter(new StreamWriter(stream, Encoding.UTF8, leaveOpen: true)))
            {
                new DotvvmByteArrayConverter().WriteJson(writer, array, new JsonSerializer());
                writer.Flush();
            }

            // Deserialize array
            stream.Position = 0;
            byte[] deserialized;
            using (var reader = new JsonTextReader(new StreamReader(stream, Encoding.UTF8)))
            {
                while (reader.TokenType == JsonToken.None)
                    reader.Read();

                deserialized = (byte[])new DotvvmByteArrayConverter().ReadJson(reader, typeof(byte[]), null, new JsonSerializer());
            }

            CollectionAssert.AreEqual(array, deserialized);
        }

        [TestMethod]
        public void ViewModelWithByteArray()
        {
            var obj = new TestViewModelWithByteArray() {
                Bytes = new byte[] { 1, 2, 3 }
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            CollectionAssert.AreEqual(obj.Bytes, obj2.Bytes);
            Assert.AreEqual(1, (int)json["Bytes"][0]);
            Assert.AreEqual(2, (int)json["Bytes"][1]);
            Assert.AreEqual(3, (int)json["Bytes"][2]);

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
            var obj2 = SerializeAndDeserialize(obj, isPostback: true).vm;

            Assert.AreEqual(obj.P1, obj2.P1);
            Assert.AreEqual(obj.P2, obj2.P2);
            Assert.IsTrue(obj.P3.SequenceEqual(obj2.P3));
            Assert.AreEqual(obj.P4.a, obj2.P4.a);
            Assert.AreEqual(obj.P4.b.P1, obj2.P4.b.P1);
            Assert.AreEqual(obj.P4.b.P2, obj2.P4.b.P2);
            Assert.AreEqual("default", obj2.P4.b.ServerToClient);
            Assert.AreEqual("default", obj2.P4.b.ClientToServer);
        }

        [TestMethod]
        public void SupportBasicRecord()
        {
            var obj = new TestViewModelWithRecords() {
                Primitive = 10
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual(obj.Primitive, obj2.Primitive);
            Assert.AreEqual(obj.A, obj2.A);
            Assert.AreEqual(obj.Primitive, (int)json["Primitive"]);
        }
        [TestMethod]
        public void SupportConstructorRecord()
        {
            var obj = new TestViewModelWithRecords() {
                A = new (1, "ahoj")
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual(obj.A, obj2.A);
            Assert.AreEqual(obj.A.X, obj2.A.X);
            Assert.AreEqual(1, (int)json["A"]["X"]);
            Assert.AreEqual("ahoj", (string)json["A"]["Y"]);
        }
        [TestMethod]
        public void SupportConstructorRecordWithProperty()
        {
            var obj = new TestViewModelWithRecords() {
                B = new (1, "ahoj") { Z = "zz" }
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual(obj.B, obj2.B);
            Assert.AreEqual(1, obj2.B.X);
            Assert.AreEqual("zz", obj2.B.Z);
            Assert.AreEqual("zz", (string)json["B"]["Z"]);
            Assert.AreEqual("ahoj", (string)json["B"]["Y"]);
        }
        [TestMethod]
        public void SupportStructRecord()
        {
            var obj = new TestViewModelWithRecords() {
                C = new (1, "ahoj")
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual(obj.C, obj2.C);
            Assert.AreEqual(1, obj2.C.X);
            Assert.AreEqual(1, (int)json["C"]["X"]);
            Assert.AreEqual("ahoj", (string)json["C"]["Y"]);
        }
        [TestMethod]
        public void SupportMutableStruct()
        {
            var obj = new TestViewModelWithRecords() {
                D = new() { X = 1, Y = "ahoj" }
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual(1, (int)json["D"]["X"]);
            Assert.AreEqual("ahoj", (string)json["D"]["Y"]);
            Assert.AreEqual(obj.D.Y, obj2.D.Y);
            Assert.AreEqual(obj.D.X, obj2.D.X);
            Assert.AreEqual(1, obj2.D.X);
        }

        [TestMethod]
        public void SupportRecordWithGridDataSet()
        {
            var obj = new TestViewModelWithRecords() {
                E = new(new GridViewDataSet<string>() { Items = new List<string> { "a", "b", "c" } })
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual(0, (int)json["E"]["Dataset"]["PagingOptions"]["PageIndex"]);
            CollectionAssert.AreEqual(obj.E.Dataset.Items.ToArray(), obj2.E.Dataset.Items.ToArray());
            Assert.AreEqual(obj.E.Dataset.PagingOptions.PageIndex, obj2.E.Dataset.PagingOptions.PageIndex);
        }

        [TestMethod]
        public void SupportViewModelWithGridDataSet()
        {
            var obj = new TestViewModelWithDataset() {
                NoInit = new GridViewDataSet<string>() { Items = new List<string> { "a", "b", "c" } },
                Preinitialized = { Items = new List<string> { "d", "e", "f" }, PagingOptions = { PageSize = 1 } }
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual(1, (int)json["Preinitialized"]["PagingOptions"]["PageSize"]);
            CollectionAssert.AreEqual(obj.NoInit.Items.ToArray(), obj2.NoInit.Items.ToArray());
            CollectionAssert.AreEqual(obj.Preinitialized.Items.ToArray(), obj2.Preinitialized.Items.ToArray());
            Assert.AreEqual(obj.Preinitialized.PagingOptions.PageSize, obj2.Preinitialized.PagingOptions.PageSize);
            Assert.AreEqual("AAA", obj.Preinitialized.SortingOptions.SortExpression);
            Assert.AreEqual("AAA", obj2.Preinitialized.SortingOptions.SortExpression);
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

    public class TestViewModelWithByteArray
    {
        public byte[] Bytes { get; set; }
    }

    public class TestViewModelWithCollectionOfNestedProtectedData
    {
        [Protect(ProtectMode.SignData)]
        public List<DataNode> Collection { get; set; }

        [Protect(ProtectMode.SignData)]
        public List<List<DataNode>> Matrix { get; set; }
    }

    public class TestViewModelWithDataset
    {
        public GridViewDataSet<string> Preinitialized { get; set; } = new GridViewDataSet<string> { SortingOptions = { SortExpression = "AAA" } };
        public GridViewDataSet<string> NoInit { get; set; }
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

    public record TestViewModelWithRecords
    {
        public ImmutableRecord A { get; set; }
        public RecordWithAdditionalField B { get; set; }
        public StructRecord C { get; set; }
        public MutableStruct D { get; set; }
        public WithDataset E { get; set; }

        public int Primitive { get; set; }

        public record ImmutableRecord(int X, string Y);

        public record RecordWithAdditionalField(int X, string Y)
        {
            public string Z { get; set; }
        }


        public record struct StructRecord(int X, string Y);

        public struct MutableStruct
        {
            public int X { get; set; }
            public string Y { get; set; }
        }

        public record WithDataset(GridViewDataSet<string> Dataset);
    }
}
