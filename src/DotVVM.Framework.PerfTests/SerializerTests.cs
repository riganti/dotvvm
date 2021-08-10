using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using DotVVM.Framework.Testing;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.PerfTests
{
    public class SerializerTests
    {
        public class FakeProtector : IViewModelProtector
        {
            public string Protect(string serializedData, IDotvvmRequestContext context) => serializedData;

            public byte[] Protect(byte[] plaintextData, params string[] purposes) => plaintextData;

            public string Unprotect(string protectedData, IDotvvmRequestContext context) => protectedData;

            public byte[] Unprotect(byte[] protectedData, params string[] purposes) => protectedData;
        }

        private readonly DotvvmConfiguration configuration;
        private readonly DefaultViewModelSerializer serializer;
        private readonly ViewModel viewModel;
        public readonly IDotvvmRequestContext request;
        private readonly bool allowDiffs;

        public string LastViewModel { get; private set; }

        public SerializerTests(bool allowDiffs)
        {
            this.allowDiffs = allowDiffs;
            this.configuration = DotvvmConfiguration.CreateDefault(s => {
                s.AddSingleton<IViewModelProtector, FakeProtector>();
            });
            this.serializer = configuration.ServiceProvider.GetRequiredService<IViewModelSerializer>() as DefaultViewModelSerializer;
            this.viewModel = CreateViewModel();
            this.request = new TestDotvvmRequestContext()
            {
                ViewModel = viewModel,
                CsrfToken = "",
                ResourceManager = new ResourceManagement.ResourceManager(configuration.Resources),
                Configuration = configuration,
                ModelState = new ModelState()
            };
        }

        public void Serialize()
        {
            viewModel.Nested.Array[0].MyProperty = Guid.NewGuid().ToString(); // modify a bit

            serializer.
                BuildViewModel(request, null);
            LastViewModel = serializer.SerializeViewModel(request);
            if (allowDiffs) request.ReceivedViewModelJson = (JObject)request.ViewModelJson["viewModel"];
        }

        public void Deserialize()
        {
            request.ViewModel = new ViewModel();
            serializer.PopulateViewModel(request, LastViewModel);
        }


        public static ViewModel CreateViewModel(int level = 5)
        {
            var random = new Random(42);
            return new ViewModel
            {
                Nested = level > 0 ? CreateViewModel(level - 1) : null,
                Nested2 = level > 0 ? CreateViewModel(level - 1) : null,
                Array = Enumerable.Range(0, 10).Select(i =>
                    new ViewModel2
                    {
                        MyProperty = Guid.NewGuid().ToString(),
                        MyProperty2 = Guid.NewGuid().ToString(),
                        MyProperty3 = Guid.NewGuid().ToString(),
                        MyProperty4 = Guid.NewGuid().ToString(),
                        MyProperty5 = Guid.NewGuid().ToString(),
                        MyProperty6 = Guid.NewGuid().ToString(),
                        MyProperty7 = Guid.NewGuid().ToString(),
                        MyProperty8 = Guid.NewGuid().ToString(),
                        MyProperty9 = Guid.NewGuid().ToString(),
                        MyProperty10 = Guid.NewGuid().ToString(),
                        MyProperty11 = random.Next(),
                        MyProperty12 = random.Next(),
                        MyProperty13 = random.Next(),
                        MyProperty14 = random.Next(),
                        MyProperty15 = random.NextDouble(),
                        MyProperty16 = random.Next(),
                        MyProperty17 = random.Next(),
                        MyProperty18 = DateTime.Today.AddSeconds(random.Next()),
                        MyProperty19 = DateTime.Today.AddMinutes(random.Next()),
                    }).ToArray()
            };
        }

        public class ViewModel
        {
            public ViewModel Nested { get; set; }
            public ViewModel Nested2 { get; set; }
            public ViewModel2[] Array { set; get; }
        }

        public class ViewModel2
        {
            public string MyProperty { get; set; }
            public string MyProperty2 { get; set; }
            public string MyProperty3 { get; set; }
            public string MyProperty4 { get; set; }
            public string MyProperty5 { get; set; }
            public string MyProperty6 { get; set; }
            public string MyProperty7 { get; set; }
            public string MyProperty8 { get; set; }
            public string MyProperty9 { get; set; }
            public string MyProperty10 { get; set; }
            public int MyProperty11 { get; set; }
            public int MyProperty12 { get; set; }
            public int MyProperty13 { get; set; }
            public double MyProperty14 { get; set; }
            public double MyProperty15 { get; set; }
            public int MyProperty16 { get; set; }
            public int MyProperty17 { get; set; }
            public DateTime MyProperty18 { get; set; }
            public DateTime MyProperty19 { get; set; }
        }
    }
}
