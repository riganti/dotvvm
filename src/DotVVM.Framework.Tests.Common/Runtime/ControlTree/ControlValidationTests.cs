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
        public DateTime? NullabelDate { get; set; }
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

        [TestMethod]
        public void ComboBox_InvalidNullableDifference()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:ComboBox DataSource='{value: NullableDateList}' SelectedValue='{value: NonNullableDate}' />"));

            // TODO: check exception message
        }

        [TestMethod]
        public void ComboBox_AllowedNullableDifference()
        {
            var control =
                ParseControl("<dot:ComboBox DataSource='{value: NullableDateList}' SelectedValue='{value: NullableDate}' />");

            // TODO: check tree
        }

        [TestMethod]
        public void CheckBox_InvalidItemsType()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:CheckBox CheckedItems='{value: VmArray}' CheckedValue='{value: GuidProp}' />"));

            // TODO: check exception message
        }

        [TestMethod]
        public void CheckBox_InvalidNullableDifference()
        {
            var exception = Assert.ThrowsException<DotvvmCompilationException>(() =>
                ParseControl("<dot:CheckBox CheckedItems='{value: NullableDateList}' CheckedValue='{value: NonNullableDate}' />"));

            // TODO: check exception message
        }
    }
}
