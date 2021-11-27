using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmControlErrorsTests : DotvvmControlTestBase
    {
        [TestMethod]
        public void DotvvmBindableControl_DataContextSetToHardCodedValue_ThrowsException()
        {
            var div = "<div DataContext=\"\"></div>";
            var dotvvmBuilder = CreateControlRenderer(div, new object());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "The property 'DotvvmBindableObject.DataContext' cannot contain hard coded value");
        }

        [TestMethod]
        public void CheckBox_NotAllowedHardcodedValue_ThrowsException()
        {
            var checkbox = "<dot:CheckBox Checked=\"false\" />";
            var dotvvmBuilder = CreateControlRenderer(checkbox, new object());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder()); 
            StringAssert.Contains(exc.Message, "The property 'CheckBox.Checked' cannot contain hard coded value");
        }

        [TestMethod]
        public void CheckBox_WrongPropertyValue_ThrowsException()
        {
            var checkbox = "<dot:CheckBox Checked=\"{value: 'NotAllowedValue' }\" />";
            var dotvvmBuilder = CreateControlRenderer(checkbox, new object());

            Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
        }

        [TestMethod]
        public void CheckBox_MissingRequiredProperty_ThrowsException()
        {
            var checkbox = "<dot:CheckBox />";
            var dotvvmBuilder = CreateControlRenderer(checkbox, new object());

            Assert.ThrowsException<DotvvmControlException>(() => dotvvmBuilder());
        }

        [TestMethod]
        public void DotvvmControl_NonExistingProperty_ThrowsException()
        {
            var control = "<dot:InlineScript NonExistingProperty=\"test\" />";
            var dotvvmBuilder = CreateControlRenderer(control, new object());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "does not have a property 'NonExistingProperty'");
        }

        [TestMethod]
        public void DotvvmControl_NonExistingControl_ThrowsException()
        {
            var control = "<dot:NonExistingControl />";
            var dotvvmBuilder = CreateControlRenderer(control, new object());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "control <dot:NonExistingControl> could not be resolved");
        }

        [TestMethod]
        public void DotvvmControl_UnknownInnerContent_ThrowsException()
        {
            var control = "<dot:ConfirmPostBackHandler Message=\"Confirmation 1\" />";
            var dotvvmBuilder = CreateControlRenderer(control, new object());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "Content control must inherit from DotvvmControl, but DotVVM.Framework.Controls.ConfirmPostBackHandler doesn't.");
        }

        [TestMethod]
        public void Button_ControlUsageValidation_ThrowsException()
        {
            var control = "<dot:Button Click=\"{command: null}\" Text=\"Text property\" >Template content</dot:Button>";
            var dotvvmBuilder = CreateControlRenderer(control, new object());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "Text property and inner content of the <dot:Button> control cannot be set at the same time");
        }

        [TestMethod]
        public void Button_NonExistingCommand_ThrowsException()
        {
            var control = "<dot:Button Click=\"{command: NonExistingCommand()}\" /></body>";
            var dotvvmBuilder = CreateControlRenderer(control, new object());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "Could not initialize binding");
        }

        [TestMethod]
        public void CheckBox_NonExistingViewModelProperty_ThrowsException()
        {
            var control = "<dot:CheckBox Checked=\"{value: InvalidPropertyName}\" />";
            var dotvvmBuilder = CreateControlRenderer(control, new object());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "Could not initialize binding");
        }

        [TestMethod]
        public void CheckBox_ControlUsageValidation_CheckedValue_ComplexType_ThrowsException()
        {
            var control = "<dot:CheckBox CheckedItems=\"{value: Colors}\" CheckedValue=\"{value: Colors[0]}\" />";
            var dotvvmBuilder = CreateControlRenderer(control, new CheckBoxTestViewModel());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "The ItemKeyBinding property must be specified when the CheckedValue property contains a complex type.");
        }

        [TestMethod]
        public void CheckBox_ControlUsageValidation_ItemKeyBinding_ComplexType_ThrowsException()
        {
            var control = "<dot:CheckBox CheckedItems=\"{value: Colors}\" CheckedValue=\"{value: Colors[0]}\" ItemKeyBinding=\"{value: _this}\" />";
            var dotvvmBuilder = CreateControlRenderer(control, new CheckBoxTestViewModel());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "The ItemKeyBinding property must return a value of a primitive type.");
        }

        [TestMethod]
        public void CheckBox_ControlUsageValidation_ItemKeyBinding_CheckedValue_Not_Binding_ThrowsException()
        {
            var control = "<dot:CheckBox CheckedItems=\"{value: Strings}\" CheckedValue=\"a\" ItemKeyBinding=\"{value: 'a'}\" />";
            var dotvvmBuilder = CreateControlRenderer(control, new CheckBoxTestViewModel());

            var exc = Assert.ThrowsException<DotvvmCompilationException>(() => dotvvmBuilder());
            StringAssert.Contains(exc.Message, "The ItemKeyBinding property can be only used when CheckedValue is a binding.");
        }

    }
    public class CheckBoxTestViewModel
    {
        public List<ColorData> Colors { get; set; }
        public List<string> Strings { get; set; }
        public List<ColorData> SelectedColors { get; set; }
    }
    public class ColorData
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
