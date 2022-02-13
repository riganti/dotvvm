using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Controls.DynamicData;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class DynamicDataTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            _ = Repeater.RenderAsNamedTemplateProperty;
            config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
        }, services: s => {
            s.AddDynamicData();
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task BasicDynamicEntity()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dd:DynamicEntity DataContext={value: Entity}
                            Changed-Name={command: 0} />
                "
            );

            CollectionAssert.AreEqual(new WrappedHtmlControl2[0], r.View.GetAllDescendants().OfType<WrappedHtmlControl2>().ToArray());

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task BasicDynamicEditor()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dd:DynamicEditor Property={value: Integer} />
                    <dd:DynamicEditor Property={value: Boolean} />
                "
            );

            CollectionAssert.AreEqual(new WrappedHtmlControl2[0], r.View.GetAllDescendants().OfType<WrappedHtmlControl2>().ToArray());

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class SimpleEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class BasicTestViewModel: DotvvmViewModelBase
        {
            [Bind(Name = "int")]
            [DisplayFormat(DataFormatString = "0000000000")]
            public int Integer { get; set; } = 17;
            public bool Boolean { get; set; }
            public SimpleEntity Entity { get; set; }
            public bool AfterPreRender { get; set; }

            public List<SimpleEntity> List { get; set; } = new List<string> { "list-item1", "list-item2" }.Select((s, i) => new SimpleEntity { Id = i, Name = s }).ToList();

            public override Task PreRender()
            {
                AfterPreRender = true;
                return base.PreRender();
            }
        }
    }

}
