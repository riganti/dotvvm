using System;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmControlFindTests : DotvvmControlTestBase
    {
        [TestMethod]
        public void FindControlInContainer_Generic_ReturnsNull_WhenNotFound()
        {
            var parent = new HtmlGenericControl("div");
            var child = new TextBox { ID = "myTextBox" };
            parent.Children.Add(child);

            var result = parent.FindControlInContainer<TextBox>("nonExistentId");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindControlInContainer_Generic_Throws_WhenNotFoundAndThrowIfNotFound()
        {
            var parent = new HtmlGenericControl("div");
            var child = new TextBox { ID = "myTextBox" };
            parent.Children.Add(child);

            Assert.ThrowsException<Exception>(() => parent.FindControlInContainer<TextBox>("nonExistentId", throwIfNotFound: true));
        }

        [TestMethod]
        public void FindControlInContainer_Generic_Throws_WhenWrongType()
        {
            var parent = new HtmlGenericControl("div");
            var child = new Literal { ID = "myLiteral" };
            parent.Children.Add(child);

            var ex = Assert.ThrowsException<DotvvmControlException>(() => parent.FindControlInContainer<TextBox>("myLiteral"));
            StringAssert.Contains(ex.Message, "not an instance of the desired type");
        }

        [TestMethod]
        public void FindControlInContainer_Generic_ReturnsControl_WhenFoundWithCorrectType()
        {
            var parent = new HtmlGenericControl("div");
            var child = new TextBox { ID = "myTextBox" };
            parent.Children.Add(child);

            var result = parent.FindControlInContainer<TextBox>("myTextBox");
            Assert.AreSame(child, result);
        }

        [TestMethod]
        public void FindControlByClientId_Generic_ReturnsNull_WhenNotFound()
        {
            var parent = new HtmlGenericControl("div");
            var child = new TextBox { ID = "myTextBox" };
            parent.Children.Add(child);

            var result = parent.FindControlByClientId<TextBox>("nonExistentClientId");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindControlByClientId_Generic_Throws_WhenNotFoundAndThrowIfNotFound()
        {
            var parent = new HtmlGenericControl("div");
            var child = new TextBox { ID = "myTextBox" };
            parent.Children.Add(child);

            Assert.ThrowsException<Exception>(() => parent.FindControlByClientId<TextBox>("nonExistentClientId", throwIfNotFound: true));
        }

        [TestMethod]
        public void FindControlByClientId_Generic_Throws_WhenWrongType()
        {
            var parent = new HtmlGenericControl("div");
            var child = new Literal { ID = "myLiteral" };
            parent.Children.Add(child);

            var ex = Assert.ThrowsException<DotvvmControlException>(() => parent.FindControlByClientId<TextBox>("myLiteral"));
            StringAssert.Contains(ex.Message, "not an instance of the desired type");
        }

        [TestMethod]
        public void FindControlByClientId_Generic_ReturnsControl_WhenFoundWithCorrectType()
        {
            var parent = new HtmlGenericControl("div");
            var child = new TextBox { ID = "myTextBox" };
            parent.Children.Add(child);

            var result = parent.FindControlByClientId<TextBox>("myTextBox");
            Assert.AreSame(child, result);
        }

        [TestMethod]
        public void FindControlByClientId_Generic_ReturnsControl_WhenClientIdDiffersFromId()
        {
            var parent = new HtmlGenericControl("div");
            var child = new TextBox { ID = "markupId" };
            child.SetValue(DotvvmControl.ClientIDProperty, "renderedClientId");
            parent.Children.Add(child);

            var result = parent.FindControlByClientId<TextBox>("renderedClientId");
            Assert.AreSame(child, result);
            Assert.IsNull(parent.FindControlByClientId<TextBox>("markupId"));
        }
    }
}
