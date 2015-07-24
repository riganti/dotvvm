using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp.RuntimeBinder;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using DotVVM.Framework.Styles;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class DefaultViewCompiler : IViewCompiler
    {
        public DefaultViewCompiler(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
            this.controlTreeResolver = configuration.ServiceLocator.GetService<IControlTreeResolver>();
            this.assemblyCache = CompiledAssemblyCache.Instance;
        }


        private readonly CompiledAssemblyCache assemblyCache;
        private readonly IControlTreeResolver controlTreeResolver;
        private readonly DotvvmConfiguration configuration;

        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        public IControlBuilder CompileView(IReader reader, string fileName, string assemblyName, string namespaceName, string className)
        {
            // parse the document
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(reader);
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);

            var resolvedView = controlTreeResolver.ResolveTree(node, fileName);

            var styleVisitor = new StylingVisitor(configuration.Styles);
            resolvedView.Accept(styleVisitor);

            var emitter = new DefaultViewCompilerCodeEmitter();
            var compilingVisitor = new ViewCompilingVisitor(emitter, configuration.ServiceLocator.GetService<IBindingCompiler>(), className);
            
            resolvedView.Accept(compilingVisitor);

            // create the assembly
            var assembly = BuildAssembly(emitter, assemblyName, namespaceName, className);
            var controlBuilder = (IControlBuilder)assembly.CreateInstance(namespaceName + "." + className);
            resolvedView.Metadata.ControlBuilderType = controlBuilder.GetType();
            return controlBuilder;
        }


        /// <summary>
        /// Builds the assembly.
        /// </summary>
        private Assembly BuildAssembly(DefaultViewCompilerCodeEmitter emitter, string assemblyName, string namespaceName, string className)
        {
            using (var ms = new MemoryStream())
            {
                // static references
                var staticReferences = new[]
                {
                    typeof(object).Assembly,
                    typeof(RuntimeBinderException).Assembly,
                    typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly,
                    Assembly.GetExecutingAssembly()
                }
                .Concat(configuration.Markup.Assemblies.Select(Assembly.Load)).Distinct()
                .Select(MetadataReference.CreateFromAssembly);

                // add dynamic references
                var dynamicReferences = emitter.UsedControlBuilderTypes.Select(t => t.Assembly).Concat(emitter.UsedAssemblies).Distinct()
                    .Select(a => assemblyCache.GetAssemblyMetadata(a));

                // compile
                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
                var compilation = CSharpCompilation.Create(
                    assemblyName,
                    emitter.BuildTree(namespaceName, className),
                    Enumerable.Concat(staticReferences, dynamicReferences),
                    options);

                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    var assembly = Assembly.Load(ms.ToArray());
                    assemblyCache.AddAssembly(assembly, compilation.ToMetadataReference());
                    return assembly;
                }
                else
                {
                    throw new Exception("The compilation failed! This is most probably bug in the DotVVM framework.\r\n\r\n"
                        + string.Join("\r\n", result.Diagnostics)
                        + "\r\n\r\n" + compilation.SyntaxTrees[0] + "\r\n\r\n");
                }
            }
        }
    }
}