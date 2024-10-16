using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmBindableObjectTests
    {
        readonly BindingCompilationService bindingService = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<BindingCompilationService>();
        readonly DataContextStack dataContext = DataContextStack.Create(typeof(TestViewModel));

        [TestMethod]
        public void CopyProperty_Error_NotSet()
        {
            var source = new HtmlGenericControl("div");
            var target = new HtmlGenericControl("div");

            var ex = Assert.ThrowsException<DotvvmControlException>(() => source.CopyProperty(HtmlGenericControl.VisibleProperty, target, HtmlGenericControl.VisibleProperty, throwOnFailure: true));
            StringAssert.Contains(ex.Message, "Visible is not set");
        }

        [TestMethod]
        public void CopyProperty_Nop_NotSet()
        {
            var source = new HtmlGenericControl("div");
            var target = new HtmlGenericControl("div");

            source.CopyProperty(HtmlGenericControl.VisibleProperty, target, HtmlGenericControl.VisibleProperty); // throwOnFailure: false is default
            Assert.IsFalse(target.IsPropertySet(HtmlGenericControl.VisibleProperty));
        }

        [TestMethod]
        public void CopyProperty_Copy_Value()
        {
            var source = new HtmlGenericControl("div");
            source.SetValue(HtmlGenericControl.VisibleProperty, (object)false);
            var target = new HtmlGenericControl("div");
            source.CopyProperty(HtmlGenericControl.VisibleProperty, target, HtmlGenericControl.VisibleProperty);

            Assert.IsFalse(target.GetValue<bool>(HtmlGenericControl.VisibleProperty));
            Assert.AreSame(source.GetValue(HtmlGenericControl.VisibleProperty), target.GetValue(HtmlGenericControl.VisibleProperty));
        }

        [TestMethod]
        public void CopyProperty_Copy_Binding()
        {
            var source = new HtmlGenericControl("div");
            source.DataContext = new TestViewModel { IntProp = 0 };
            source.SetValue(Internal.DataContextTypeProperty, dataContext);
            source.SetValue(HtmlGenericControl.VisibleProperty, bindingService.Cache.CreateValueBinding("IntProp == 12", dataContext));
            var target = new HtmlGenericControl("div");
            source.CopyProperty(HtmlGenericControl.VisibleProperty, target, HtmlGenericControl.VisibleProperty);
            target.DataContext = source.DataContext;

            Assert.IsFalse(source.GetValue<bool>(HtmlGenericControl.VisibleProperty));
            Assert.IsFalse(target.GetValue<bool>(HtmlGenericControl.VisibleProperty));
            Assert.AreSame(source.GetValue(HtmlGenericControl.VisibleProperty), target.GetValue(HtmlGenericControl.VisibleProperty));
        }

        [TestMethod]
        public void CopyProperty_EvalBinding()
        {
            var source = new HtmlGenericControl("div");
            source.DataContext = new TestViewModel { IntProp = 0 };
            source.SetValue(Internal.DataContextTypeProperty, dataContext);
            source.SetValue(HtmlGenericControl.VisibleProperty, bindingService.Cache.CreateValueBinding("IntProp == 12", dataContext));

            Assert.IsFalse(Button.IsSubmitButtonProperty.MarkupOptions.AllowBinding);
            var target = new Button();
            source.CopyProperty(HtmlGenericControl.VisibleProperty, target, Button.IsSubmitButtonProperty);
            target.DataContext = source.DataContext;

            Assert.IsFalse(target.IsSubmitButton);
            Assert.AreEqual(false, target.GetValueRaw(Button.IsSubmitButtonProperty));
        }

        [TestMethod]
        public void CopyProperty_Error_ValueToBinding()
        {
            var source = new HtmlGenericControl("div");
            source.SetValue(HtmlGenericControl.VisibleProperty, (object)false);
            Assert.IsFalse(CheckBox.CheckedProperty.MarkupOptions.AllowHardCodedValue);
            var target = new CheckBox();

            var ex = Assert.ThrowsException<DotvvmControlException>(() =>
                source.CopyProperty(HtmlGenericControl.VisibleProperty, target, CheckBox.CheckedProperty, throwOnFailure: true));
            StringAssert.Contains(ex.Message, "Checked does not support hard coded values");
        }

        [TestMethod]
        public void CopyProperty_Nop_ValueToBinding()
        {
            // TODO: this is a weird behavior, I'd consider changing it in a future major version
            var source = new HtmlGenericControl("div");
            source.SetValue(HtmlGenericControl.VisibleProperty, (object)false);
            Assert.IsFalse(CheckBox.CheckedProperty.MarkupOptions.AllowHardCodedValue);
            var target = new CheckBox();

            source.CopyProperty(HtmlGenericControl.VisibleProperty, target, CheckBox.CheckedProperty);
            Assert.IsFalse(target.IsPropertySet(CheckBox.CheckedProperty));
        }

        [TestMethod]
        public void CopyProperty_Copy_InheritedBinding()
        {
            var sourceParent = new HtmlGenericControl("div");
            sourceParent.DataContext = new TestViewModel { IntProp = 0 };
            sourceParent.SetValue(Internal.DataContextTypeProperty, dataContext);
            sourceParent.SetValue(Validation.EnabledProperty, bindingService.Cache.CreateValueBinding("IntProp == 12", dataContext));
            var source = new HtmlGenericControl("div");
            sourceParent.Children.Add(source);

            var target = new HtmlGenericControl("div");
            source.CopyProperty(Validation.EnabledProperty, target, Validation.EnabledProperty);

            Assert.AreSame(sourceParent.GetValue(Validation.EnabledProperty), target.GetValue(Validation.EnabledProperty));
        }

        [TestMethod]
        public void CopyProperty_Copy_InheritedValue()
        {
            var sourceParent = new HtmlGenericControl("div");
            sourceParent.SetValue(Validation.EnabledProperty, (object)false);
            var source = new HtmlGenericControl("div");
            sourceParent.Children.Add(source);

            var target = new HtmlGenericControl("div");
            source.CopyProperty(Validation.EnabledProperty, target, Validation.EnabledProperty);

            Assert.AreSame(sourceParent.GetValue(Validation.EnabledProperty), target.GetValue(Validation.EnabledProperty));
        }


        [TestMethod]
        public void CopyProperty_Copy_FormControlsEnabledBinding()
        {
            var sourceParent = new HtmlGenericControl("div");
            sourceParent.DataContext = new TestViewModel { IntProp = 0 };
            sourceParent.SetValue(Internal.DataContextTypeProperty, dataContext);
            sourceParent.SetValue(FormControls.EnabledProperty, bindingService.Cache.CreateValueBinding("IntProp == 12", dataContext));
            var source = new TextBox();
            sourceParent.Children.Add(source);

            var target = new TextBox();
            source.CopyProperty(TextBox.EnabledProperty, target, TextBox.EnabledProperty);
            target.DataContext = source.DataContext;

            Assert.AreSame(sourceParent.GetValue(FormControls.EnabledProperty), target.GetValue(TextBox.EnabledProperty));
            Assert.AreSame(source.GetValue(TextBox.EnabledProperty), target.GetValue(TextBox.EnabledProperty));
        }

        [TestMethod]
        public void CopyProperty_Copy_FormControlsEnabledValue()
        {
            var sourceParent = new HtmlGenericControl("div");
            sourceParent.SetValue(FormControls.EnabledProperty, (object)false);
            var source = new TextBox();
            sourceParent.Children.Add(source);

            var target = new TextBox();
            source.CopyProperty(TextBox.EnabledProperty, target, TextBox.EnabledProperty);

            Assert.AreSame(sourceParent.GetValue(FormControls.EnabledProperty), target.GetValue(TextBox.EnabledProperty));
            Assert.AreSame(source.GetValue(TextBox.EnabledProperty), target.GetValue(TextBox.EnabledProperty));
        }

    }
}
