using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Runtime.ControlTree
{

    internal class TestViewModel : Tests.Binding.TestViewModel
    {
        public StructList<DateTime?>? NullableDateList { get; }
        public StructList<DateTime>? DateList { get; }

        public DateTime NonNullableDate { get; set; }
        public DateTime? NullableDate { get; set; }
    }
    public struct StructList<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() =>
            new List<T> { default(T) }.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
    [TestClass]
    public class ControlValidationTests
    {


        ResolvedControl ParseControl(string markup, Type viewModel = null)
        {
            viewModel = viewModel ?? typeof(TestViewModel);
            var tree = DotvvmTestHelper.ParseResolvedTree($"@viewModel {viewModel}\n{markup}");

            var control = tree.Content.First(c => c.Metadata.Type != typeof(RawLiteral));
            return control;
        }

        [TestMethod]
        public void ComboBox_InvalidItemsType()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:ComboBox DataSource='{value: VmArray}' SelectedValue='{value: GuidProp}' />"));

            // TODO: check exception message
        }

        [DataTestMethod]
        [DataRow("DateList", "NonNullableDate")]
        [DataRow("DateList", "NullableDate")]
        [DataRow("NullableDateList", "NullableDate")]
        public void ComboBox_AllowedNullableDifference(string dataSourceProperty, string selectedValueProperty)
        {
            var control =
                ParseControl($"<dot:ComboBox DataSource='{{value: {dataSourceProperty}}}' SelectedValue='{{value: {selectedValueProperty}}}' />");

            // TODO: check tree
        }

        [TestMethod]
        public void ComboBox_InvalidNullableDifference()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:ComboBox DataSource='{value: NullableDateList}' SelectedValue='{value: NonNullableDate}' />"));

            // TODO: check exception message
        }

        [DataTestMethod]
        [DataRow("NonNullableDate", "NonNullableDate")]
        [DataRow("NonNullableDate", "NullableDate")]
        [DataRow("NullableDate", "NullableDate")]
        public void ComboBox_ItemValueBinding_AllowedNullableDifference(string itemValueBinding, string selectedValueProperty)
        {
            var control =
                ParseControl($"<dot:ComboBox DataSource='{{value: VmArray}}' ItemValueBinding='{{value: {itemValueBinding}}}' SelectedValue='{{value: {selectedValueProperty}}}' />");

            // TODO: check tree
        }

        [TestMethod]
        public void ComboBox_ItemValueBinding_InvalidNullableDifference()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:ComboBox DataSource='{value: VmArray}' ItemValueBinding='{value: NullableDate}' SelectedValue='{value: NonNullableDate}' />"));

            // TODO: check exception message
        }

        [DataTestMethod]
        [DataRow("NonNullableDate")]
        [DataRow("NullableDate")]
        [DataRow("MyProperty")]
        [DataRow("Enum")]
        [DataRow("NullableEnum")]
        public void ComboBox_ItemValueBinding_AllowedTypes(string itemValueBinding)
        {
            var control =
                ParseControl($"<dot:ComboBox DataSource='{{value: VmArray}}' ItemValueBinding='{{value: {itemValueBinding}}}' SelectedValue='{{value: VmArray[0].{itemValueBinding}}}' />");

            // TODO: check tree
        }

        [DataTestMethod]
        [DataRow("Collection")]
        [DataRow("ChildObject")]
        [DataRow("Struct")]
        public void ComboBox_ItemValueBinding_NotAllowedTypes(string itemValueBinding)
        {
            var control = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl($"<dot:ComboBox DataSource='{{value: VmArray}}' ItemValueBinding='{{value: {itemValueBinding}}}' SelectedValue='{{value: VmArray[0].{itemValueBinding}}}' />"));

            // TODO: check tree
        }


        [TestMethod]
        public void CheckBox_InvalidNullableDifference()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:CheckBox CheckedItems='{value: DateList}' CheckedValue='{value: NullableDate}' />"));

            // TODO: check exception message
        }

        [DataTestMethod]
        [DataRow("DateList", "NonNullableDate")]
        [DataRow("NullableDateList", "NonNullableDate")]
        [DataRow("NullableDateList", "NullableDate")]
        public void CheckBox_AllowedNullableDifference(string checkedItemsProperty, string checkedValueProperty)
        {
            var control = ParseControl($"<dot:CheckBox CheckedItems='{{value: {checkedItemsProperty}}}' CheckedValue='{{value: {checkedValueProperty}}}' />");
            // TODO: check tree
        }

        [TestMethod]
        public void CheckBox_InvalidItemsType()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:CheckBox CheckedItems='{value: VmArray}' CheckedValue='{value: GuidProp}' />"));

            // TODO: check exception message
        }


        [DataTestMethod]
        [DataRow("NonNullableDate", "NonNullableDate")]
        [DataRow("NullableDate", "NonNullableDate")]
        [DataRow("NullableDate", "NullableDate")]
        public void Radio_AllowedNullableDifference(string checkedItemProperty, string checkedValueProperty)
        {
            var control = ParseControl($"<dot:RadioButton CheckedItem='{{value: {checkedItemProperty}}}' CheckedValue='{{value: {checkedValueProperty}}}' />");
            // TODO: check tree
        }

        [TestMethod]
        public void Radio_InvalidNullableDifference()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:RadioButton CheckedItem='{value: NonNullableDate}' CheckedValue='{value: NullableDate}' />"));

            // TODO: check exception message
        }

        [TestMethod]
        public void Radio_InvalidItemsType()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:RadioButton CheckedItem='{value: VmArray[0]}' CheckedValue='{value: GuidProp}' />"));

            // TODO: check exception message
        }

        [TestMethod]
        public void ClientModule_SingleOccurenceOnly()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:ClientModule></dot:ClientModule> <dot:ClientModule></dot:ClientModule>"));

            // TODO: check exception message
        }

        [TestMethod]
        public void ClientModule_InRootScopeOnly()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<div><dot:ClientModule></dot:ClientModule></div>"));

            // TODO: check exception message
        }
    }
}
