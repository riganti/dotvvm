using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Security;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DefaultViewModelSerializerTests
    {
        private DotvvmConfiguration configuration;
        private DefaultViewModelSerializer serializer;
        private DotvvmRequestContext context;

        [TestInitialize]
        public void TestInit()
        {
            configuration = DotvvmConfiguration.CreateDefault(services => {
                services.AddSingleton<IDataProtectionProvider>(new DpapiDataProtectionProvider("DotVVM Tests"));
                services.AddTransient<IViewModelProtector, DefaultViewModelProtector>();
                services.AddTransient<ICsrfProtector, DefaultCsrfProtector>();
                services.AddSingleton<ICookieManager, ChunkingCookieManager>();
                services.AddSingleton<IDotvvmCacheAdapter, DefaultDotvvmCacheAdapter>();
            });
            configuration.Security.SigningKey = Convert.FromBase64String("Uiq1FXs016lC6QaWIREB7H2P/sn4WrxkvFkqaIKpB27E7RPuMipsORgSgnT+zJmUu8zXNSJ4BdL73JEMRDiF6A1ScRNwGyDxDAVL3nkpNlGrSoLNM1xHnVzSbocLFDrdEiZD2e3uKujguycvWSNxYzjgMjXNsaqvCtMu/qRaEGc=");
            configuration.Security.EncryptionKey = Convert.FromBase64String("jNS9I3ZcxzsUSPYJSwzCOm/DEyKFNlBmDGo9wQ6nxKg=");

            var requestMock = new Mock<IHttpRequest>();
            requestMock.SetupGet(m => m.Url).Returns(new Uri("http://localhost:8628/Sample1"));
            requestMock.SetupGet(m => m.Path).Returns(new DotvvmHttpPathString(new PathString("/Sample1")));
            requestMock.SetupGet(m => m.PathBase).Returns(new DotvvmHttpPathString(new PathString("")));
            requestMock.SetupGet(m => m.Method).Returns("GET");
            requestMock.SetupGet(m => m.Headers).Returns(new DotvvmHeaderCollection(new HeaderDictionary(new Dictionary<string, string[]>())));

            var contextMock = new Mock<IHttpContext>();
            contextMock.SetupGet(m => m.Request).Returns(requestMock.Object);
            contextMock.SetupGet(m => m.User).Returns(new WindowsPrincipal(WindowsIdentity.GetAnonymous()));


            serializer = configuration.ServiceProvider.GetService<IViewModelSerializer>() as DefaultViewModelSerializer;
            context = new DotvvmRequestContext(contextMock.Object, configuration, configuration.ServiceProvider) {
                Presenter = configuration.RouteTable.GetDefaultPresenter(configuration.ServiceProvider),
                Route = new DotvvmRoute("TestRoute", "test.dothtml", new { }, p => p.GetService<DotvvmPresenter>(), configuration)
            };
        }



        [TestMethod]
        public void DefaultViewModelSerializer_NoEncryptedValues()
        {
            var oldViewModel = new TestViewModel() {
                Property1 = "a",
                Property2 = 4,
                Property3 = new DateTime(2000, 1, 1),
                Property4 = new List<TestViewModel2>()
                {
                    new TestViewModel2() { PropertyA = "t", PropertyB = 15 },
                    new TestViewModel2() { PropertyA = "xxx", PropertyB = 16 }
                },
                Property5 = null
            };
            context.ViewModel = oldViewModel;
            serializer.BuildViewModel(context, null);
            var result = context.GetSerializedViewModel();
            result = UnwrapSerializedViewModel(result);
            result = WrapSerializedViewModel(result);

            var newViewModel = new TestViewModel();
            context.ViewModel = newViewModel;
            serializer.PopulateViewModel(context, result);

            Assert.AreEqual(oldViewModel.Property1, newViewModel.Property1);
            Assert.AreEqual(oldViewModel.Property2, newViewModel.Property2);
            Assert.AreEqual(oldViewModel.Property3, newViewModel.Property3);
            Assert.AreEqual(oldViewModel.Property4[0].PropertyA, newViewModel.Property4[0].PropertyA);
            Assert.AreEqual(oldViewModel.Property4[0].PropertyB, newViewModel.Property4[0].PropertyB);
            Assert.AreEqual(oldViewModel.Property4[1].PropertyA, newViewModel.Property4[1].PropertyA);
            Assert.AreEqual(oldViewModel.Property4[1].PropertyB, newViewModel.Property4[1].PropertyB);
            Assert.AreEqual(oldViewModel.Property5, newViewModel.Property5);
        }

        [TestMethod]
        public void Serializer_Valid_ExistingValueNotReplaced()
        {
            var json = SerializeViewModel(new TestViewModel12 { Property = new TestViewModel13 { MyProperty = 56 } });

            var viewModel = new TestViewModel12 { Property = new TestViewModel13 { MyProperty = 55 } };
            viewModel.Property.SetPrivateField(123);
            PopulateViewModel(viewModel, json);
            Assert.AreEqual(56, viewModel.Property.MyProperty);
            Assert.AreEqual(123, viewModel.Property.GetPrivateField());
        }

        [TestMethod]
        public void Serializer_Tuples()
        {
            var json = SerializeViewModel(new TestViewModelWithTuple {
                TupleProperty = Tuple.Create(
                    new TestViewModel2 {
                        PropertyA = "abc"
                    },
                    new TestViewModel3 {
                        Property1 = "def"
                    }
                )
            });

            var result = new TestViewModelWithTuple();
            PopulateViewModel(result, json);
            Assert.AreEqual("abc", result.TupleProperty.Item1.PropertyA);
            Assert.AreEqual("def", result.TupleProperty.Item2.Property1);
        }

        [TestMethod]
        public void DeserializeDictionaryTest()
        {
            var json = SerializeViewModel(new TestViewModel6 {
                ClassClass = new Dictionary<object, object> { { "obj1", "obj2" } },
                StructClass = new Dictionary<char, object> { { 'c', "obj" } }
            });
            var result = new TestViewModel6();
            PopulateViewModel(result, json);
            Assert.AreEqual("obj2", result.ClassClass["obj1"]);
            Assert.AreEqual("obj", result.StructClass['c']);
        }

        [TestMethod]
        public void ViewModelResponse_CommandResult_Serialized()
        {
            var viewModel = new TestViewModel2 {
                PropertyA = "a",
                PropertyB = 1
            };

            var commandResult = new TestViewModel12 {
                Property = new TestViewModel13 { MyProperty = 42 }
            };

            var response = PrepareResponse(viewModel, commandResult);

            var responseModel = response["viewModel"].ToObject<TestViewModel2>();
            var responseResult = response["commandResult"].ToObject<TestViewModel12>();

            Assert.AreEqual("a", responseModel.PropertyA);
            Assert.AreEqual(1, responseModel.PropertyB);

            Assert.AreEqual(42, responseResult.Property.MyProperty);
        }

        [TestMethod]
        public void ViewModelResponse_NoCommandResult_NotPresent()
        {
            var viewModel = new TestViewModel2 {
                PropertyA = "a",
                PropertyB = 1
            };
            var response = PrepareResponse(viewModel, null);

            var responseModel = response["viewModel"].ToObject<TestViewModel2>();

            Assert.IsFalse(response.TryGetValue("commandResult", out var _));
            Assert.AreEqual("a", responseModel.PropertyA);
            Assert.AreEqual(1, responseModel.PropertyB);
        }

        [TestMethod]
        public void ViewModelResponse_NoCustomProperties_NotPresent()
        {
            var viewModel = new TestViewModel2 {
                PropertyA = "a",
                PropertyB = 1
            };
            var response = PrepareResponse(viewModel, null);

            var responseModel = response["viewModel"].ToObject<TestViewModel2>();

            Assert.IsFalse(response.TryGetValue("customProperties", out var _));
            Assert.AreEqual("a", responseModel.PropertyA);
            Assert.AreEqual(1, responseModel.PropertyB);
        }

        [TestMethod]
        public void ViewModelResponse_CustomResponseProperties_Serialized()
        {
            var viewModel = new TestViewModel2 {
                PropertyA = "a",
                PropertyB = 1
            };

            var response = PrepareResponse(viewModel, null, new Dictionary<string, object> {
                {"prop1", new TestViewModel13 { MyProperty = 42 }},
                {"prop2", "Hello"}
            });

            var responseModel = response["viewModel"].ToObject<TestViewModel2>();
            var prop1 = response["customProperties"]["prop1"].ToObject<TestViewModel13>();
            var prop2 = response["customProperties"]["prop2"].ToObject<string>();

            Assert.AreEqual("a", responseModel.PropertyA);
            Assert.AreEqual(1, responseModel.PropertyB);

            Assert.AreEqual(42, prop1.MyProperty);
            Assert.AreEqual("Hello", prop2);
        }

        [TestMethod]
        public void StaticCommandResponse_CustomResponseProperties_Serialized()
        {
            var response = PrepareStaticCommandResponse("Test", new Dictionary<string, object> {
                {"prop1", new TestViewModel13 { MyProperty = 42 }},
                {"prop2", "Hello"}
            });

            var commandResult = response["result"].ToObject<string>();
            var prop1 = response["customProperties"]["prop1"].ToObject<TestViewModel13>();
            var prop2 = response["customProperties"]["prop2"].ToObject<string>();

            Assert.AreEqual("Test", commandResult);

            Assert.AreEqual(42, prop1.MyProperty);
            Assert.AreEqual("Hello", prop2);
        }

        [TestMethod]
        public void StaticCommandResponse_NoCustomResponseProperties_NotPresent()
        {
            var response = PrepareStaticCommandResponse("Test");

            var commandResult = response["result"].ToObject<string>();

            Assert.AreEqual("Test", commandResult);

            Assert.IsFalse(response.TryGetValue("customProperties", out var _));
        }

        [TestMethod]
        public void StaticCommandResponse_AlreadySerializedProperties_Throws()
        {
            var result = serializer.BuildStaticCommandResponse(context, "Test");

            Assert.ThrowsException<InvalidOperationException>(() => {

                context.CustomResponseProperties.Add("nope", 12);
            });
        }

        [TestMethod]
        public void ViewModelResponse_AlreadySerializedProperties_Throws()
        {
            serializer.BuildViewModel(context, null);

            Assert.ThrowsException<InvalidOperationException>(() => {

                context.CustomResponseProperties.Add("nope", 12);
            });
        }

        private JObject PrepareResponse(object viewModel, object commandResult, Dictionary<string, object> customProperties = null)
        {
            context.ViewModel = viewModel;

            foreach (var prop in customProperties ?? new Dictionary<string, object>())
            {
                context.CustomResponseProperties.Add(prop.Key, prop.Value);
            }

            serializer.BuildViewModel(context, commandResult);
            var result = context.GetSerializedViewModel();

            return JObject.Parse(result);
        }

        private JObject PrepareStaticCommandResponse(object commandResult, Dictionary<string, object> customProperties = null)
        {
            foreach (var prop in customProperties ?? new Dictionary<string, object>())
            {
                context.CustomResponseProperties.Add(prop.Key, prop.Value);
            }

            var result = serializer.BuildStaticCommandResponse(context, commandResult);

            return JObject.Parse(result);
        }

        private string SerializeViewModel(object viewModel)
        {
            context.ViewModel = viewModel;

            serializer.SendDiff = false;
            serializer.BuildViewModel(context, null);
            return UnwrapSerializedViewModel(serializer.SerializeViewModel(context));
        }

        private void PopulateViewModel(object viewModel, string json)
        {
            context.ViewModel = viewModel;
            serializer.PopulateViewModel(context,
                "{'validationTargetPath': null,'viewModel':" + json + "}");
        }

        public class TestViewModelWithTuple
        {
            public Tuple<TestViewModel2, TestViewModel3> TupleProperty { get; set; }
        }

        class TestViewModel12
        {
            public TestViewModel13 Property { get; set; }
        }
        class TestViewModel13
        {
            public int MyProperty { get; set; }
            private int privateField = 33;
            public void SetPrivateField(int value)
            {
                privateField = value;
            }
            public int GetPrivateField() => privateField;
        }


        public class TestViewModel
        {
            public string Property1 { get; set; }
            public int Property2 { get; set; }
            public DateTime Property3 { get; set; }
            public List<TestViewModel2> Property4 { get; set; }
            public string Property5 { get; set; }
        }
        public class TestViewModel2
        {
            public string PropertyA { get; set; }
            public int PropertyB { get; set; }
        }



        [TestMethod]
        public void DefaultViewModelSerializer_SignedAndEncryptedValue()
        {
            var oldViewModel = new TestViewModel3() {
                Property1 = "a",
                Property2 = 4,
                Property3 = new DateTime(2000, 1, 1),
                Property4 = new List<TestViewModel4>()
                {
                    new TestViewModel4() { PropertyA = "t", PropertyB = 15 },
                    new TestViewModel4() { PropertyA = "xxx", PropertyB = 16 }
                }
            };
            context.ViewModel = oldViewModel;

            serializer.BuildViewModel(context, null);
            var result = context.GetSerializedViewModel();
            result = UnwrapSerializedViewModel(result);
            result = WrapSerializedViewModel(result);

            var newViewModel = new TestViewModel3();
            context.ViewModel = newViewModel;
            serializer.PopulateViewModel(context, result);

            Assert.AreEqual(oldViewModel.Property1, newViewModel.Property1);
            Assert.AreEqual(oldViewModel.Property2, newViewModel.Property2);
            Assert.AreEqual(oldViewModel.Property3, newViewModel.Property3);
            Assert.AreEqual(oldViewModel.Property4[0].PropertyA, newViewModel.Property4[0].PropertyA);
            Assert.AreEqual(oldViewModel.Property4[0].PropertyB, newViewModel.Property4[0].PropertyB);
            Assert.AreEqual(oldViewModel.Property4[1].PropertyA, newViewModel.Property4[1].PropertyA);
            Assert.AreEqual(oldViewModel.Property4[1].PropertyB, newViewModel.Property4[1].PropertyB);
        }

        [TestMethod]
        public void DefaultViewModelSerializer_SignReadonlyProperty()
        {
            var oldViewModel = new ViewModelWithInvalidProtectionSettings();
            context.ViewModel = oldViewModel;

            Assert.ThrowsException<DotvvmCompilationException>(() => {
                try
                {
                    serializer.BuildViewModel(context, null);
                    context.GetSerializedViewModel();
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }
            });
        }

        public class ViewModelWithInvalidProtectionSettings
        {
            [Protect(ProtectMode.SignData)]
            public int ReadOnlyProperty => 1;
        }

        public class TestViewModel3
        {
            public string Property1 { get; set; }

            [Protect(ProtectMode.SignData)]
            public int Property2 { get; set; }

            [Protect(ProtectMode.EncryptData)]
            public DateTime Property3 { get; set; }
            public List<TestViewModel4> Property4 { get; set; }
        }
        public class TestViewModel4
        {
            [Protect(ProtectMode.SignData)]
            public string PropertyA { get; set; }

            [Protect(ProtectMode.EncryptData)]
            public int PropertyB { get; set; }
        }




        public class TestViewModel6
        {
            public Dictionary<char, object> StructClass { get; set; }
            public Dictionary<object, object> ClassClass { get; set; }
        }



        [TestMethod]
        public void DefaultViewModelSerializer_SignedAndEncryptedValue_NullableInt_NullValue()
        {
            var oldViewModel = new TestViewModel5() {
                ProtectedNullable = null
            };
            context.ViewModel = oldViewModel;

            serializer.BuildViewModel(context, null);
            var result = context.GetSerializedViewModel();
            result = UnwrapSerializedViewModel(result);
            result = WrapSerializedViewModel(result);

            var newViewModel = new TestViewModel5();
            context.ViewModel = newViewModel;
            serializer.PopulateViewModel(context, result);

            Assert.AreEqual(oldViewModel.ProtectedNullable, newViewModel.ProtectedNullable);
        }


        class TestViewModel5
        {
            [Protect(ProtectMode.SignData)]
            public int? ProtectedNullable { get; set; }
        }


        [TestMethod]
        public void DefaultViewModelSerializer_Enum()
        {
            var oldViewModel = new EnumTestViewModel() {
                Property1 = TestEnum.Second
            };
            context.ViewModel = oldViewModel;
            serializer.BuildViewModel(context, null);
            var result = context.GetSerializedViewModel();
            result = UnwrapSerializedViewModel(result);
            result = WrapSerializedViewModel(result);

            var newViewModel = new EnumTestViewModel();
            context.ViewModel = newViewModel;
            serializer.PopulateViewModel(context, result);

            Assert.IsFalse(result.Contains(typeof(TestEnum).FullName));
            Assert.AreEqual(oldViewModel.Property1, newViewModel.Property1);
        }


        public class EnumCollectionTestViewModel
        {
            public TestEnum Property1 { get; set; }

            public List<EnumTestViewModel> Children { get; set; }
        }

        public class EnumTestViewModel
        {
            public TestEnum Property1 { get; set; }
        }

        public enum TestEnum
        {
            First,
            Second,
            Third
        }




        [TestMethod]
        public void DefaultViewModelSerializer_EnumInCollection()
        {
            var oldViewModel = new EnumCollectionTestViewModel() {
                Property1 = TestEnum.Third,
                Children = new List<EnumTestViewModel>()
                {
                    new EnumTestViewModel() { Property1 = TestEnum.First },
                    new EnumTestViewModel() { Property1 = TestEnum.Second },
                    new EnumTestViewModel() { Property1 = TestEnum.Third }
                }
            };

            context.ViewModel = oldViewModel;
            serializer.BuildViewModel(context, null);
            var result = context.GetSerializedViewModel();
            result = UnwrapSerializedViewModel(result);
            result = WrapSerializedViewModel(result);

            var newViewModel = new EnumCollectionTestViewModel() { Children = new List<EnumTestViewModel>() };
            context.ViewModel = newViewModel;
            serializer.PopulateViewModel(context, result);

            Assert.IsFalse(result.Contains(typeof(TestEnum).FullName));
            Assert.AreEqual(oldViewModel.Property1, newViewModel.Property1);
            Assert.AreEqual(oldViewModel.Children[0].Property1, newViewModel.Children[0].Property1);
            Assert.AreEqual(oldViewModel.Children[1].Property1, newViewModel.Children[1].Property1);
            Assert.AreEqual(oldViewModel.Children[2].Property1, newViewModel.Children[2].Property1);
        }

        [TestMethod]
        public void Serializer_Valid_BindBothOnGetOnlyProperty()
        {
            var json = SerializeViewModel(new GetOnlyPropertyViewModel { Property = 42 });

            var viewModel = new GetOnlyPropertyViewModel();
            PopulateViewModel(viewModel, json);
            Assert.AreEqual(43, viewModel.PropertyPlusOne);
        }

        public class GetOnlyPropertyViewModel
        {
            public int Property { get; set; }
            [Bind(Direction.Both)]
            public int PropertyPlusOne => Property + 1;
        }


        /// <summary>
        /// Wraps the serialized view model to an object that comes from the client.
        /// </summary>
        private static string WrapSerializedViewModel(string result)
        {
            return string.Format("{{'currentPath':[],'command':'','controlUniqueId':'','viewModel':{0},'validationTargetPath':'','updatedControls':{{}}}}".Replace("'", "\""), result);
        }

        /// <summary>
        /// Unwraps the object that goes to the client to the serialized view model.
        /// </summary>
        private static string UnwrapSerializedViewModel(string result)
        {
            return JObject.Parse(result)["viewModel"].ToString();
        }

    }
}
