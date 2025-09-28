using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using STJ = System.Text.Json;
using STJS = System.Text.Json.Serialization;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Utils;
using System.Text;
using DotVVM.Framework.Controls;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Collections.Immutable;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class SerializerTests
    {
        static ViewModelJsonConverter jsonConverter = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<ViewModelJsonConverter>();
        static JsonSerializerOptions jsonOptions = new JsonSerializerOptions(DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe) {
            Converters = { jsonConverter },
            WriteIndented = true
        };
        static DotvvmSerializationState CreateState(bool isPostback, JsonObject readEncryptedValues = null)
        {
            var config = DotvvmTestHelper.DefaultConfig;
            return DotvvmSerializationState.Create(
                isPostback,
                config.ServiceProvider,
                readEncryptedValues is null ? null : new JsonObject([ new("0", readEncryptedValues) ])
            );
        }

        static string Serialize<T>(T viewModel, out JsonObject encryptedValues, bool isPostback = false)
        {
            using var state = CreateState(isPostback, null);

            var json = STJ.JsonSerializer.Serialize(viewModel, jsonOptions);
            var ev = state.WriteEncryptedValues.ToSpan();
            encryptedValues = ev.Length > 0 ? JsonNode.Parse(ev).AsObject() : new JsonObject();
            return json;
        }

        static T Deserialize<T>(string json, JsonObject encryptedValues = null)
        {
            using var state = CreateState(false, encryptedValues ?? new JsonObject());

            return STJ.JsonSerializer.Deserialize<T>(json, jsonOptions);
        }

        static T PopulateViewModel<T>(string json, T existingValue, JsonObject encryptedValues = null)
        {
            using var state = CreateState(true, encryptedValues ?? new JsonObject());
            var specificConverter = jsonConverter.CreateConverter<T>();
            var jsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            return (T)specificConverter.Populate(ref jsonReader, typeof(T), existingValue, jsonOptions, state);
        }

        internal static (T vm, JsonObject json) SerializeAndDeserialize<T>(T viewModel, bool isPostback = false)
        {
            var json = Serialize<T>(viewModel, out var encryptedValues, isPostback);
            var viewModel2 = Deserialize<T>(json, encryptedValues);
            return (viewModel2, JsonNode.Parse(json).AsObject());
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
            var deserialized = Deserialize<TestViewModelWithNestedProtectedData>(serialized, encryptedValues);
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
            var deserialized = Deserialize<TestViewModelWithCollectionOfNestedProtectedData>(serialized, encryptedValues);
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
            var deserialized = Deserialize<TestViewModelWithCollectionOfNestedProtectedData>(serialized, encryptedValues);
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
            var deserialized = Deserialize<TestViewModelWithNestedProtectedData>(serialized, encryptedValues);
            Assert.AreEqual(serialized, Serialize(deserialized, out var _, false));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(new byte[] { })]
        [DataRow(new byte[] { 1 })]
        [DataRow(new byte[] { 1, 2, 3 })]
        public void CustomJsonConverters_ByteArray(byte[] array)
        {
            var converter = new DotvvmByteArrayConverter();
            using var stream = new MemoryStream();
            // Serialize array
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
            {
                converter.Write(writer, array, DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
            }

            // Deserialize array
            byte[] deserialized;
            var reader = new Utf8JsonReader(stream.ToSpan());
            reader.Read();
            deserialized = (byte[])converter.Read(ref reader, typeof(byte[]), DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);

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
            var (obj2, json) = SerializeAndDeserialize(obj, isPostback: true);

            Assert.AreEqual("""{"$type":"pUM6BN1XpGhNArrP","Item1":9,"Item2":8,"Item3":7,"Item4":6}""", json["P1"].ToJsonString(new JsonSerializerOptions { WriteIndented = false }));
            Assert.AreEqual("""{"$type":"VnTd1CsMIOOD62hn","Key":3,"Value":4}""", json["P3"][0].ToJsonString(new JsonSerializerOptions { WriteIndented = false }));
            Assert.AreEqual("""{"$type":"wAhAjOT9J9ARTgbN","Item1":5,"Item2":6,"Item3":7,"Item4":8}""", json["P2"].ToJsonString(new JsonSerializerOptions { WriteIndented = false }));

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
        public void SupportTuplesPopulate()
        {
            var obj = new TestViewModelWithTuples() {
                P4 = (10, new TestViewModelWithBind { P1 = "1" }),
                P5 = new (new TestViewModelWithBind { P1 = "2" }, new TestViewModelWithBind { P1 = "3" }),
                P6 = new(new TestViewModelWithBind { P1 = "4" }, 5)
            };
            var json = Serialize(obj, out var _, isPostback: true);
            var obj2 = new TestViewModelWithTuples() {
                P4 = (0, new TestViewModelWithBind()),
                P5 = new (new TestViewModelWithBind(), new TestViewModelWithBind()),
                P6 = new (new TestViewModelWithBind(), 0)
            };
            var originalInstances = (
                P4: obj2.P4.b,
                P5Key: obj2.P5.Key,
                P5Value: obj2.P5.Value,
                P6a: obj2.P6.Item1
            );
            var objPopulated = PopulateViewModel(json, obj2);
            Assert.AreEqual(10, objPopulated.P4.a);
            Assert.AreEqual("1", objPopulated.P4.b.P1);
            Assert.AreEqual("2", objPopulated.P5.Key.P1);
            Assert.AreEqual("3", objPopulated.P5.Value.P1);
            Assert.AreEqual("4", objPopulated.P6.Item1.P1);
            Assert.AreEqual(5, objPopulated.P6.Item2);
            Assert.AreSame(objPopulated, obj2);
            Assert.AreSame(originalInstances.P4, objPopulated.P4.b);
            Assert.AreSame(originalInstances.P5Key, objPopulated.P5.Key);
            Assert.AreSame(originalInstances.P5Value, objPopulated.P5.Value);
            Assert.AreSame(originalInstances.P6a, objPopulated.P6.Item1);
        }

        [TestMethod]
        public void SupportCollectionPopulate()
        {
            var obj = new TestViewModelWithCollections() {
                ViewModels = new List<TestViewModelWithBind> {
                    new TestViewModelWithBind { P1 = "Item1" },
                    new TestViewModelWithBind { P1 = "Item2" },
                    new TestViewModelWithBind { P1 = "Item3" }
                },
                Strings = new List<string> { "A", "B", "C" }
            };
            var json = Serialize(obj, out var _, isPostback: true);
            Console.WriteLine($"JSON: {json}");

            var obj2 = new TestViewModelWithCollections() {
                ViewModels = new List<TestViewModelWithBind> {
                    new TestViewModelWithBind { P1 = "Old1" },
                    new TestViewModelWithBind { P1 = "Old2" }
                },
                Strings = new List<string> { "X", "Y" }
            };
            var originalInstances = (
                Collection: obj2.ViewModels,
                Item0: obj2.ViewModels[0],
                Item1_: obj2.ViewModels[1], // avoid name conflict
                StringCollection: obj2.Strings
            );
            var objPopulated = PopulateViewModel(json, obj2);

            // The collection instances should be preserved
            Assert.AreSame(objPopulated, obj2);
            Assert.AreSame(originalInstances.Collection, objPopulated.ViewModels);
            Assert.AreSame(originalInstances.StringCollection, objPopulated.Strings);

            // The collection should have the new content
            Assert.AreEqual(3, objPopulated.ViewModels.Count);
            Assert.AreEqual("Item1", objPopulated.ViewModels[0].P1);
            Assert.AreEqual("Item2", objPopulated.ViewModels[1].P1);
            Assert.AreEqual("Item3", objPopulated.ViewModels[2].P1);

            Assert.AreEqual(3, objPopulated.Strings.Count);
            Assert.AreEqual("A", objPopulated.Strings[0]);
            Assert.AreEqual("B", objPopulated.Strings[1]);
            Assert.AreEqual("C", objPopulated.Strings[2]);

            // The existing view model instances should be populated (not replaced)
            Assert.AreSame(originalInstances.Item0, objPopulated.ViewModels[0]);
            Assert.AreSame(originalInstances.Item1_, objPopulated.ViewModels[1]);

            Assert.AreEqual("Item1", originalInstances.Item0.P1); // was "Old1"
            Assert.AreEqual("Item2", originalInstances.Item1_.P1); // was "Old2"
        }

        [TestMethod]
        public void SupportNestedCollectionPopulate()
        {
            // Clear cache to ensure TestViewModelWithNestedCollections uses the registered DotvvmCollectionConverter
            var mapper = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();
            if (mapper is ViewModelSerializationMapper concreteMapper)
            {
                concreteMapper.ClearCache(typeof(TestViewModelWithNestedCollections));
            }

            var obj = new TestViewModelWithNestedCollections() {
                Matrix = new List<List<TestViewModelWithBind>> {
                    new List<TestViewModelWithBind> {
                        new TestViewModelWithBind { P1 = "Row1Col1" },
                        new TestViewModelWithBind { P1 = "Row1Col2" }
                    },
                    new List<TestViewModelWithBind> {
                        new TestViewModelWithBind { P1 = "Row2Col1" }
                    }
                }
            };
            var json = Serialize(obj, out var _, isPostback: true);
            var obj2 = new TestViewModelWithNestedCollections() {
                Matrix = new List<List<TestViewModelWithBind>> {
                    new List<TestViewModelWithBind> {
                        new TestViewModelWithBind { P1 = "OldValue" }
                    }
                }
            };
            var originalInstances = (
                Matrix: obj2.Matrix,
                Row0: obj2.Matrix[0],
                Item00: obj2.Matrix[0][0]
            );
            var objPopulated = PopulateViewModel(json, obj2);

            // The collection instances should be preserved
            Assert.AreSame(objPopulated, obj2);
            Assert.AreSame(originalInstances.Matrix, objPopulated.Matrix);
            Assert.AreSame(originalInstances.Row0, objPopulated.Matrix[0]);

            // The nested collection should have the new content
            Assert.AreEqual(2, objPopulated.Matrix.Count);
            Assert.AreEqual(2, objPopulated.Matrix[0].Count);
            Assert.AreEqual(1, objPopulated.Matrix[1].Count);
            Assert.AreEqual("Row1Col1", objPopulated.Matrix[0][0].P1);
            Assert.AreEqual("Row1Col2", objPopulated.Matrix[0][1].P1);
            Assert.AreEqual("Row2Col1", objPopulated.Matrix[1][0].P1);

            // The existing view model instance should be populated (not replaced)
            Assert.AreSame(originalInstances.Item00, objPopulated.Matrix[0][0]);

            // Verify that the property was actually updated from its original value
            // (this confirms the instance was populated, not just that the reference is preserved)
            Assert.AreEqual("Row1Col1", originalInstances.Item00.P1); // was "OldValue"
        }

        [TestMethod]
        public void SupportNestedArraysPopulate()
        {
            var obj = new TestViewModelWithNestedCollections() {
                NestedArrays = new List<List<TestViewModelWithBind[]>> {
                    new List<TestViewModelWithBind[]> {
                        new TestViewModelWithBind[] {
                            new TestViewModelWithBind { P1 = "Row0-Array0-Item0" },
                            new TestViewModelWithBind { P1 = "Row0-Array0-Item1" }
                        },
                        new TestViewModelWithBind[] {
                            new TestViewModelWithBind { P1 = "Row0-Array1-Item0" }
                        }
                    },
                    new List<TestViewModelWithBind[]> {
                        new TestViewModelWithBind[] {
                            new TestViewModelWithBind { P1 = "Row1-Array0-Item0" },
                            new TestViewModelWithBind { P1 = "Row1-Array0-Item1" },
                            new TestViewModelWithBind { P1 = "Row1-Array0-Item2" }
                        }
                    }
                }
            };
            var json = Serialize(obj, out var _, isPostback: true);

            var obj2 = new TestViewModelWithNestedCollections() {
                NestedArrays = new List<List<TestViewModelWithBind[]>> {
                    new List<TestViewModelWithBind[]> {
                        new TestViewModelWithBind[] {
                            new TestViewModelWithBind { P1 = "Old-Row0-Array0-Item0" }
                        }
                    }
                }
            };

            // Store references to existing instances for verification
            var originalInstances = (
                RootCollection: obj2.NestedArrays,
                InnerList: obj2.NestedArrays[0],
                Array: obj2.NestedArrays[0][0],
                ViewModel: obj2.NestedArrays[0][0][0]
            );

            var objPopulated = PopulateViewModel(json, obj2);

            // The root collection instance should be preserved
            Assert.AreSame(objPopulated, obj2);
            Assert.AreSame(originalInstances.RootCollection, objPopulated.NestedArrays);

            // Verify the nested structure was populated correctly
            Assert.AreEqual(2, objPopulated.NestedArrays.Count);

            // First row
            Assert.AreEqual(2, objPopulated.NestedArrays[0].Count);
            Assert.AreEqual(2, objPopulated.NestedArrays[0][0].Length);
            Assert.AreEqual("Row0-Array0-Item0", objPopulated.NestedArrays[0][0][0].P1);
            Assert.AreEqual("Row0-Array0-Item1", objPopulated.NestedArrays[0][0][1].P1);
            Assert.AreEqual(1, objPopulated.NestedArrays[0][1].Length);
            Assert.AreEqual("Row0-Array1-Item0", objPopulated.NestedArrays[0][1][0].P1);

            // Second row
            Assert.AreEqual(1, objPopulated.NestedArrays[1].Count);
            Assert.AreEqual(3, objPopulated.NestedArrays[1][0].Length);
            Assert.AreEqual("Row1-Array0-Item0", objPopulated.NestedArrays[1][0][0].P1);
            Assert.AreEqual("Row1-Array0-Item1", objPopulated.NestedArrays[1][0][1].P1);
            Assert.AreEqual("Row1-Array0-Item2", objPopulated.NestedArrays[1][0][2].P1);

            // The inner list should be preserved and populated (not replaced)
            Assert.AreSame(originalInstances.InnerList, objPopulated.NestedArrays[0]);

            // Arrays cannot be populated in-place, so they are replaced
            Assert.AreNotSame(originalInstances.Array, objPopulated.NestedArrays[0][0]);

            // However, the view model instances within arrays should still be preserved when possible
            // Let's test this by checking if we can preserve view models during array creation
            Assert.AreSame(originalInstances.ViewModel, objPopulated.NestedArrays[0][0][0]);

            // Verify that the view model property was actually updated from its original value
            // (this confirms the instance was populated, not just that the reference is preserved)
            Assert.AreEqual("Row0-Array0-Item0", originalInstances.ViewModel.P1); // was "Old-Row0-Array0-Item0"
        }

        [TestMethod]
        public void PolymorphicCollectionPopulate_NonAbstractBaseDoesNotWork()
        {
            var obj = new TestViewModelWithPolymorphicCollection() {
                Items = new List<BaseItem> {
                    new DerivedItemA { BaseProperty = "Base1", DerivedAProperty = "DerivedA1" },
                    new DerivedItemB { BaseProperty = "Base2", DerivedBProperty = "DerivedB1" },
                    new DerivedItemA { BaseProperty = "Base3", DerivedAProperty = "DerivedA2" }
                }
            };
            var json = Serialize(obj, out var _, isPostback: true);

            var obj2 = new TestViewModelWithPolymorphicCollection() {
                Items = new List<BaseItem> {
                    new DerivedItemA { BaseProperty = "OldBase1", DerivedAProperty = "OldDerivedA1" },
                    new DerivedItemB { BaseProperty = "OldBase2", DerivedBProperty = "OldDerivedB1" }
                }
            };

            var originalInstances = (
                Collection: obj2.Items,
                ItemA: (DerivedItemA)obj2.Items[0],
                ItemB: (DerivedItemB)obj2.Items[1]
            );

            var objPopulated = PopulateViewModel(json, obj2);

            Assert.AreSame(objPopulated, obj2);
            Assert.AreSame(originalInstances.Collection, objPopulated.Items);

            Assert.AreEqual(3, objPopulated.Items.Count);

            Assert.IsInstanceOfType(objPopulated.Items[0], typeof(DerivedItemA));
            Assert.AreSame(originalInstances.ItemA, objPopulated.Items[0]);
            var firstItem = (DerivedItemA)objPopulated.Items[0];
            Assert.AreEqual("Base1", firstItem.BaseProperty);
            Assert.AreEqual("OldDerivedA1", firstItem.DerivedAProperty);

            Assert.IsInstanceOfType(objPopulated.Items[1], typeof(DerivedItemB));
            Assert.AreSame(originalInstances.ItemB, objPopulated.Items[1]);
            var secondItem = (DerivedItemB)objPopulated.Items[1];
            Assert.AreEqual("Base2", secondItem.BaseProperty);
            Assert.AreEqual("OldDerivedB1", secondItem.DerivedBProperty);

            var thirdItem = objPopulated.Items[2];
            Assert.AreEqual(thirdItem.GetType(), typeof(BaseItem));
            Assert.AreEqual("Base3", thirdItem.BaseProperty);
        }

        [TestMethod]
        public void SupportAbstractPolymorphicCollectionPopulate()
        {
            var obj = new TestViewModelWithAbstractPolymorphicCollection() {
                Items = new List<AbstractBaseItem> {
                    new AbstractDerivedItemA { BaseProperty = "Base1", DerivedAProperty = "DerivedA1" },
                    new AbstractDerivedItemB { BaseProperty = "Base2", DerivedBProperty = "DerivedB1" }
                }
            };
            var json = Serialize(obj, out var _, isPostback: true);

            var obj2 = new TestViewModelWithAbstractPolymorphicCollection() {
                Items = new List<AbstractBaseItem> {
                    new AbstractDerivedItemA { BaseProperty = "OldBase1", DerivedAProperty = "OldDerivedA1" },
                    new AbstractDerivedItemB { BaseProperty = "OldBase2", DerivedBProperty = "OldDerivedB1" }
                }
            };

            var originalInstances = (
                Collection: obj2.Items,
                ItemA: (AbstractDerivedItemA)obj2.Items[0],
                ItemB: (AbstractDerivedItemB)obj2.Items[1]
            );

            var objPopulated = PopulateViewModel(json, obj2);

            // The collection instance should be preserved
            Assert.AreSame(objPopulated, obj2);
            Assert.AreSame(originalInstances.Collection, objPopulated.Items);

            // Verify the polymorphic items were populated correctly
            Assert.AreEqual(2, objPopulated.Items.Count);

            // First item - AbstractDerivedItemA - should be preserved and updated
            Assert.IsInstanceOfType(objPopulated.Items[0], typeof(AbstractDerivedItemA));
            Assert.AreSame(originalInstances.ItemA, objPopulated.Items[0]);
            var firstItem = (AbstractDerivedItemA)objPopulated.Items[0];
            Assert.AreEqual("Base1", firstItem.BaseProperty);
            Assert.AreEqual("DerivedA1", firstItem.DerivedAProperty);

            // Second item - AbstractDerivedItemB - should be preserved and updated
            Assert.IsInstanceOfType(objPopulated.Items[1], typeof(AbstractDerivedItemB));
            Assert.AreSame(originalInstances.ItemB, objPopulated.Items[1]);
            var secondItem = (AbstractDerivedItemB)objPopulated.Items[1];
            Assert.AreEqual("Base2", secondItem.BaseProperty);
            Assert.AreEqual("DerivedB1", secondItem.DerivedBProperty);
        }

        [TestMethod]
        public void SupportKeyValuePairCollection()
        {
            var obj = new TestViewModelWithAdvancedCollections()
            {
                KeyValuePairs = new List<KeyValuePair<string, TestViewModelWithBind>>
                {
                    new KeyValuePair<string, TestViewModelWithBind>("key1", new TestViewModelWithBind { P1 = "Item1", P2 = "Value1" }),
                    new KeyValuePair<string, TestViewModelWithBind>("key2", new TestViewModelWithBind { P1 = "Item2", P2 = "Value2" }),
                    new KeyValuePair<string, TestViewModelWithBind>("key3", new TestViewModelWithBind { P1 = "Item3", P2 = "Value3" })
                }
            };
            var json = Serialize(obj, out var _, isPostback: true);

            var obj2 = new TestViewModelWithAdvancedCollections()
            {
                KeyValuePairs = new List<KeyValuePair<string, TestViewModelWithBind>>
                {
                    new KeyValuePair<string, TestViewModelWithBind>("key1", new TestViewModelWithBind { P1 = "OldItem1", P2 = "OldValue1" }),
                    new KeyValuePair<string, TestViewModelWithBind>("key2", new TestViewModelWithBind { P1 = "OldItem2", P2 = "OldValue2" })
                }
            };

            var originalInstances = (
                Collection: obj2.KeyValuePairs,
                FirstValue: obj2.KeyValuePairs[0].Value,
                SecondValue: obj2.KeyValuePairs[1].Value
            );

            var objPopulated = PopulateViewModel(json, obj2);

            // The collection instance should be preserved
            Assert.AreSame(objPopulated, obj2);
            Assert.AreSame(originalInstances.Collection, objPopulated.KeyValuePairs);

            // Verify the key-value pairs were populated correctly
            Assert.AreEqual(3, objPopulated.KeyValuePairs.Count);
            Assert.AreEqual("key1", objPopulated.KeyValuePairs[0].Key);
            Assert.AreEqual("key2", objPopulated.KeyValuePairs[1].Key);
            Assert.AreEqual("key3", objPopulated.KeyValuePairs[2].Key);

            // First two values should be preserved and updated
            Assert.AreSame(originalInstances.FirstValue, objPopulated.KeyValuePairs[0].Value);
            Assert.AreSame(originalInstances.SecondValue, objPopulated.KeyValuePairs[1].Value);
            Assert.AreEqual("Item1", objPopulated.KeyValuePairs[0].Value.P1);
            Assert.AreEqual("Item2", objPopulated.KeyValuePairs[1].Value.P1);

            // Third value should be a new instance
            Assert.AreNotSame(originalInstances.FirstValue, objPopulated.KeyValuePairs[2].Value);
            Assert.AreEqual("Item3", objPopulated.KeyValuePairs[2].Value.P1);
        }

        [TestMethod] 
        public void SupportDictionaryCollection()
        {
            var obj = new TestViewModelWithAdvancedCollections()
            {
                Dictionary = new Dictionary<string, TestViewModelWithBind>
                {
                    ["key1"] = new TestViewModelWithBind { P1 = "Item1", P2 = "Value1" },
                    ["key2"] = new TestViewModelWithBind { P1 = "Item2", P2 = "Value2" },
                    ["key3"] = new TestViewModelWithBind { P1 = "Item3", P2 = "Value3" }
                }
            };
            var json = Serialize(obj, out var _, isPostback: true);

            var obj2 = new TestViewModelWithAdvancedCollections()
            {
                Dictionary = new Dictionary<string, TestViewModelWithBind>
                {
                    // Note: Different order to test that population works regardless of order
                    ["key2"] = new TestViewModelWithBind { P1 = "OldItem2", P2 = "OldValue2" },
                    ["key1"] = new TestViewModelWithBind { P1 = "OldItem1", P2 = "OldValue1" }
                }
            };

            var originalInstances = (
                Dictionary: obj2.Dictionary,
                FirstValue: obj2.Dictionary["key1"],
                SecondValue: obj2.Dictionary["key2"]
            );

            var objPopulated = PopulateViewModel(json, obj2);

            // The dictionary instance should be preserved
            Assert.AreSame(objPopulated, obj2);
            Assert.AreSame(originalInstances.Dictionary, objPopulated.Dictionary);

            // Verify the dictionary was populated correctly
            Assert.AreEqual(3, objPopulated.Dictionary.Count);
            Assert.IsTrue(objPopulated.Dictionary.ContainsKey("key1"));
            Assert.IsTrue(objPopulated.Dictionary.ContainsKey("key2"));
            Assert.IsTrue(objPopulated.Dictionary.ContainsKey("key3"));

            // Existing values should be preserved and updated
            Assert.AreSame(originalInstances.FirstValue, objPopulated.Dictionary["key1"]);
            Assert.AreSame(originalInstances.SecondValue, objPopulated.Dictionary["key2"]);
            Assert.AreEqual("Item1", objPopulated.Dictionary["key1"].P1);
            Assert.AreEqual("Item2", objPopulated.Dictionary["key2"].P1);

            // New value should be a new instance
            Assert.AreNotSame(originalInstances.FirstValue, objPopulated.Dictionary["key3"]);
            Assert.AreEqual("Item3", objPopulated.Dictionary["key3"].P1);
        }

        [TestMethod]
        public void SupportImmutableArrayCollection()
        {
            var obj = new TestViewModelWithAdvancedCollections()
            {
                ImmutableArray = ImmutableArray.Create(
                    new TestViewModelWithBind { P1 = "Item1", P2 = "Value1" },
                    new TestViewModelWithBind { P1 = "Item2", P2 = "Value2" },
                    new TestViewModelWithBind { P1 = "Item3", P2 = "Value3" }
                )
            };
            var json = Serialize(obj, out var _, isPostback: true);

            var obj2 = new TestViewModelWithAdvancedCollections()
            {
                ImmutableArray = ImmutableArray.Create(
                    new TestViewModelWithBind { P1 = "OldItem1", P2 = "OldValue1" },
                    new TestViewModelWithBind { P1 = "OldItem2", P2 = "OldValue2" }
                )
            };

            var originalInstances = (
                FirstItem: obj2.ImmutableArray[0],
                SecondItem: obj2.ImmutableArray[1]
            );

            var objPopulated = PopulateViewModel(json, obj2);

            // The root object should be preserved
            Assert.AreSame(objPopulated, obj2);

            // Verify the immutable array was populated correctly
            Assert.AreEqual(3, objPopulated.ImmutableArray.Length);

            // For immutable collections, we can't preserve the collection instance itself,
            // but we should still be able to preserve individual view model instances where possible
            Assert.AreSame(originalInstances.FirstItem, objPopulated.ImmutableArray[0]);
            Assert.AreSame(originalInstances.SecondItem, objPopulated.ImmutableArray[1]);
            Assert.AreEqual("Item1", objPopulated.ImmutableArray[0].P1);
            Assert.AreEqual("Item2", objPopulated.ImmutableArray[1].P1);

            // Third item should be a new instance
            Assert.AreNotSame(originalInstances.FirstItem, objPopulated.ImmutableArray[2]);
            Assert.AreEqual("Item3", objPopulated.ImmutableArray[2].P1);
        }

        [TestMethod]
        public void SupportImmutableListCollection()
        {
            var obj = new TestViewModelWithAdvancedCollections()
            {
                ImmutableList = ImmutableList.Create(
                    new TestViewModelWithBind { P1 = "Item1", P2 = "Value1" },
                    new TestViewModelWithBind { P1 = "Item2", P2 = "Value2" },
                    new TestViewModelWithBind { P1 = "Item3", P2 = "Value3" }
                )
            };
            var json = Serialize(obj, out var _, isPostback: true);

            var obj2 = new TestViewModelWithAdvancedCollections()
            {
                ImmutableList = ImmutableList.Create(
                    new TestViewModelWithBind { P1 = "OldItem1", P2 = "OldValue1" },
                    new TestViewModelWithBind { P1 = "OldItem2", P2 = "OldValue2" }
                )
            };

            var originalInstances = (
                FirstItem: obj2.ImmutableList[0],
                SecondItem: obj2.ImmutableList[1]
            );

            var objPopulated = PopulateViewModel(json, obj2);

            // The root object should be preserved
            Assert.AreSame(objPopulated, obj2);

            // Verify the immutable list was populated correctly
            Assert.AreEqual(3, objPopulated.ImmutableList.Count);

            // For immutable collections, we can't preserve the collection instance itself,
            // but we should still be able to preserve individual view model instances where possible
            Assert.AreSame(originalInstances.FirstItem, objPopulated.ImmutableList[0]);
            Assert.AreSame(originalInstances.SecondItem, objPopulated.ImmutableList[1]);
            Assert.AreEqual("Item1", objPopulated.ImmutableList[0].P1);
            Assert.AreEqual("Item2", objPopulated.ImmutableList[1].P1);

            // Third item should be a new instance
            Assert.AreNotSame(originalInstances.FirstItem, objPopulated.ImmutableList[2]);
            Assert.AreEqual("Item3", objPopulated.ImmutableList[2].P1);
        }

        [TestMethod]
        public void SupportImmutableDictionaryCollection()
        {
            var obj = new TestViewModelWithAdvancedCollections()
            {
                ImmutableDictionary = ImmutableDictionary<string, TestViewModelWithBind>.Empty
                    .Add("key1", new TestViewModelWithBind { P1 = "Item1", P2 = "Value1" })
                    .Add("key2", new TestViewModelWithBind { P1 = "Item2", P2 = "Value2" })
                    .Add("key3", new TestViewModelWithBind { P1 = "Item3", P2 = "Value3" })
            };
            var json = Serialize(obj, out var _, isPostback: true);

            var obj2 = new TestViewModelWithAdvancedCollections()
            {
                ImmutableDictionary = ImmutableDictionary<string, TestViewModelWithBind>.Empty
                    .Add("key2", new TestViewModelWithBind { P1 = "OldItem2", P2 = "OldValue2" })
                    .Add("key1", new TestViewModelWithBind { P1 = "OldItem1", P2 = "OldValue1" })
            };

            var originalInstances = (
                FirstValue: obj2.ImmutableDictionary["key1"],
                SecondValue: obj2.ImmutableDictionary["key2"]
            );

            var objPopulated = PopulateViewModel(json, obj2);

            // The root object should be preserved
            Assert.AreSame(objPopulated, obj2);

            // Verify the immutable dictionary was populated correctly
            Assert.AreEqual(3, objPopulated.ImmutableDictionary.Count);
            Assert.IsTrue(objPopulated.ImmutableDictionary.ContainsKey("key1"));
            Assert.IsTrue(objPopulated.ImmutableDictionary.ContainsKey("key2"));
            Assert.IsTrue(objPopulated.ImmutableDictionary.ContainsKey("key3"));

            // For immutable collections, we can't preserve the collection instance itself,
            // but we should still be able to preserve individual view model instances where possible
            Assert.AreSame(originalInstances.FirstValue, objPopulated.ImmutableDictionary["key1"]);
            Assert.AreSame(originalInstances.SecondValue, objPopulated.ImmutableDictionary["key2"]);
            Assert.AreEqual("Item1", objPopulated.ImmutableDictionary["key1"].P1);
            Assert.AreEqual("Item2", objPopulated.ImmutableDictionary["key2"].P1);

            // New value should be a new instance
            Assert.AreNotSame(originalInstances.FirstValue, objPopulated.ImmutableDictionary["key3"]);
            Assert.AreEqual("Item3", objPopulated.ImmutableDictionary["key3"].P1);
        }

        [TestMethod]
        public void DoesNotCloneSettableRecord()
        {
            var obj = new TestViewModelWithRecords() {
                Primitive = 10
            };
            var json = Serialize(obj, out var ev, false);
            var obj2 = new TestViewModelWithRecords() { Primitive = 100 };
            var obj3 = PopulateViewModel(json, obj2, ev);
            Assert.AreEqual(10, obj3.Primitive);
            Assert.IsTrue(ReferenceEquals(obj2, obj3), "The deserialized object TestViewModelWithRecords is not referenced equal to the existingValue");
            Assert.AreEqual(10, obj2.Primitive);
        }

        [TestMethod]
        public void ClonesInitOnlyClass()
        {
            var obj = new TestInitOnlyClass() { X = 10, Y = "A" };
            var json = Serialize(obj, out var ev, false);
            var obj2 = new TestInitOnlyClass() { X = 20, Y = "B" };
            var obj3 = PopulateViewModel(json, obj2, ev);
            Assert.AreEqual(10, obj3.X);
            Assert.AreEqual("A", obj3.Y);
            Assert.AreEqual(20, obj2.X, "The deserializer didn't clone TestInitOnlyClass and used the init-only setter at runtime.");
            Assert.IsFalse(ReferenceEquals(obj2, obj3), "The deserializer didn't clone TestInitOnlyClass");
            Assert.AreEqual("B", obj2.Y, "The deserializer used TestInitOnlyClass.Y setter at runtime, but then returned another instance.");
        }
        [TestMethod]
        public void ClonesInitOnlyRecord()
        {
            var obj = new TestInitOnlyRecord() { X = 10, Y = "A" };
            var json = Serialize(obj, out var ev, false);
            var obj2 = new TestInitOnlyRecord() { X = 20, Y = "B" };
            var obj3 = PopulateViewModel(json, obj2, ev);
            Assert.AreEqual(10, obj3.X);
            Assert.AreEqual("A", obj3.Y);
            Assert.AreEqual(20, obj2.X, "The deserializer didn't clone TestInitOnlyRecord and used the init-only setter at runtime.");
            Assert.IsFalse(ReferenceEquals(obj2, obj3), "The deserializer didn't clone TestInitOnlyRecord");
            Assert.AreEqual("B", obj2.Y, "The deserializer used TestInitOnlyRecord.Y setter at runtime, but then returned another instance.");
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

        [TestMethod]
        public void SupportConstructorInjection()
        {
            var service = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<DotvvmTestHelper.ITestSingletonService>();
            var obj = new ViewModelWithService("test", service);
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual(obj.Property1, obj2.Property1);
            Assert.AreEqual(obj.GetService(), obj2.GetService());
            Assert.AreEqual(obj.Property1, (string)json["Property1"]);
            Assert.IsNull(json["Service"]);
        }

        [TestMethod]
        public void SupportsSignedDictionary()
        {
            var obj = new TestViewModelWithSignedDictionary() {
                SignedDictionary = {
                    ["a"] = "x",
                    ["b"] = "y"
                }
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            CollectionAssert.Contains(obj2.SignedDictionary, new KeyValuePair<string, string>("a", "x"));
            CollectionAssert.Contains(obj2.SignedDictionary, new KeyValuePair<string, string>("b", "y"));
            Assert.AreEqual(obj.SignedDictionary.Count, obj2.SignedDictionary.Count);
            XAssert.IsType<JsonArray>(json["SignedDictionary"]);
        }

        [TestMethod]
        public void SupportsDateTime()
        {
            var obj = new TestViewModelWithDateTimes() {
                DateTime1 = new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc),
                DateTime2 = new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Local),
                DateTime3 = new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Unspecified),
                DateOnly = new DateOnly(2000, 1, 1),
                TimeOnly = new TimeOnly(15, 0, 0)
            };
            var (obj2, json) = SerializeAndDeserialize(obj);
            Console.WriteLine(json);
            Assert.AreEqual(obj.DateTime1, obj2.DateTime1);
            Assert.AreEqual(DateTimeKind.Unspecified, obj2.DateTime1.Kind);
            Assert.AreEqual(obj.DateTime2, obj2.DateTime2);
            Assert.AreEqual(DateTimeKind.Unspecified, obj2.DateTime2.Kind);
            Assert.AreEqual(obj.DateTime3, obj2.DateTime3);
            Assert.AreEqual(DateTimeKind.Unspecified, obj2.DateTime3.Kind);
            Assert.AreEqual(obj.DateOnly, obj2.DateOnly);
            Assert.AreEqual(obj.TimeOnly, obj2.TimeOnly);

            Assert.AreEqual("2000-01-01", json["DateOnly"].GetValue<string>());
            Assert.AreEqual("15:00:00", json["TimeOnly"].GetValue<string>());
            Assert.AreEqual("2000-01-01T15:00:00", json["DateTime1"].GetValue<string>());
            Assert.AreEqual("2000-01-01T15:00:00", json["DateTime2"].GetValue<string>());
            Assert.AreEqual("2000-01-01T15:00:00", json["DateTime3"].GetValue<string>());
        }

        [TestMethod]
        public void SupportsDateTime_MicrosecondPrecision()
        {
            var obj = new TestViewModelWithDateTimes() {
                DateTime1 = new DateTime(2000, 1, 2, 15, 16, 17).AddMilliseconds(123.456),
                TimeOnly = new TimeOnly(15, 16, 17).Add(TimeSpan.FromMilliseconds(123.456))
            };
            var (obj2, json) = SerializeAndDeserialize(obj);
            Console.WriteLine(json);
            Assert.AreEqual(obj.DateTime1, obj2.DateTime1);
            Assert.AreEqual(DateTimeKind.Unspecified, obj2.DateTime1.Kind);
            Assert.AreEqual(obj.TimeOnly, obj2.TimeOnly);
            Assert.AreEqual(obj.TimeOnly.Ticks, obj2.TimeOnly.Ticks);

#if DotNetCore
            Assert.AreEqual("2000-01-02T15:16:17.123456", json["DateTime1"].GetValue<string>());
            Assert.AreEqual("15:16:17.1234560", json["TimeOnly"].GetValue<string>());
#else
            Assert.AreEqual("2000-01-02T15:16:17.123", json["DateTime1"].GetValue<string>());
            Assert.AreEqual("15:16:17.1230000", json["TimeOnly"].GetValue<string>());
#endif
        }

        [TestMethod]
        public void SupportsEnums()
        {
            var obj = new TestViewModelWithEnums() {
                Byte = TestViewModelWithEnums.ByteEnum.C,
                SByte = TestViewModelWithEnums.SByteEnum.B,
                UInt16 = TestViewModelWithEnums.UInt16Enum.B,
                Int16 = TestViewModelWithEnums.Int16Enum.D,
                UInt32 = TestViewModelWithEnums.UInt32Enum.B,
                Int32 = TestViewModelWithEnums.Int32Enum.D,
                UInt64 = TestViewModelWithEnums.UInt64Enum.B,
                Int64 = TestViewModelWithEnums.Int64Enum.B,
                DateTimeKind = DateTimeKind.Utc,
                DuplicateName = TestViewModelWithEnums.DuplicateNameEnum.DAndAlsoLonger,
                EnumMember = TestViewModelWithEnums.EnumMemberEnum.B,
                Int32Flags = TestViewModelWithEnums.Int32FlagsEnum.ABC,
                UInt64Flags = TestViewModelWithEnums.UInt64FlagsEnum.F1 | TestViewModelWithEnums.UInt64FlagsEnum.F2 | TestViewModelWithEnums.UInt64FlagsEnum.F64
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual("member-b", json["EnumMember"].GetValue<string>());
            Assert.AreEqual(obj.Byte, obj2.Byte);
            Assert.AreEqual(obj.SByte, obj2.SByte);
            Assert.AreEqual(obj.UInt16, obj2.UInt16);
            Assert.AreEqual(obj.Int16, obj2.Int16);
            Assert.AreEqual(obj.UInt32, obj2.UInt32);
            Assert.AreEqual(obj.Int32, obj2.Int32);
            Assert.AreEqual(obj.UInt64, obj2.UInt64);
            Assert.AreEqual(obj.Int64, obj2.Int64);
            Assert.AreEqual(obj.DateTimeKind, obj2.DateTimeKind);
            Assert.AreEqual(obj.DuplicateName, obj2.DuplicateName);
            Assert.AreEqual(obj.EnumMember, obj2.EnumMember);
            Assert.AreEqual(obj.Int32Flags, obj2.Int32Flags);
            Assert.AreEqual(obj.UInt64Flags, obj2.UInt64Flags);
        }

        [DataTestMethod]
        [DataRow(DateTimeKind.Local, "'Local'", true)]
        [DataRow(TestViewModelWithEnums.ByteEnum.A, "'A'", true)]
        [DataRow(TestViewModelWithEnums.ByteEnum.B, "'B'", true)]
        [DataRow(TestViewModelWithEnums.ByteEnum.C, "'C'", true)]
        [DataRow((TestViewModelWithEnums.ByteEnum)45, "45", false)]
        [DataRow(TestViewModelWithEnums.Int16Enum.A, "'A'", true)]
        [DataRow(TestViewModelWithEnums.Int16Enum.B, "'B'", true)]
        [DataRow((TestViewModelWithEnums.Int16Enum)(-6), "-6", false)]
        [DataRow(TestViewModelWithEnums.EnumMemberEnum.A, "'member-a'", true)]
        [DataRow(TestViewModelWithEnums.DuplicateNameEnum.A, "'A'", true)]
        [DataRow(TestViewModelWithEnums.DuplicateNameEnum.B, "'A'", true)]
        [DataRow(TestViewModelWithEnums.DuplicateNameEnum.C, "'C'", true)]
        [DataRow(TestViewModelWithEnums.DuplicateNameEnum.DAndAlsoLonger, "'D'", true)]
        [DataRow((TestViewModelWithEnums.DuplicateNameEnum)3, "3", false)]
        [DataRow(TestViewModelWithEnums.Int32FlagsEnum.ABC, "'a+b+c'", true)]
        [DataRow(TestViewModelWithEnums.Int32FlagsEnum.A | TestViewModelWithEnums.Int32FlagsEnum.BCD, "'b+c+d,a'", true)]
        [DataRow(TestViewModelWithEnums.Int32FlagsEnum.Everything, "'everything'", true)]
        [DataRow((TestViewModelWithEnums.Int32FlagsEnum)2356543, "2356543", false)]
        [DataRow((TestViewModelWithEnums.Int32FlagsEnum)0, "0", true)]
        [DataRow((TestViewModelWithEnums.UInt64FlagsEnum)0, "0", true)]
        [DataRow(TestViewModelWithEnums.UInt64FlagsEnum.F1 | TestViewModelWithEnums.UInt64FlagsEnum.F2 | TestViewModelWithEnums.UInt64FlagsEnum.F64, "'F64,F2,F1'", true)]
        [DataRow(TestViewModelWithEnums.UInt64FlagsEnum.F64, "'F64'", true)]
        [DataRow((TestViewModelWithEnums.UInt64FlagsEnum)12 | TestViewModelWithEnums.UInt64FlagsEnum.F64, "9223372036854775820", false)]
        [DataRow((TestViewModelWithEnums.UInt64FlagsEnum)ulong.MaxValue, "18446744073709551615", false)]
        [DataRow((TestViewModelWithEnums.UInt64FlagsEnum)0, "0", true)]
        public void TestEnumSerialization(object enumValue, string serializedValue, bool canDeserialize)
        {
            var json = JsonSerializer.Serialize(enumValue, DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
            Assert.AreEqual(serializedValue.Replace("'", "\""), json);
            if (canDeserialize)
            {
                var deserialized = JsonSerializer.Deserialize(json, enumValue.GetType(), DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
                Assert.AreEqual(enumValue, deserialized);
            }
            else
            {
                XAssert.ThrowsAny<Exception>(() => JsonSerializer.Deserialize(json, enumValue.GetType(), DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe));
            }
        }

        [TestMethod]
        public void TestEnumSerializationRoundtrip()
        {
            foreach (var type in typeof(TestViewModelWithEnums).GetNestedTypes())
            {
                foreach (var value in Enum.GetValues(type))
                {
                    var json = JsonSerializer.Serialize(value, DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
                    var deserialized = JsonSerializer.Deserialize(json, value.GetType(), DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
                    Assert.AreEqual(value, deserialized, message: $"{value} != {deserialized} for enum value {type.Name}.{value}");
                }
            }
        }

        [TestMethod]
        public void DoesNotTouchIrrelevantGetters()
        {
            var obj = new ParentClassWithBrokenGetters() {
                NestedVM = {
                    SomeNestedVM = new TestViewModelWithRecords {
                        Primitive = 100
                    }
                }
            };
            var (obj2, json) = SerializeAndDeserialize(obj);

            Assert.AreEqual(obj.NestedVM.SomeNestedVM.Primitive, obj2.NestedVM.SomeNestedVM.Primitive);
            Assert.AreEqual(obj.NestedVM.BrokenGetter, obj2.NestedVM.BrokenGetter);
        }
        public class ViewModelWithService
        {
            public string Property1 { get; }
            private DotvvmTestHelper.ITestSingletonService Service { get; }
            public DotvvmTestHelper.ITestSingletonService GetService() => Service;

            [JsonConstructor]
            public ViewModelWithService(string property1, DotvvmTestHelper.ITestSingletonService service)
            {
                Property1 = property1;
                Service = service;
            }
        }

        [TestMethod]
        public void FailsReasonablyOnUnmatchedConstructorProperty1()
        {
            var obj = new ViewModelWithUnmatchedConstuctorProperty1("test");
            var x = Assert.ThrowsException<Exception>(() => SerializeAndDeserialize(obj));
            Assert.AreEqual("Can not deserialize DotVVM.Framework.Tests.ViewModel.SerializerTests.ViewModelWithUnmatchedConstuctorProperty1, constructor parameter x is not mapped to any property.", x.Message);
        }

        public class ViewModelWithUnmatchedConstuctorProperty1
        {
            [JsonConstructor]
            public ViewModelWithUnmatchedConstuctorProperty1(string x) { }
        }

        [TestMethod]
        public void FailsReasonablyOnUnmatchedConstructorProperty2()
        {
            var obj = new ViewModelWithUnmatchedConstuctorProperty2(null!);
            var x = Assert.ThrowsException<Exception>(() => SerializeAndDeserialize(obj));
            Assert.AreEqual("Can not deserialize DotVVM.Framework.Tests.ViewModel.SerializerTests.ViewModelWithUnmatchedConstuctorProperty2, constructor parameter x is not mapped to any property and service TestViewModelWithByteArray was not found in ServiceProvider.", x.Message);
        }

        public class ViewModelWithUnmatchedConstuctorProperty2
        {
            [JsonConstructor]
            public ViewModelWithUnmatchedConstuctorProperty2(TestViewModelWithByteArray x) { }
        }


        [TestMethod]
        public void PropertyShadowing()
        {
            var obj = new TestViewModelWithPropertyShadowing.Inner {
                EnumerableToList = ["x", "y"],
                ObjectToList = ["z" ],
                InterfaceToInteger = 5,
                ObjectToInteger = 6,
                ShadowedByField = 7
            };

            var (obj2, json) = SerializeAndDeserialize(obj);
            XAssert.Equal(obj.EnumerableToList, obj2.EnumerableToList);
            XAssert.Equal(obj.ObjectToList, obj2.ObjectToList);
            XAssert.Equal(obj.InterfaceToInteger, obj2.InterfaceToInteger);
            XAssert.Equal(obj.ObjectToInteger, obj2.ObjectToInteger);
            XAssert.Equal(obj.ShadowedByField, obj2.ShadowedByField);
            XAssert.IsType<JsonArray>(json["EnumerableToList"]);
        }

        [TestMethod]
        public void PropertyShadowing_BaseTypeDeserialized()
        {
            var obj = new TestViewModelWithPropertyShadowing.Inner {
                EnumerableToList = ["x", "y"],
                ObjectToList = ["z" ],
                InterfaceToInteger = 5,
                ObjectToInteger = 6,
                ShadowedByField = 7
            };
            // Serialized Inner but deserializes the base type
            var (obj2Box, json) = SerializeAndDeserialize<DynamicDispatchVMContainer<TestViewModelWithPropertyShadowing>>(new() { Value = obj });
            var obj2 = obj2Box.Value;
            json = json["Value"].AsObject();
            XAssert.Equal(typeof(TestViewModelWithPropertyShadowing), obj2.GetType());

            XAssert.Equal(obj.EnumerableToList, obj2.EnumerableToList);
            XAssert.IsType<JsonElement>(obj2.ObjectToList);
            XAssert.Null(obj2.InterfaceToInteger);
            XAssert.Equal(6d, XAssert.IsType<double>(obj2.ObjectToInteger));
            XAssert.Equal(7d, XAssert.IsType<double>(obj2.ShadowedByField));
            XAssert.Equal(5, (int)json["InterfaceToInteger"]);
            XAssert.Equal(6, (int)json["ObjectToInteger"]);
            XAssert.Equal(7, (double)json["ShadowedByField"]);
            XAssert.IsType<JsonArray>(json["EnumerableToList"]);
        }

        [TestMethod]
        public void InterfaceDeserialization_Error()
        {
            var obj = new VMWithInterface();
            var ex = XAssert.ThrowsAny<Exception>(() => SerializeAndDeserialize(new StaticDispatchVMContainer<IVMInterface1> { Value = obj }));
            XAssert.Contains("Can not deserialize DotVVM.Framework.Tests.ViewModel.SerializerTests.IVMInterface1 because it's abstract.", ex.Message);
        }

        [TestMethod]
        public void InterfaceSerialization_Static()
        {
            var obj = new VMWithInterface() {
                Property1 = "x",
                Property2 = "y",
                Property3 = "z"
            };
            var jsonStr = Serialize(new StaticDispatchVMContainer<IVMInterface1> { Value = obj }, out var _, false);
            Console.WriteLine(jsonStr);
            var json = JsonNode.Parse(jsonStr).AsObject()["Value"].AsObject();
            XAssert.DoesNotContain("Property3", json);
            XAssert.Equal("x", (string)json["Property1"]);
            XAssert.Equal("y", (string)json["Property2"]);

            var obj2 = new StaticDispatchVMContainer<IVMInterface1> { Value = new VMWithInterface() };
            var obj2Populated = PopulateViewModel(jsonStr, obj2);
            Assert.AreSame(obj2, obj2Populated);
            XAssert.Equal(obj.Property1, obj2.Value.Property1);
            XAssert.Equal(obj.Property2, obj2.Value.Property2);
        }

        [TestMethod]
        public void InterfaceSerialization_Dynamic()
        {
            var obj = new VMWithInterface() {
                Property1 = "x",
                Property2 = "y",
                Property3 = "z"
            };
            var jsonStr = Serialize(new DefaultDispatchVMContainer<IVMInterface1> { Value = obj }, out var _, false);
            Console.WriteLine(jsonStr);
            var json = JsonNode.Parse(jsonStr).AsObject()["Value"].AsObject();
            XAssert.Equal("x", (string)json["Property1"]);
            XAssert.Equal("y", (string)json["Property2"]);
            XAssert.Equal("z", (string)json["Property3"]);

            var obj2 = new DefaultDispatchVMContainer<IVMInterface1> { Value = new VMWithInterface() };
            var obj2Populated = PopulateViewModel(jsonStr, obj2);
            Assert.AreSame(obj2, obj2Populated);
            XAssert.Equal(obj.Property1, obj2.Value.Property1);
            XAssert.Equal(obj.Property2, obj2.Value.Property2);
            XAssert.Equal(obj.Property3, ((VMWithInterface)obj2.Value).Property3);
        }

        class VMWithInterface: IVMInterface1
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
        }

        interface IVMInterface1: IVMInterface2
        {
            string Property1 { get; set; }
        }
        interface IVMInterface2
        {
            string Property2 { get; set; }
        }


        [TestMethod]
        public void SupportCustomConverters()
        {
            var obj = new TestViewModelWithCustomConverter() { Property1 = "A", Property2 = "B" };
            var (obj2, json) = SerializeAndDeserialize(new StaticDispatchVMContainer<TestViewModelWithCustomConverter> { Value = obj });
            Console.WriteLine(json);
            json = json["Value"].AsObject();
            Assert.AreEqual(obj.Property1, obj2.Value.Property1);
            Assert.AreEqual(obj.Property2, obj2.Value.Property2);
            Assert.AreEqual("A", (string)json["Property1"]);
            Assert.AreEqual(null, json["Property2"]);
            Assert.AreEqual("A,B", (string)json["Properties"]);

            var obj3 = Deserialize<TestViewModelWithCustomConverter>("""{"Properties":"C,D"}""");
            Assert.AreEqual("C", obj3.Property1);
            Assert.AreEqual("D", obj3.Property2);
        }

        [TestMethod]
        public void SupportCustomConverters_DynamicDispatch()
        {
            var obj = new TestViewModelWithCustomConverter() { Property1 = "A", Property2 = "B" };
            var jsonStr = Serialize(new DefaultDispatchVMContainer<object> { Value = obj }, out var _, false);
            Console.WriteLine(jsonStr);
            var obj2 = new DefaultDispatchVMContainer<object> { Value = new TestViewModelWithCustomConverter() };
            var obj2Populated = PopulateViewModel(jsonStr, obj2);
            Assert.AreSame(obj2, obj2Populated);
            Assert.AreEqual(obj.Property1, ((TestViewModelWithCustomConverter)obj2.Value).Property1);
            Assert.AreEqual(obj.Property2, ((TestViewModelWithCustomConverter)obj2.Value).Property2);

            var json = JsonNode.Parse(jsonStr).AsObject()["Value"].AsObject();
            Assert.AreEqual("A", (string)json["Property1"]);
            Assert.AreEqual(null, json["Property2"]);
            Assert.AreEqual("A,B", (string)json["Properties"]);
        }



        [TestMethod]
        public void Serializer_CanDisableDotvvmSerializer()
        {
            var (obj,json) = SerializeAndDeserialize(new TestDefaultSerializationViewModel { ItsSerializedAnyway = true });
            Assert.AreEqual("""{"ItsSerializedAnyway":true}""", json.ToJsonString(new JsonSerializerOptions { WriteIndented = false }));

            Assert.IsTrue(obj.ItsSerializedAnyway);
        }


        [TestMethod]
        public void Serializer_CanUseDefaultPolymorphismSupport_1()
        {
            var (obj, json) = SerializeAndDeserialize<TestDefaultSerializationViewModel>(new TestDefaultSerializationViewModel.Derived1 { ItsSerializedAnyway = true, Property1 = "abc" });
            Assert.AreEqual("""{"$t":1,"Property1":"abc","ItsSerializedAnyway":true}""", json.ToJsonString(new JsonSerializerOptions { WriteIndented = false }));

            Assert.IsTrue(obj.ItsSerializedAnyway);
            Assert.AreEqual("abc", ((TestDefaultSerializationViewModel.Derived1)obj).Property1);
        }

        [TestMethod]
        public void Serializer_CanUseDefaultPolymorphismSupport_2()
        {
            var (obj, json) = SerializeAndDeserialize<TestDefaultSerializationViewModel>(new TestDefaultSerializationViewModel.Derived2 { ItsSerializedAnyway = true, Property2 = 123 });
            Assert.AreEqual("""{"$t":2,"Property2":123,"ItsSerializedAnyway":true}""",
                json.ToJsonString(new JsonSerializerOptions { WriteIndented = false }));

            Assert.IsTrue(obj.ItsSerializedAnyway);
            Assert.AreEqual(123, ((TestDefaultSerializationViewModel.Derived2)obj).Property2);
        }

        [TestMethod]
        public void Serializer_CanUseDefaultPolymorphismSupport_2B()
        {
            var (obj, json) = SerializeAndDeserialize<TestDefaultSerializationViewModel>(new TestDefaultSerializationViewModel.Derived2B { ItsSerializedAnyway = true, Property2 = 123, Property2B = -5 });

            Assert.AreEqual("""{"$t":2,"Property2":123,"ItsSerializedAnyway":true}""", json.ToJsonString(new JsonSerializerOptions { WriteIndented = false }));

            Assert.IsTrue(obj.ItsSerializedAnyway);
            Assert.AreEqual(123, ((TestDefaultSerializationViewModel.Derived2)obj).Property2);
            Assert.AreEqual(typeof(TestDefaultSerializationViewModel.Derived2), obj.GetType());
        }


        [DotvvmSerialization(DisableDotvvmConverter = true)]
        [JsonPolymorphic(TypeDiscriminatorPropertyName = "$t", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
        [JsonDerivedType(typeof(Derived1), typeDiscriminator: 1)]
        [JsonDerivedType(typeof(Derived2), typeDiscriminator: 2)]
        public class TestDefaultSerializationViewModel
        {
            [Bind(Direction.None)]
            public bool ItsSerializedAnyway { get; set; }

            public class Derived1: TestDefaultSerializationViewModel
            {
                public string Property1 { get; set; }
            }

            public class Derived2: TestDefaultSerializationViewModel
            {
                public int Property2 { get; set; }
            }

            public class Derived2B: Derived2
            {
                public int Property2B { get; set; }
            }
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
        public KeyValuePair<TestViewModelWithBind, TestViewModelWithBind> P5 { get; set; }
        public Tuple<TestViewModelWithBind, int> P6 { get; set; }
    }

    public class TestViewModelWithBind
    {
        [Bind(Name = "property ONE")]
        public string P1 { get; set; } = "value 1";
        [JsonPropertyName("property TWO")]
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

    public class TestInitOnlyClass
    {
        public int X { get; init; }
        public string Y { get; set; }
    }
    public class TestInitOnlyRecord
    {
        public int X { get; init; }
        public string Y { get; set; }
    }

    public class TestViewModelWithSignedDictionary
    {
        [Protect(ProtectMode.SignData)]
        public Dictionary<string, string> SignedDictionary { get; set; } = new();
    }

    // we had a bug that the deserializer touched all properties before deserializing, some of these could crash on NRE because they were computing something
    public class ClassWithBrokenGetters
    {
        public TestViewModelWithRecords SomeNestedVM { get; set; } = null;

        public bool BrokenGetter => SomeNestedVM.Primitive > 10;
    }

    public class ParentClassWithBrokenGetters
    {
        public ClassWithBrokenGetters NestedVM { get; set; } = new ClassWithBrokenGetters();
    }

    public class TestViewModelWithEnums
    {
        public DateTimeKind DateTimeKind { get; set; }
        public DuplicateNameEnum DuplicateName { get; set; }
        public Int32Enum Int32 { get; set; }
        public SByteEnum SByte { get; set; }
        public Int16Enum Int16 { get; set; }
        public Int64Enum Int64 { get; set; }
        public ByteEnum Byte { get; set; }
        public UInt16Enum UInt16 { get; set; }
        public UInt32Enum UInt32 { get; set; }
        public UInt64Enum UInt64 { get; set; }
        public EnumMemberEnum EnumMember { get; set; }
        public Int32FlagsEnum Int32Flags { get; set; }
        public UInt64FlagsEnum UInt64Flags { get; set; }

        public enum DuplicateNameEnum { A = 0, B = 0, C = 1, DButLonger = 2, D = 2, DAndAlsoLonger = 2 }

        public enum Int32Enum : int { A, B = int.MinValue, C = -1, D = int.MaxValue }
        public enum SByteEnum : sbyte { A, B }
        public enum Int16Enum : short { A, B = short.MinValue, C = -1, D = short.MaxValue }
        public enum Int64Enum : long { A, B = long.MinValue, C = -1, D = long.MaxValue }
        public enum ByteEnum : byte { A, B, C = 255 }
        public enum UInt16Enum : ushort { A, B = ushort.MaxValue }
        public enum UInt32Enum : uint { A, B = uint.MaxValue }
        public enum UInt64Enum : ulong { A, B = ulong.MaxValue }
        public enum EnumMemberEnum
        {
            [EnumMember(Value = "member-a")]
            A,
            [EnumMember(Value = "member-b")]
            B
        }

        [Flags]
        public enum Int32FlagsEnum : int
        {
            [EnumMember(Value = "a")]
            A = 1,
            [EnumMember(Value = "b")]
            B = 2,
            [EnumMember(Value = "c")]
            C = 4,
            [EnumMember(Value = "a+b+c")]
            ABC = 7,
            [EnumMember(Value = "a+b+c+d+e")]
            ABCDE = 31,
            [EnumMember(Value = "b+c+d")]
            BCD = 14,
            [EnumMember(Value = "everything")]
            Everything = -1
        }

        [Flags]
        public enum UInt64FlagsEnum: ulong
        {
            F1 = 1,
            F2 = 2,
            F64 = 1UL << 63,
        }
    }

    public class TestViewModelWithDateTimes
    {
        public DateTime DateTime1 { get; set; }
        public DateTime DateTime2 { get; set; }
        public DateTime DateTime3 { get; set; }

        public DateOnly DateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }
    }

    public class TestViewModelWithPropertyShadowing
    {
        public object ObjectToInteger { get; set; }
        [JsonIgnore] // does not "inherit" to shadowed property
        public IComparable InterfaceToInteger { get; set; }

        public IEnumerable<string> EnumerableToList { get; set; }
        public object ObjectToList { get; set; }

        public object ShadowedByField { get; set; }

        public class Inner: TestViewModelWithPropertyShadowing
        {
            public new int ObjectToInteger { get; set; } = 123;
            public new int InterfaceToInteger { get; set; } = 1234;

            public new List<string> EnumerableToList { get; set; } = [ "A", "B" ];
            public new List<string> ObjectToList { get; set; } = [ "C", "D" ];

            public new double ShadowedByField { get; set; } = 12345;
        }
    }

    [JsonConverter(typeof(TestViewModelWithCustomConverter.Converter))]
    class TestViewModelWithCustomConverter
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }

        public class Converter : JsonConverter<TestViewModelWithCustomConverter>
        {
            public override TestViewModelWithCustomConverter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var result = new TestViewModelWithCustomConverter();
                while (reader.TokenType != JsonTokenType.EndObject && reader.Read())
                {
                    if (reader.ValueTextEquals("Properties"))
                    {
                        reader.Read();
                        var val = reader.GetString().Split(',');
                        result.Property1 = val[0];
                        result.Property2 = val[1];
                        reader.Read();
                    }
                    else
                    {
                        reader.Read();
                        reader.Skip();
                        reader.Read();
                    }
                }
                return result;
            }

            public override void Write(Utf8JsonWriter writer, TestViewModelWithCustomConverter value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString("Properties"u8, $"{value.Property1},{value.Property2}");
                writer.WriteString("Property1"u8, value.Property1);
                writer.WriteEndObject();
            }
        }
    }

    class DynamicDispatchVMContainer<TStatic>
    {
        [Bind(AllowDynamicDispatch = true)]
        public TStatic Value { get; set; }
    }

    class StaticDispatchVMContainer<TStatic>
    {
        [Bind(AllowDynamicDispatch = false)]
        public TStatic Value { get; set; }
    }

    class DefaultDispatchVMContainer<TStatic>
    {
        [Bind(Name = "Value")] // make sure that the attribute presence does not affect the default behavior
        public TStatic Value { get; set; }
    }

    public class TestViewModelWithCollections
    {
        public List<TestViewModelWithBind> ViewModels { get; set; } = new List<TestViewModelWithBind>();
        public List<string> Strings { get; set; } = new List<string>();
    }

    public class TestViewModelWithNestedCollections
    {
        public List<List<TestViewModelWithBind>> Matrix { get; set; } = new List<List<TestViewModelWithBind>>();
        public List<List<TestViewModelWithBind[]>> NestedArrays { get; set; } = new List<List<TestViewModelWithBind[]>>();
    }

    public class TestViewModelWithPolymorphicCollection
    {
        public List<BaseItem> Items { get; set; } = new List<BaseItem>();
    }

    public class BaseItem
    {
        public string BaseProperty { get; set; }
    }

    public class DerivedItemA : BaseItem
    {
        public string DerivedAProperty { get; set; }
    }

    public class DerivedItemB : BaseItem
    {
        public string DerivedBProperty { get; set; }
    }

    public class TestViewModelWithAdvancedCollections
    {
        public List<KeyValuePair<string, TestViewModelWithBind>> KeyValuePairs { get; set; } = new List<KeyValuePair<string, TestViewModelWithBind>>();
        public Dictionary<string, TestViewModelWithBind> Dictionary { get; set; } = new Dictionary<string, TestViewModelWithBind>();
        public ImmutableArray<TestViewModelWithBind> ImmutableArray { get; set; } = ImmutableArray<TestViewModelWithBind>.Empty;
        public ImmutableList<TestViewModelWithBind> ImmutableList { get; set; } = ImmutableList<TestViewModelWithBind>.Empty;
        public ImmutableDictionary<string, TestViewModelWithBind> ImmutableDictionary { get; set; } = ImmutableDictionary<string, TestViewModelWithBind>.Empty;
    }

    public class DerivedSubclassA : DerivedItemA
    {
        public string SubProperty { get; set; }
    }

    public class TestViewModelWithConcreteCollection
    {
        public List<DerivedItemA> Items { get; set; } = new List<DerivedItemA>();
    }

    // Test classes for abstract polymorphic collections
    public abstract class AbstractBaseItem
    {
        public string BaseProperty { get; set; }
    }

    public class AbstractDerivedItemA : AbstractBaseItem
    {
        public string DerivedAProperty { get; set; }
    }

    public class AbstractDerivedItemB : AbstractBaseItem
    {
        public string DerivedBProperty { get; set; }
    }

    public class TestViewModelWithAbstractPolymorphicCollection
    {
        public List<AbstractBaseItem> Items { get; set; } = new List<AbstractBaseItem>();
    }
}
