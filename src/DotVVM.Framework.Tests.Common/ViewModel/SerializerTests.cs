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

        static string Serialize<T>(T viewModel, bool isPostback = false)
        {
            var serializer = JsonSerializer.Create(DefaultViewModelSerializer.CreateDefaultSettings());
            serializer.Converters.Add(CreateConverter(isPostback));

            var output = new StringWriter();
            serializer.Serialize(output, viewModel);
            return output.ToString();
        }

        static void Populate<T>(T viewModel, string json)
        {
            var serializer = JsonSerializer.Create(DefaultViewModelSerializer.CreateDefaultSettings());
            serializer.Converters.Add(CreateConverter(true));

            serializer.Populate(new StringReader(json), viewModel);
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
            var obj2 = new TestViewModelWithTuples();
            Populate(obj2, Serialize(obj));

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
