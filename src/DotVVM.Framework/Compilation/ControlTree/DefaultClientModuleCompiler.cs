using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class DefaultClientModuleCompiler : IClientModuleCompiler
    {
        private readonly DotvvmConfiguration configuration;
        private readonly BindingCompilationService bindingCompilationService;
        private readonly IBindingExpressionBuilder bindingExpressionBuilder;

        private readonly ConcurrentDictionary<IControlResolverMetadata, CompiledClientModule> compiledModules = new ConcurrentDictionary<IControlResolverMetadata, CompiledClientModule>();
        private readonly ConcurrentDictionary<string, string> preparedResources = new ConcurrentDictionary<string, string>();

        public DefaultClientModuleCompiler(DotvvmConfiguration configuration, BindingCompilationService bindingCompilationService, IBindingExpressionBuilder bindingExpressionBuilder)
        {
            this.configuration = configuration;
            this.bindingCompilationService = bindingCompilationService;
            this.bindingExpressionBuilder = bindingExpressionBuilder;
        }

        public ClientModuleExtensionParameter GetClientModuleExtensionParameter(MarkupFile file, IControlResolverMetadata viewMetadata, ITypeDescriptor viewModelType, ImmutableList<NamespaceImport> namespaceImports, ImmutableList<InjectedServiceExtensionParameter> injectedServices, DothtmlElementNode clientModuleNode)
        {
            var compiledModule = GetCompiledModule(file, viewMetadata, viewModelType, namespaceImports, clientModuleNode);
            return compiledModule.BindingExtensionParameter;
        }
        
        private CompiledClientModule GetCompiledModule(MarkupFile file, IControlResolverMetadata viewMetadata, ITypeDescriptor viewModelType, ImmutableList<NamespaceImport> namespaceImports, DothtmlElementNode clientModuleNode)
        {
            return compiledModules.GetOrAdd(viewMetadata, _ => {
                var namespaceName = NamingHelper.GetNamespaceFromFileName(file.FileName, file.LastWriteDateTimeUtc, "DotvvmClientModules");
                var assemblyName = namespaceName;
                var className = NamingHelper.GetClassFromFileName(file.FileName) + "ClientModule";
                var usings = BuildUsings(viewModelType, namespaceImports);

                // emit C# code
                var emitter = new DefaultClientModuleCompilerCodeEmitter();
                var code = string.Join(string.Empty, clientModuleNode.EnumerateChildNodes().OfType<DothtmlLiteralNode>().SelectMany(n => n.Tokens).Select(t => t.Text));
                var members = GetMembers(usings, namespaceName, className, code);
                emitter.AddMembers(members);

                // build assembly
                var compilationBuilder = new CompilationBuilder(configuration.Markup, assemblyName);
                var trees = emitter.BuildTree(namespaceName, className, usings);
                compilationBuilder.AddToCompilation(trees, emitter.UsedAssemblies);
                var assembly = compilationBuilder.BuildAssembly();
                var compiledType = assembly.GetType(namespaceName + "." + className);

                var clientModuleExtensionParameter = new ClientModuleExtensionParameter(new ResolvedTypeDescriptor(compiledType));
                clientModuleExtensionParameter.RegisterJavascriptTranslations(configuration.Markup.JavascriptTranslator.MethodCollection);

                return new CompiledClientModule() {
                    Assembly = assembly,
                    Type = compiledType,
                    BindingExtensionParameter = clientModuleExtensionParameter,
                    Members = members,
                    ResourceName = "_root"
                };
            });
        }

        private List<UsingDirectiveSyntax> BuildUsings(ITypeDescriptor viewModelType, ImmutableList<NamespaceImport> namespaceImports)
        {
            return new[]
                {
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(viewModelType.Namespace))
                }.Concat(namespaceImports
                    .Select(i => {
                        var u = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(i.Namespace));
                        if (i.HasAlias)
                        {
                            u = u.WithAlias(SyntaxFactory.NameEquals(i.Alias));
                        }

                        return u;
                    })
                )
                .ToList();
        }

        private List<MemberDeclarationSyntax> GetMembers(List<UsingDirectiveSyntax> usings, string namespaceName, string className, string code)
        {
            var tree = CSharpSyntaxTree.ParseText(@$"{string.Join("\r\n", usings)}

namespace {namespaceName} {{
    public class {className} {{
        {code}
    }}
}}");
            var root = (CompilationUnitSyntax) tree.GetRoot();
            var ns = (NamespaceDeclarationSyntax) root.Members.Single();
            var c = (ClassDeclarationSyntax) ns.Members.Single();

            var members = c.Members.ToList();

            var unsupportedMembers = members.Where(m => !IsSupported(m)).ToList();
            if (unsupportedMembers.Any())
            {
                throw new DotvvmCompilationException("Client modules support only public instance methods.");
            }

            return members;
        }

        private bool IsSupported(MemberDeclarationSyntax member)
        {
            return member is MethodDeclarationSyntax method
                   && method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))
                   && !method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword))
                   && !method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        }

        public string TryGetClientModuleResourceName(ControlResolverMetadata viewMetadata)
        {
            return compiledModules.TryGetValue(viewMetadata, out var compiledModule) ? compiledModule.ResourceName : null;
        }

        public void PrepareClientModuleResource(IControlResolverMetadata viewMetadata, IDataContextStack dataContextStack)
        {
            var compiledModule = compiledModules[viewMetadata];
            preparedResources.GetOrAdd(compiledModule.ResourceName, _ => TranslateMembers(compiledModule.Type, compiledModule.Members, (DataContextStack)dataContextStack));
        }

        public string GetClientModuleResourceScript(string clientModuleName)
        {
            return preparedResources[clientModuleName];
        }

        private string TranslateMembers(Type moduleType, List<MemberDeclarationSyntax> members, DataContextStack dataContext)
        {
            var sb = new StringBuilder();
            sb.AppendLine("dotvvm.clientModules = dotvvm.clientModules || {};");
            sb.AppendLine("dotvvm.clientModules['_root'] = {");

            foreach (var member in members)
            {
                string js;
                if (member is MethodDeclarationSyntax method)
                {
                    js = TranslateMethod(moduleType, dataContext, method);
                }
                else
                {
                    throw new NotSupportedException();
                }

                sb.AppendLine($" '{method.Identifier}': {js},");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string TranslateMethod(Type moduleType, DataContextStack dataContext, MethodDeclarationSyntax method)
        {
            var compiledMethod = moduleType.GetMethod(method.Identifier.ToString());

            // compile binding expression and wrap parameters correctly
            var originalString = new OriginalStringBindingProperty(method.Body.Statements.ToString());
            var options = BindingParserOptions.Create(typeof(StaticCommandBindingExpression<>));
            var additionalSymbols = compiledMethod.GetParameters()
                .Select(p => new KeyValuePair<string, Expression>(p.Name, Expression.Parameter(p.ParameterType, p.Name)))
                .ToArray();
            var body = bindingExpressionBuilder.Parse(originalString.Code, dataContext, options, additionalSymbols);
            var expression = Expression.Lambda(body, additionalSymbols.Select(s => (ParameterExpression) s.Value).ToArray());

            // create binding
            var properties = new object[]
            {
                dataContext,
                options,
                new BindingErrorReporterProperty(),
                originalString,
                new ParsedExpressionBindingProperty(expression)
            };
            var binding = (ICommandBinding) bindingCompilationService.CreateBinding(typeof(StaticCommandBindingExpression), properties);

            var js = binding.GetProperty<KnockoutExpressionBindingProperty>();
            return js.Code.ToDefaultString();
        }
    }
}
