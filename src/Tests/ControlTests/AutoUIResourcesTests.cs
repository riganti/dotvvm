using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.AutoUI;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests;

[TestClass]
public class AutoUIResourcesTests
{
    private static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
        _ = Repeater.RenderAsNamedTemplateProperty;
        config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
    }, services: s => {
        s.AddAutoUI(options => {
            options.PropertyDisplayNamesResourceFile = typeof(AutoUIPropertyNames);
            options.ErrorMessagesResourceFile = typeof(AutoUIErrorMessages);
        });
    });
    OutputChecker check = new OutputChecker("testoutputs");

    [DataTestMethod]
    [DataRow("en")]
    [DataRow("cs")]
    public async Task ResourceLabelsFormTest(string culture)
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        var originalUiCulture = Thread.CurrentThread.CurrentUICulture;
        try
        {
            var r = await cth.RunPage(typeof(SimpleViewModel), @"
                <auto:Form DataContext={value: Entity1} />
                <auto:Form DataContext={value: Entity2} />
                <br />
                Culture: {{resource: System.Threading.Thread.CurrentThread.CurrentCulture.Name}}
                <br />
                <dot:Button Text=Validate Click={command: Test()} Validation.Enabled=true />
                ",
                fileName: $"ResourceLabelsFormTest_{culture}.dothtml",
                culture: new CultureInfo(culture)
            );
            //check.CheckString(r.FormattedHtml, fileExtension: "html", checkName: culture);

            r.ViewModel.Entity1.FirstName = "very long value is set in the field";
            r.ViewModel.Entity1.Email = "invalid mail";

            var validationResult = await r.RunCommand("Test()", applyChanges: false, culture: new CultureInfo(culture));
            check.CheckString(validationResult.ResultText, fileExtension: "json", checkName: culture);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
        }
    }

    public class SimpleViewModel
    {
        public SimpleEntity Entity1 { get; set; } = new();
        public SecondEntity Entity2 { get; set; } = new();

        public void Test()
        {
        }
    }

    public class SimpleEntity
    {
        [MaxLength(10, ErrorMessageResourceType = typeof(CustomResourceFile), ErrorMessageResourceName = "CustomError")]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [EmailAddress]
        [MinLength(20, ErrorMessage = "Custom overriden error message")]
        public string Email { get; set; }
    }

    public class SecondEntity
    {
        [RegularExpression("[a-zA-Z]+")]
        public string FirstName { get; set; }
    }
}
