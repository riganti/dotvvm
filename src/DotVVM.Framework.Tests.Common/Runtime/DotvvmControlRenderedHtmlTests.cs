using System.Collections;
using System.Collections.Generic;
using System.IO;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmControlRenderedHtmlTests : DotvvmControlTestBase
    {
        [TestMethod]
        public void GridViewTextColumn_RenderedHtmlTest_ServerRendering()
        {
            var gridView = new GridView() {
                Columns = new List<GridViewColumn>
                {
                    new GridViewTextColumn() { HeaderCssClass = "lol", HeaderText="Header Text", ValueBinding = ValueBindingExpression.CreateBinding(BindingService, h => (object)h[0], (DataContextStack)null) }
                },
                DataSource = ValueBindingExpression.CreateBinding(BindingService, h => (IList)h[0], (DataContextStack)null),
            };
            gridView.SetValue(RenderSettings.ModeProperty, RenderMode.Server);
            var viewModel = new[] { "ROW 1", "ROW 2", "ROW 3" };
            var html = InvokeLifecycleAndRender(gridView, CreateContext(viewModel));

            Assert.IsTrue(html.Contains("ROW 2"));
            Assert.IsTrue(html.Contains("class=\"lol\""));
        }

        [TestMethod]
        public void RepeaterEmptyData_RenderedHtmlTest_ServerRendering()
        {
            Repeater createRepeater(RenderMode renderMode)
            {
                var repeater = new Repeater() {
                    ItemTemplate = new DelegateTemplate((f, s, c) => c.Children.Add(new HtmlGenericControl("ITEM_TAG"))),
                    EmptyDataTemplate = new DelegateTemplate((f, s, c) => c.Children.Add(new HtmlGenericControl("EMPTY_DATA"))),
                    DataSource = ValueBindingExpression.CreateThisBinding<string[]>(Configuration.ServiceProvider.GetRequiredService<BindingCompilationService>(), null),
                    RenderWrapperTag = false
                };
                repeater.SetValue(RenderSettings.ModeProperty, renderMode);
                return repeater;
            }
            var viewModel = new string[] { };
            var clientHtml = InvokeLifecycleAndRender(createRepeater(RenderMode.Client), CreateContext(viewModel));

            Assert.IsTrue(clientHtml.Contains("<ITEM_TAG"));
            Assert.IsTrue(clientHtml.Contains("<EMPTY_DATA"));
            Assert.IsTrue(!clientHtml.Contains("<div"));
            Assert.IsTrue(clientHtml.Contains("<!-- ko "));

            var serverHtml = InvokeLifecycleAndRender(createRepeater(RenderMode.Server), CreateContext(viewModel));
            Assert.IsTrue(serverHtml.Contains("<EMPTY_DATA"));
            Assert.IsTrue(!serverHtml.Contains("<div"));
        }

        [TestMethod]
        public void BindingGroup_EnumProperty()
        {
            var writer = new StringWriter();
            var html = new HtmlWriter(writer, CreateContext(new object()));
            html.AddKnockoutDataBind("tt", new KnockoutBindingGroup() {
                { "test", new TextBox(){ Type = TextBoxType.Date }, TextBox.TypeProperty }
            });
            html.RenderSelfClosingTag("span");
            Assert.AreEqual("<spandata-bind=\"tt:{&quot;test&quot;:&quot;Date&quot;}\"/>", writer.ToString().Replace(" ", ""));
        }

        [TestMethod]
        public void BindingGroup_MultipleBindings_InProperOrder()
        {
            var textbox = new OrderedDataBindTextBox();
            textbox.SetBinding(TextBox.TextProperty,
                ValueBindingExpression.CreateThisBinding<string>(Configuration.ServiceProvider.GetRequiredService<BindingCompilationService>(), null));

            var html = InvokeLifecycleAndRender(textbox, CreateContext(string.Empty));

            StringAssert.Contains(html.Replace(" ", ""), "data-bind=\"first:true,value:$rawData,second:true\"");
        }

        [TestMethod]
        public void MarkupControl_NoWrapperTagDirective()
        {
            var viewModel = new string[] { };
            var clientHtml = InvokeLifecycleAndRender(new HtmlGenericControl("elem1") {
                Children = {
                    new DotvvmMarkupControl(){
                        Directives = {
                            ["noWrapperTag"] = ""
                        },
                        Children = {
                            new HtmlGenericControl("elem2")
                        }
                    }
                }
            }, CreateContext(viewModel));

            Assert.IsTrue(clientHtml.Contains("<elem1"));
            Assert.IsTrue(clientHtml.Contains("<elem2"));
            Assert.IsTrue(!clientHtml.Contains("<div"));
            Assert.IsTrue(clientHtml.Contains("<!-- ko "));
        }

        [TestMethod]
        public void HtmlGenericControl_MetaTag_RenderContentAttribute()
        {
            var context = CreateContext(new object());
            var mockHttpContext = new Mock<IHttpContext>();
            var mockHttpRequest = new Mock<IHttpRequest>();
            var mockPathBase = new Mock<IPathString>();

            mockPathBase.Setup(p => p.Value).Returns("home");
            mockHttpRequest.Setup(p => p.PathBase).Returns(mockPathBase.Object);
            mockHttpContext.Setup(p => p.Request).Returns(mockHttpRequest.Object);
            context.HttpContext = mockHttpContext.Object;

            var clientHtml = InvokeLifecycleAndRender(new HtmlGenericControl("meta") {
                Attributes =
                {
                    { "content", "~/test" }
                }
            }, context);

            Assert.IsTrue(clientHtml.Contains("<meta"));
            Assert.IsTrue(clientHtml.Contains("/home/test"));
            Assert.IsTrue(!clientHtml.Contains("~"));
        }

        [TestMethod]
        public void MarkupControl_WrapperTagDirective()
        {
            var viewModel = new string[] { };
            var clientHtml = InvokeLifecycleAndRender(new HtmlGenericControl("elem1") {
                Children = {
                    new DotvvmMarkupControl(){
                        Directives = {
                            ["wrapperTag"] = "elem3"
                        },
                        Children = {
                            new HtmlGenericControl("elem2")
                        }
                    }
                }
            }, CreateContext(viewModel));

            Assert.IsTrue(clientHtml.Contains("<elem1"));
            Assert.IsTrue(clientHtml.Contains("<elem2"));
            Assert.IsTrue(!clientHtml.Contains("<div"));
            Assert.IsTrue(clientHtml.Contains("<elem3"));
        }

        [TestMethod]
        [ExpectedException(typeof(DotvvmControlException))]
        public void MarkupControl_WrapperTagAndNoWrapperTagDirective()
        {
            var viewModel = new string[] { };
            var clientHtml = InvokeLifecycleAndRender(new HtmlGenericControl("elem1") {
                Children = {
                    new DotvvmMarkupControl(){
                        Directives = {
                            ["wrapperTag"] = "elem3",
                            ["noWrapperTag"] = ""
                        },
                        Children = {
                            new HtmlGenericControl("elem2")
                        }
                    }
                }
            }, CreateContext(viewModel));
        }

        public class OrderedDataBindTextBox : TextBox
        {
            protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
            {
                writer.AddKnockoutDataBind("first", "true");

                base.AddAttributesToRender(writer, context);

                writer.AddKnockoutDataBind("second", "true");
            }
        }
    }
}
