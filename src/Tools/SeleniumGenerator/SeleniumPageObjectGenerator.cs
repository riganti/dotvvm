using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Tools.SeleniumGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumPageObjectGenerator
    {
        private readonly DotvvmConfiguration dotvvmConfig;
        private readonly ConcurrentDictionary<string, Task<IAbstractTreeRoot>> resolvedTreeRoots;
        private readonly ConcurrentDictionary<string, Task> updatedMarkupFiles;

        public SeleniumPageObjectGenerator(SeleniumGeneratorOptions options, DotvvmConfiguration dotvvmConfig)
        {
            this.dotvvmConfig = dotvvmConfig;
            this.resolvedTreeRoots = new ConcurrentDictionary<string, Task<IAbstractTreeRoot>>();
            updatedMarkupFiles = new ConcurrentDictionary<string, Task>();
            visitor = new SeleniumPageObjectVisitor(this);
            visitor.DiscoverControlGenerators(options);
        }

        private readonly SeleniumPageObjectVisitor visitor;

        public async Task ProcessMarkupFiles(IEnumerable<SeleniumGeneratorConfiguration> configurations)
        {
            var results = await Task.WhenAll(configurations.Select(configuration => Task.Run(async () => {
                var viewTree = await ResolveControlTree(configuration.ViewFullPath);

                // get all ui tests selectors used in current view
                var usedSelectors = GetUsedSelectors(viewTree);

                // resolve master pages of current page
                var masterPageObjectDefinitions = await ResolveMasterPages(dotvvmConfig, configuration, viewTree);

                // union used unique names across all master pages and current view
                var allUsedNames = UnionUsedUniqueNames(masterPageObjectDefinitions, usedSelectors);

                // get content of page object of current view
                var pageObject = CreatePageObjectDefinition(configuration, viewTree, allUsedNames);

                // combine all master page objects with current page object so we can generate page object class for all proxies
                pageObject = CombineViewHelperDefinitions(pageObject, masterPageObjectDefinitions);
                return (configuration, pageObject);
            })));
            var pageObjectNamespaces = results.ToDictionary(p => p.pageObject.Name, p => p.pageObject.Namespace);
            await Task.WhenAll(results.Select(result => Task.Run(async () => {
                // generate the page object class
                GeneratePageObjectClass(result.configuration, result.pageObject, pageObjectNamespaces);

                // update view markup file
                await UpdateMarkupFile(result.pageObject, result.configuration.ViewFullPath);
            })));
        }

        public async Task ProcessMarkupFile(SeleniumGeneratorConfiguration seleniumConfiguration)
        {
            var viewTree = await ResolveControlTree(seleniumConfiguration.ViewFullPath);

            // get all ui tests selectors used in current view
            var usedSelectors = GetUsedSelectors(viewTree);

            // resolve master pages of current page
            var masterPageObjectDefinitions = await ResolveMasterPages(dotvvmConfig, seleniumConfiguration, viewTree);

            // union used unique names across all master pages and current view
            var allUsedNames = UnionUsedUniqueNames(masterPageObjectDefinitions, usedSelectors);

            // get content of page object of current view
            var pageObject = CreatePageObjectDefinition(seleniumConfiguration, viewTree, allUsedNames);

            // combine all master page objects with current page object so we can generate page object class for all proxies
            pageObject = CombineViewHelperDefinitions(pageObject, masterPageObjectDefinitions);

            // generate the page object class
            GeneratePageObjectClass(seleniumConfiguration, pageObject);

            // update view markup file
            await UpdateMarkupFile(pageObject, seleniumConfiguration.ViewFullPath);
        }

        public Task<IAbstractTreeRoot> ResolveControlTree(string filePath)
        {
            return resolvedTreeRoots.GetOrAdd(filePath, ResolveControlTreeUncached);
        }

        private HashSet<string> UnionUsedUniqueNames(
            IEnumerable<MasterPageObjectDefinition> masterPageObjectDefinitions,
            IEnumerable<string> usedSelectors)
        {
            var masterPagesUsedNames = masterPageObjectDefinitions.SelectMany(m => m.UsedNames);
            return new HashSet<string>(masterPagesUsedNames.Union(usedSelectors));
        }

        private HashSet<string> GetUsedSelectors(IAbstractTreeRoot viewTree)
        {
            // traverse the tree
            var selectorFinderVisitor = new SeleniumSelectorFinderVisitor();
            selectorFinderVisitor.VisitView((ResolvedTreeRoot)viewTree);
            return selectorFinderVisitor.GetResult();
        }

        private PageObjectDefinition CombineViewHelperDefinitions(PageObjectDefinition pageObject,
            ICollection<MasterPageObjectDefinition> masterPageObjects)
        {
            if (masterPageObjects.Any())
            {
                var masterMembers = masterPageObjects.SelectMany(m => m.Members);
                var constructorExpressions = masterPageObjects.SelectMany(m => m.ConstructorStatements);

                pageObject.Members.AddRange(masterMembers);
                pageObject.ConstructorStatements.AddRange(constructorExpressions);
            }

            return pageObject;
        }

        private async Task<List<MasterPageObjectDefinition>> ResolveMasterPages(
            DotvvmConfiguration dotvvmConfiguration,
            SeleniumGeneratorConfiguration seleniumConfiguration,
            IAbstractTreeRoot viewTree)
        {
            var pageObjectDefinitions = new List<MasterPageObjectDefinition>();
            await CreateMasterPageObjectDefinitions(seleniumConfiguration, viewTree, pageObjectDefinitions);

            foreach (var pageObjectDefinition in pageObjectDefinitions)
            {
                await UpdateMarkupFile(pageObjectDefinition, pageObjectDefinition.MasterPageFullPath);
            }

            return pageObjectDefinitions;
        }

        private async Task CreateMasterPageObjectDefinitions(SeleniumGeneratorConfiguration seleniumConfiguration,
            IAbstractTreeRoot viewTree,
            ICollection<MasterPageObjectDefinition> pageObjectDefinitions)
        {
            if (IsNestedInMasterPage(viewTree))
            {
                var masterPageFile = viewTree.Directives[ParserConstants.MasterPageDirective].FirstOrDefault();
                if (masterPageFile != null)
                {
                    var masterTree = await ResolveControlTree(masterPageFile.Value);
                    var usedSelectors = GetUsedSelectors(masterTree);

                    var masterPageObjectDefinition = GetMasterPageObjectDefinition(seleniumConfiguration, masterTree, masterPageFile, usedSelectors);

                    pageObjectDefinitions.Add(masterPageObjectDefinition);

                    // recursion
                    await CreateMasterPageObjectDefinitions(seleniumConfiguration, masterTree, pageObjectDefinitions);
                }
            }
        }

        private MasterPageObjectDefinition GetMasterPageObjectDefinition(
            SeleniumGeneratorConfiguration seleniumConfiguration,
            IAbstractTreeRoot masterTree,
            IAbstractDirective masterPageFile,
            HashSet<string> usedSelectors)
        {
            var definition = CreatePageObjectDefinition(seleniumConfiguration, masterTree, usedSelectors);
            return MapPageObjectDefinition(definition, masterPageFile);
        }

        private MasterPageObjectDefinition MapPageObjectDefinition(PageObjectDefinition definition, IAbstractDirective masterPageFile)
        {
            var masterDefinition = new MasterPageObjectDefinition(masterPageFile.Value);
            masterDefinition.Members.AddRange(definition.Members);
            masterDefinition.MarkupFileModifications.AddRange(definition.MarkupFileModifications);
            masterDefinition.ConstructorStatements.AddRange(definition.ConstructorStatements);
            masterDefinition.DataContextPrefixes.AddRange(definition.DataContextPrefixes);
            masterDefinition.Children.AddRange(definition.Children);
            masterDefinition.UsedNames.UnionWith(definition.UsedNames);

            return masterDefinition;
        }

        private bool IsNestedInMasterPage(IAbstractTreeRoot view)
        {
            return view.Directives.ContainsKey(ParserConstants.MasterPageDirective);
        }

        private Task UpdateMarkupFile(PageObjectDefinition pageObject, string viewPath)
        {
            return updatedMarkupFiles.GetOrAdd(viewPath, _ => Task.Run(() => {
                var sb = new StringBuilder(File.ReadAllText(viewPath, Encoding.UTF8));
                var allModifications = GetAllModifications(pageObject);

                foreach (var modification in allModifications.OrderByDescending(m => m.Position))
                {
                    modification.Apply(sb);
                }
                File.WriteAllText(viewPath, sb.ToString(), Encoding.UTF8);
            }));
        }

        private IEnumerable<Modifications.MarkupFileModification> GetAllModifications(PageObjectDefinition pageObject)
        {
            var modifications = pageObject.MarkupFileModifications;
            foreach (var child in pageObject.Children)
            {
                modifications.AddRange(GetAllModifications(child));
            }

            return modifications;
        }

        private void GeneratePageObjectClass(SeleniumGeneratorConfiguration seleniumConfiguration, PageObjectDefinition pageObject, IDictionary<string, string> pageObjectNamespaces = null)
        {
            var usings = GetSeleniumHelpersUsingList(pageObject);
            if (pageObjectNamespaces != null)
            {
                foreach (var property in pageObject.Members.OfType<PropertyDeclarationSyntax>())
                {
                    var name = property.Type.ToString();
                    if (name == pageObject.Name)
                    {
                        continue;
                    }
                    if (pageObjectNamespaces.TryGetValue(name, out var @namespace))
                    {
                        usings = usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(@namespace)));
                    }
                }
            }

            var tree = CSharpSyntaxTree.Create(
                SyntaxFactory.CompilationUnit()
                    .WithUsings(usings)
                    .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                    {
                        SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(seleniumConfiguration.TargetNamespace))
                            .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                            {
                                GenerateHelperClassContents(pageObject)
                            }))
                    }))
                    .NormalizeWhitespace()
            );


            File.WriteAllText(seleniumConfiguration.PageObjectFileFullPath, tree.ToString());
        }

        private static SyntaxList<UsingDirectiveSyntax> GetSeleniumHelpersUsingList(PageObjectDefinition pageObject)
        {
            var list = new SyntaxList<UsingDirectiveSyntax>().AddRange(
                new[]
                {
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DotVVM.Framework.Testing.SeleniumHelpers")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DotVVM.Framework.Testing.SeleniumHelpers.Proxies")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns"))
                });
            return list;
        }

        private PageObjectDefinition CreatePageObjectDefinition(
            SeleniumGeneratorConfiguration seleniumConfiguration,
            IAbstractTreeRoot tree,
            HashSet<string> masterUsedUniqueSelectors = null)
        {
            var pageObjectDefinition = GetPageObjectDefinition(seleniumConfiguration, masterUsedUniqueSelectors);

            // traverse the tree
            visitor.PushScope(pageObjectDefinition);
            visitor.VisitView((ResolvedTreeRoot)tree);
            return visitor.PopScope();
        }

        private PageObjectDefinition GetPageObjectDefinition(SeleniumGeneratorConfiguration seleniumConfiguration,
            HashSet<string> masterUsedUniqueSelectors)
        {
            var pageObjectDefinition = new PageObjectDefinitionImpl(seleniumConfiguration.PageObjectName, seleniumConfiguration.TargetNamespace);
            if (masterUsedUniqueSelectors != null)
            {
                pageObjectDefinition.ExistingUsedSelectors.UnionWith(masterUsedUniqueSelectors);
            }

            return pageObjectDefinition;
        }

        private MemberDeclarationSyntax GenerateHelperClassContents(PageObjectDefinition pageObjectDefinition)
        {
            return SyntaxFactory
                .ClassDeclaration(pageObjectDefinition.Name)
                .WithModifiers(GetClassModifiers())
                .WithBaseList(GetBaseTypeDeclaration())
                .WithMembers(SyntaxFactory.List(pageObjectDefinition.Members))
                .AddMembers(GetConstructor(pageObjectDefinition))
                .AddMembers(pageObjectDefinition.Children.Select(GenerateHelperClassContents).ToArray());
        }

        private ConstructorDeclarationSyntax GetConstructor(PageObjectDefinition pageObjectDefinition)
        {
            return SyntaxFactory
                .ConstructorDeclaration(pageObjectDefinition.Name)
                .WithParameterList(GetConstructorMembers())
                .WithInitializer(GetBaseConstructorParameters())
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(pageObjectDefinition.ConstructorStatements));
        }

        private ParameterListSyntax GetConstructorMembers()
        {
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[]
            {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("webDriver"))
                             .WithType(SyntaxFactory.ParseTypeName("OpenQA.Selenium.IWebDriver")),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("parentHelper"))
                             .WithType(SyntaxFactory.ParseTypeName("SeleniumHelperBase"))
                             .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName("null"))),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("parentSelector"))
                             .WithType(SyntaxFactory.ParseTypeName("PathSelector"))
                             .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName("null")))
            }));
        }

        private ConstructorInitializerSyntax GetBaseConstructorParameters()
        {
            return SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                                {
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("webDriver")),
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("parentHelper")),
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("parentSelector"))
                    })));
        }

        private SyntaxTokenList GetClassModifiers()
        {
            return SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }

        private BaseListSyntax GetBaseTypeDeclaration()
        {
            return SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(new[]
            {
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("SeleniumHelperBase"))
            }));
        }

        private Task<IAbstractTreeRoot> ResolveControlTreeUncached(string filePath)
        {
            var markupLoader = dotvvmConfig.ServiceProvider.GetService<IMarkupFileLoader>();
            var markupFile = markupLoader.GetMarkup(dotvvmConfig, filePath);
            var fileContent = markupFile.ReadContent();

            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(fileContent);

            var parser = new DothtmlParser();
            var rootNode = parser.Parse(tokenizer.Tokens);

            return Task.Run(() => {

                var treeResolver = dotvvmConfig.ServiceProvider.GetService<IControlTreeResolver>();
                return treeResolver.ResolveTree(rootNode, filePath);
            });
        }
    }
}
