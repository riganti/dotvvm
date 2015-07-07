using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class NamingContainersTests
    {

        [TestMethod]
        public void NamingContainers_EnsureControlHasID_GenerateUniqueID()
        {
            var page = new DotvvmView();
            page.SetValue(Internal.UniqueIDProperty, "c0");

            var head = new HtmlGenericControl("head");
            head.SetValue(Internal.UniqueIDProperty, "c1");
            page.Children.Add(head);

            var title = new HtmlGenericControl("title");
            title.SetValue(Internal.UniqueIDProperty, "c2");
            head.Children.Add(title);

            var body = new HtmlGenericControl("body");
            body.SetValue(Internal.UniqueIDProperty, "c3");
            page.Children.Add(body);

            var div1 = new HtmlGenericControl("div");
            div1.SetValue(Internal.UniqueIDProperty, "c4");
            body.Children.Add(div1);

            var div2 = new HtmlGenericControl("div");
            div2.SetValue(Internal.UniqueIDProperty, "c5");
            div2.SetValue(Internal.IsNamingContainerProperty, true);
            body.Children.Add(div2);

            var div3 = new HtmlGenericControl("div");
            div3.SetValue(Internal.UniqueIDProperty, "c6");
            div2.Children.Add(div3);

            div3.EnsureControlHasId(true);
            div1.EnsureControlHasId(true);

            Assert.AreEqual("c4", div1.ID);
            Assert.AreEqual("c5_c6", div3.ID);
        }

    }
}
