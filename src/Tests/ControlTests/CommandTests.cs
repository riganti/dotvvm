using CheckTestOutput;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class CommandTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {

        }, services: services => {
            services.Services.AddTransient<OmgViewModelWithIsAlsoAService>();
            services.Services.AddTransient<OmgViewModelWithIsAlsoAService2>();
        });
        OutputChecker check = new OutputChecker("testoutputs");


        [TestMethod]
        public async Task RootViewModelIsRecord()
        {
            var r = await cth.RunPage(typeof(ViewModel1), " <dot:Button Text=Click Click={command: Text = NestedVM.TestProp + NestedVM2.TestProp + NestedVM3.A} /> ");

            Assert.AreEqual("Text1", (string)r.ViewModel.Text);
            Assert.AreEqual("Text2", (string)r.ViewModel.NestedVM.TestProp);
            Assert.AreEqual("Text3", (string)r.ViewModel.NestedVM2.TestProp);
            Assert.AreEqual(0, (int)r.ViewModel.NestedVM3.A);

            await r.RunCommand("Text = NestedVM.TestProp + NestedVM2.TestProp + NestedVM3.A");

            Assert.AreEqual("Text2Text30", (string)r.ViewModel.Text);
        }

        
        public class ViewModel1: DotvvmViewModelBase
        {
            // don't ask me why people do this...
            // on one project, 4.1 upgrade did not work, because they try to inject
            // a service into viewmodel which is also a nested viewmodel property 🤦
            // DotVVM obviously thinks it's a viewmodel, not a service, so it
            // re-creates the object after deserialization, which broke other things if it's a root viewmodel
            public ViewModel1(OmgViewModelWithIsAlsoAService nestedVM, OmgViewModelWithIsAlsoAService2 nestedVM2)
            {
                NestedVM = nestedVM;
                NestedVM2 = nestedVM2;
                NestedVM3 = new OmgViewModelWhichCannotBeCreatedByConstructorButIsInstantiatedManually(1);
            }

            private bool calledInit, calledLoad;

            public override Task Init()
            {
                if (Context is null)
                    throw new System.Exception("Context is null in Init");
                calledInit = true;
                return base.Init();
            }
            public override Task Load()
            {
                if (Context is null)
                    throw new System.Exception("Context is null in Load");
                if (!calledInit)
                    throw new System.Exception("Init was not called");
                calledLoad = true;
                return base.Load();
            }
            public override Task PreRender()
            {
                if (Context is null)
                    throw new System.Exception("Context is null in PreRender");
                if (!calledLoad)
                    throw new System.Exception("Load was not called");
                return base.PreRender();
            }


            public OmgViewModelWithIsAlsoAService NestedVM { get; }

            public OmgViewModelWithIsAlsoAService2 NestedVM2 { get; set; }

            public OmgViewModelWhichCannotBeCreatedByConstructorButIsInstantiatedManually NestedVM3 { get; set; }

            public string Text { get; set; } = "Text1";
        }

        public class OmgViewModelWithIsAlsoAService
        {
            public string TestProp { get; set; } = "Text2";
        }

        public class OmgViewModelWithIsAlsoAService2
        {
            public string TestProp { get; set; } = "Text3";
        }

        public class OmgViewModelWhichCannotBeCreatedByConstructorButIsInstantiatedManually
        {
            private readonly int b;

            public int A { get; set; }

            public OmgViewModelWhichCannotBeCreatedByConstructorButIsInstantiatedManually(int b)
            {
                this.b = b;
            }
        }
    }
}
