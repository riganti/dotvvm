using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    public class TestControlResolver
    {
        private DotvvmConfiguration configuration;
        private IControlTreeResolver controlTreeResolver;

        public TestControlResolver(Action<DotvvmConfiguration> configure)
        {
            configuration = DotvvmTestHelper.CreateConfiguration();
            configure(configuration);
            controlTreeResolver = configuration.ServiceProvider.GetRequiredService<IControlTreeResolver>();
        }
        public ResolvedTreeRoot ParseSource(string markup, string fileName = "default.dothtml")
        {
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(markup);

            var parser = new DothtmlParser();
            var tree = parser.Parse(tokenizer.Tokens);

            return controlTreeResolver.ResolveTree(tree, fileName)
                   .CastTo<ResolvedTreeRoot>()
                   .ApplyAction(new DataContextPropertyAssigningVisitor().VisitView)
                   .ApplyAction(new StylingVisitor(configuration).VisitView)
                   .ApplyAction(ActivatorUtilities.CreateInstance<ControlUsageValidationVisitor>(configuration.ServiceProvider).VisitView);
        }

    }
}
