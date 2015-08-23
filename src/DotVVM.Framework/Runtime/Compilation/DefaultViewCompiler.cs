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
        public virtual CSharpCompilation CompileView(IReader reader, string fileName, CSharpCompilation compilation, string namespaceName, string className)
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

            return AddToCompilation(compilation, emitter, namespaceName, className);
        }

        protected virtual CSharpCompilation AddToCompilation(CSharpCompilation compilation, DefaultViewCompilerCodeEmitter emitter, string namespaceName, string className)
        {
            return compilation
                .AddSyntaxTrees(emitter.BuildTree(namespaceName, className))
                .AddReferences(emitter.UsedAssemblies
                    .Select(a => assemblyCache.GetAssemblyMetadata(a)));
        }

        public virtual CSharpCompilation CreateCompilation(string assemblyName)
        {
            return CSharpCompilation.Create(assemblyName).AddReferences(new[]
                {
                    typeof(object).Assembly,
                    typeof(RuntimeBinderException).Assembly,
                    typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly,
                    Assembly.GetExecutingAssembly()
                }.Concat(configuration.Markup.Assemblies.Select(Assembly.Load)).Distinct()
                .Select(a => assemblyCache.GetAssemblyMetadata(a)))
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        protected virtual IControlBuilder GetControlBuilder(Assembly assembly, string namespaceName, string className)
        {
            return (IControlBuilder)assembly.CreateInstance(namespaceName + "." + className);
        }

        /// <summary>
        /// Builds the assembly.
        /// </summary>
        protected virtual Assembly BuildAssembly(CSharpCompilation compilation)
        {
            using (var ms = new MemoryStream())
            {
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

        public virtual IControlBuilder CompileView(IReader reader, string fileName, string assemblyName, string namespaceName, string className)
        {
            var compilation = CreateCompilation(assemblyName);
            compilation = CompileView(reader, fileName, compilation, namespaceName, className);
            var assembly = BuildAssembly(compilation);
            return GetControlBuilder(assembly, namespaceName, className);
        }
    }
}