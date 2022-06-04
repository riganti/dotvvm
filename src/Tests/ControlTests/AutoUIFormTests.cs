using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.AutoUI;
using DotVVM.AutoUI.Annotations;
using DotVVM.AutoUI.ViewModel;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class AutoUIFormTests
    {
        private static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            _ = Repeater.RenderAsNamedTemplateProperty;
            config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
        }, services: s => {
            s.AddAutoUI();
            s.Services.AddSingleton<ISelectionProvider<ProductSelection>, ProductSelectionProvider>();
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task BulmaFormTest()
        {
            var r = await cth.RunPage(typeof(FormTestViewModel), @"
                <auto:BulmaForm DataContext={value: Entity} />
                "
            );
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task BootstrapFormTest()
        {
            var r = await cth.RunPage(typeof(FormTestViewModel), @"
                <auto:BootstrapForm DataContext={value: Entity} />
                "
            );
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class FormTestViewModel
        {
            public FormTestEntity Entity { get; set; } = new();

            public SelectionViewModel<ProductSelection> Products { get; set; } = new();
        }

        public class FormTestEntity
        {
            public string StringProp { get; set; }

            [DataType(DataType.MultilineText)]
            public string MultiLineStringProp { get; set; }

            public bool BoolProp { get; set; }

            public TestEnum EnumProp { get; set; }

            [Selection(typeof(ProductSelection))]
            public int? ProductId { get; set; }

            [Selection(typeof(ProductSelection))]
            public List<int> Products { get; set; } = new();
        }

        public enum TestEnum
        {
            One,
            Two
        }

        public record ProductSelection : Selection<int>;

        public class ProductSelectionProvider : ISelectionProvider<ProductSelection>
        {
            public Task<List<ProductSelection>> GetSelectorItems() => Task.FromResult(new List<ProductSelection>()
            {
                new() { DisplayName = "One", Value = 1 },
                new() { DisplayName = "Two", Value = 2 }
            });
        }
    }

}
