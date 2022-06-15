using CheckTestOutput;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class CommandTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {

        }, services: services => {
            services.AddTransient<OmgViewModelWithIsAlsoAService>();
        });
        OutputChecker check = new OutputChecker("testoutputs");


        [TestMethod]
        public async Task RootViewModelIsRecord()
        {
            var r = await cth.RunPage(typeof(ViewModel1), " <dot:Button Text=Click Click={command: Text = NestedVM.TestProp} /> ");

            Assert.AreEqual("Text1", (string)r.ViewModel.Text);
            Assert.AreEqual("Text2", (string)r.ViewModel.NestedVM.TestProp);

            await r.RunCommand("Text = NestedVM.TestProp");

            Assert.AreEqual("Text2", (string)r.ViewModel.Text);
        }

        
        public class ViewModel1
        {
            // don't ask me why people do this...
            // on one project, 4.1 upgrade did not work, because they try to inject
            // a service into viewmodel which is also a nested viewmodel property 🤦
            // DotVVM obviously thinks it's a viewmodel, not a service, so it
            // re-creates the object after deserialization, which broke other things if it's a root viewmodel
            public ViewModel1(OmgViewModelWithIsAlsoAService nestedVM)
            {
                NestedVM = nestedVM;
            }

            public OmgViewModelWithIsAlsoAService NestedVM { get; }

            public string Text { get; set; } = "Text1";
        }

        public class OmgViewModelWithIsAlsoAService
        {
            public string TestProp { get; set; } = "Text2";
        }
        
    }
}
