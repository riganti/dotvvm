using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    public abstract class DefaultControlTreeResolverTestsBase
    {
        protected static DotvvmConfiguration configuration { get; }

        static DefaultControlTreeResolverTestsBase()
        {
            var fakeMarkupFileLoader = new FakeMarkupFileLoader() {
                MarkupFiles = {
                    ["ControlWithBaseType.dotcontrol"] = """
                        @viewModel object
                        @baseType DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolverTestsBase.TestMarkupControl1

                        {{value: Text}}
                        """,
                    ["ControlWithPropertyDirective.dotcontrol"] = """
                        @viewModel object
                        @property string Text

                        {{value: Text}}
                        """
                }
            };
            configuration = DotvvmTestHelper.CreateConfiguration(s => {
                s.AddSingleton<IMarkupFileLoader>(fakeMarkupFileLoader);
            });
            configuration.Markup.AddCodeControls("cc", typeof(ClassWithInnerElementProperty));
            configuration.Markup.AddMarkupControl("cmc", "ControlWithBaseType", "ControlWithBaseType.dotcontrol");
            configuration.Markup.AddMarkupControl("cmc", "ControlWithPropertyDirective", "ControlWithPropertyDirective.dotcontrol");
            configuration.Freeze();
        }

        protected ResolvedTreeRoot ParseSource(string markup, string fileName = "default.dothtml", bool checkErrors = false) =>
           DotvvmTestHelper.ParseResolvedTree(markup, fileName, configuration, checkErrors);

        public class TestMarkupControl1: DotvvmMarkupControl
        {
            public string Text 
            {
                get { return (string)GetValue(TextProperty); }
                set { SetValue(TextProperty, value); }
            }
            public static readonly DotvvmProperty TextProperty =
                DotvvmProperty.Register<string, TestMarkupControl1>(nameof(Text));
        }
    }
}
