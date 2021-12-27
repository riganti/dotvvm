using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    public abstract class DefaultControlTreeResolverTestsBase
    {
        protected static DotvvmConfiguration configuration { get; }

        static DefaultControlTreeResolverTestsBase()
        {
            configuration = DotvvmTestHelper.CreateConfiguration();
            configuration.Markup.AddCodeControls("cc", typeof(ClassWithInnerElementProperty));
            configuration.Freeze();
        }

        protected ResolvedTreeRoot ParseSource(string markup, string fileName = "default.dothtml", bool checkErrors = false) =>
           DotvvmTestHelper.ParseResolvedTree(markup, fileName, configuration, checkErrors);

    }
}
