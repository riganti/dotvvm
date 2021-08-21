using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;
using System.Collections.ObjectModel;
using DotVVM.Framework.Binding;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Compilation.ViewCompiler;
using DotVVM.Framework.ResourceManagement;
using System.Reflection.Emit;
using System.Reflection;

namespace DotVVM.Framework.Compilation.ControlTree
{
    /// <summary>
    /// An abstract base class for control tree resolver.
    /// </summary>
    public abstract class ControlTreeResolverBase : IControlTreeResolver
    {
        protected readonly IControlResolver controlResolver;
        protected readonly IAbstractTreeBuilder treeBuilder;
        protected readonly DotvvmResourceRepository? resourceRepo;

        protected Lazy<IControlResolverMetadata> rawLiteralMetadata;
        protected Lazy<IControlResolverMetadata> literalMetadata;
        protected Lazy<IControlResolverMetadata> placeholderMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlTreeResolverBase"/> class.
        /// </summary>
        public ControlTreeResolverBase(IControlResolver controlResolver, IAbstractTreeBuilder treeBuilder, DotvvmResourceRepository? resourceRepo)
        {
            this.controlResolver = controlResolver;
            this.treeBuilder = treeBuilder;
            this.resourceRepo = resourceRepo;
            rawLiteralMetadata = new Lazy<IControlResolverMetadata>(() => controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(RawLiteral))));
            literalMetadata = new Lazy<IControlResolverMetadata>(() => controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(Literal))));
            placeholderMetadata = new Lazy<IControlResolverMetadata>(() => controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(PlaceHolder))));
        }

        public static HashSet<string> SingleValueDirectives = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ParserConstants.BaseTypeDirective,
            ParserConstants.MasterPageDirective,
            ParserConstants.ResourceTypeDirective,
            ParserConstants.ViewModelDirectiveName
        };

        /// <summary>
        /// Resolves the control tree.
        /// </summary>
        public virtual IAbstractTreeRoot ResolveTree(DothtmlRootNode root, string fileName)
        {
            var directives = ProcessDirectives(root);
            var wrapperType = ResolveWrapperType(directives, fileName);
            var viewModelType = ResolveViewModelType(directives, root, fileName);
            var namespaceImports = ResolveNamespaceImports(directives, root);
            var injectedServices = ResolveInjectDirectives(directives);
            IAbstractControlBuilderDescriptor? masterPage = null;
            if (directives.TryGetValue(ParserConstants.MasterPageDirective, out var masterPageDirective))
            {
                masterPage = ResolveMasterPage(fileName, masterPageDirective.First());
            }
            var viewModule = ResolveImportedViewModules(AssignViewModuleId(masterPage), directives, isMarkupControl: !wrapperType.IsEqualTo(ResolvedTypeDescriptor.Create(typeof(DotvvmView))));

            // We need to call BuildControlMetadata instead of ResolveControl. The control builder for the control doesn't have to be compiled yet so the
            // metadata would be incomplete and ResolveControl caches them internally. BuildControlMetadata just builds the metadata and the control is
            // actually resolved when the control builder is ready and the metadata are complete.
            var viewMetadata = controlResolver.BuildControlMetadata(CreateControlType(wrapperType, fileName));

            var dataContextTypeStack = CreateDataContextTypeStack(viewModelType, null, namespaceImports, new BindingExtensionParameter[] {
                new CurrentMarkupControlExtensionParameter(wrapperType),
                new BindingPageInfoExtensionParameter(),
                new BindingApiExtensionParameter(),
            }.Concat(injectedServices)
             .Concat(viewModule is null ? new BindingExtensionParameter[0] : new[] { viewModule.Value.extensionParameter }).ToArray());


            var view = treeBuilder.BuildTreeRoot(this, viewMetadata, root, dataContextTypeStack, directives, masterPage);
            view.FileName = fileName;

            if (viewModule.HasValue)
            {
                treeBuilder.AddProperty(
                    view,
                    treeBuilder.BuildPropertyValue(Internal.ReferencedViewModuleInfoProperty, viewModule.Value.resource, null),
                    out _
                );
            }

            foreach (var propertyDeclarationDirective in directives.OfType<IAbstractPropertyDeclarationDirective>().ToList())
            {
                propertyDeclarationDirective.DeclaringType = wrapperType;
                CreateDotvvmPropertyFromDirective(propertyDeclarationDirective);
            }

            ValidateMasterPage(view, masterPage, masterPageDirective?.First());

            ResolveRootContent(root, view, viewMetadata);

            return view;
        }

        protected virtual void CreateDotvvmPropertyFromDirective(IAbstractPropertyDeclarationDirective propertyDeclarationDirective) => DotvvmProperty.Register(
                            propertyDeclarationDirective.NameSyntax.Name,
                            propertyDeclarationDirective.PropertyType.As<ResolvedTypeDescriptor>()?.Type,
                            propertyDeclarationDirective.DeclaringType.As<ResolvedTypeDescriptor>()?.Type,
                            propertyDeclarationDirective.InitialValue,
                            false,
                            null,
                            propertyDeclarationDirective, false);
        /// <summary>
        /// Resolves the content of the root node.
        /// </summary>
        protected virtual void ResolveRootContent(DothtmlRootNode root, IAbstractControl view, IControlResolverMetadata viewMetadata)
        {
            // WORKAROUND:
            // if there is a control in root of a MarkupControl that has DataContext assigned, it will not find the data context space, because the space of DataContext property does not include the control itself and the space of MarkupControl also does not include the MarkupControl. And because the MarkupControl is a direct parent of the DataContext-bound control there is no space in between.

            if (viewMetadata.Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmMarkupControl))))
            {
                var placeHolder = this.treeBuilder.BuildControl(
                    this.controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(PlaceHolder))),
                    view.DothtmlNode,
                    view.DataContextTypeStack
                );
                this.treeBuilder.AddChildControl(view, placeHolder);
                view = placeHolder;
                viewMetadata = placeHolder.Metadata;
            }

            ResolveControlContentImmediately(view, root.Content);
        }

        /// <summary>
        /// Resolves the view model for the root node.
        /// </summary>
        protected virtual ITypeDescriptor? ResolveViewModelType(IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives, DothtmlRootNode root, string fileName)
        {
            if (!directives.ContainsKey(ParserConstants.ViewModelDirectiveName) || directives[ParserConstants.ViewModelDirectiveName].Count == 0)
            {
                root.AddError($"The @viewModel directive is missing in the page '{fileName}'!");
                return null;
            }
            var viewmodelDirective = (IAbstractViewModelDirective)directives[ParserConstants.ViewModelDirectiveName].First();
            if (viewmodelDirective?.ResolvedType is object && viewmodelDirective.ResolvedType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmBindableObject))))
            {
                root.AddError($"The @viewModel directive cannot contain type that derives from DotvvmBindableObject!");
                return null;
            }

            return viewmodelDirective?.ResolvedType;
        }

        protected virtual IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> ProcessDirectives(DothtmlRootNode root)
        {
            var directives = new Dictionary<string, IReadOnlyList<IAbstractDirective>>(StringComparer.OrdinalIgnoreCase);

            foreach (var directiveGroup in root.Directives.GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (SingleValueDirectives.Contains(directiveGroup.Key) && directiveGroup.Count() > 1)
                {
                    foreach (var d in directiveGroup)
                    {
                        ProcessDirective(d);
                        d.AddError($"Directive '{d.Name}' cannot be present multiple times.");
                    }
                    directives[directiveGroup.Key] = ImmutableList.Create(ProcessDirective(directiveGroup.First()));
                }
                else
                {
                    directives[directiveGroup.Key] = directiveGroup.Select(ProcessDirective).ToImmutableList();
                }
            }

            return new ReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>>(directives);
        }

        protected virtual ImmutableList<InjectedServiceExtensionParameter> ResolveInjectDirectives(IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives) =>
            directives.Values.SelectMany(d => d).OfType<IAbstractServiceInjectDirective>()
            .Where(d => d.Type != null)
            .Select(d => new InjectedServiceExtensionParameter(d.NameSyntax.Name, d.Type!))
            .ToImmutableList();

        private (JsExtensionParameter extensionParameter, ViewModuleReferenceInfo resource)? ResolveImportedViewModules(string id, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives, bool isMarkupControl)
        {
            if (!directives.TryGetValue(ParserConstants.ViewModuleDirective, out var moduleDirectives))
                return null;

            var resources =
                moduleDirectives
                .Cast<IAbstractViewModuleDirective>()
                .Select(x => {
                    if (this.resourceRepo is object && x.DothtmlNode is object)
                    {
                        var resource = this.resourceRepo.FindResource(x.ImportedResourceName);
                        var node = (x.DothtmlNode as DothtmlDirectiveNode)?.ValueNode ?? x.DothtmlNode;
                        if (resource is null)
                            node.AddError($"Cannot find resource named '{x.ImportedResourceName}' referenced by the @js directive!");
                        else if (!(resource is ScriptModuleResource))
                            node.AddError($"The resource named '{x.ImportedResourceName}' referenced by the @js directive must be of the ScriptModuleResource type!");
                    }
                    return x.ImportedResourceName;
                })
                .ToArray();

            return (new JsExtensionParameter(id, isMarkupControl), new ViewModuleReferenceInfo(id, resources, isMarkupControl));
        }

        protected virtual string AssignViewModuleId(IAbstractControlBuilderDescriptor? masterPage)
        {
            var numberOfMasterPages = 0;
            while (masterPage != null)
            {
                masterPage = masterPage.MasterPage;
                numberOfMasterPages += 1;
            }
            return "p" + numberOfMasterPages;
        }

        protected abstract IAbstractControlBuilderDescriptor? ResolveMasterPage(string currentFile, IAbstractDirective masterPageDirective);

        protected virtual void ValidateMasterPage(IAbstractTreeRoot root, IAbstractControlBuilderDescriptor? masterPage, IAbstractDirective? masterPageDirective)
        {
            if (masterPage == null)
                return;
            var viewModel = root.DataContextTypeStack.DataContextType;

            if (masterPage.DataContextType is ResolvedTypeDescriptor typeDescriptor && typeDescriptor.Type == typeof(UnknownTypeSentinel))
            {
                masterPageDirective!.DothtmlNode!.AddError("Could not resolve the type of viewmodel for the specified master page. " +
                    $"This usually means that there is an error with the @viewModel directive in the master page file: \"{masterPage.FileName}\". " +
                    $"Make sure that the provided viewModel type is correct and visible for DotVVM.");
            }
            else if (!masterPage.DataContextType.IsAssignableFrom(viewModel))
            {
                masterPageDirective!.DothtmlNode!.AddError($"The viewmodel {viewModel.Name} is not assignable to the viewmodel of the master page {masterPage.DataContextType.Name}.");
            }
        }

        protected virtual ImmutableList<NamespaceImport> ResolveNamespaceImports(IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives, DothtmlRootNode root)
            => ResolveNamespaceImportsCore(directives).ToImmutableList();

        private IEnumerable<NamespaceImport> ResolveNamespaceImportsCore(IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives)
            => directives.Values.SelectMany(d => d).OfType<IAbstractImportDirective>()
            .Where(d => !d.HasError)
            .Select(d => new NamespaceImport(d.NameSyntax.ToDisplayString(), d.AliasSyntax.As<IdentifierNameBindingParserNode>()?.Name));

        /// <summary>
        /// Processes the parser node and builds a control.
        /// </summary>
        public IAbstractControl? ProcessNode(IAbstractTreeNode parent, DothtmlNode node, IControlResolverMetadata parentMetadata, IDataContextStack dataContext)
        {
            try
            {
                if (node is DothtmlBindingNode)
                {
                    // binding in text
                    return ProcessBindingInText(node, dataContext);
                }
                else if (node is DotHtmlCommentNode commentNode)
                {
                    // HTML comment
                    return ProcessHtmlComment(node, dataContext, commentNode);
                }
                else if (node is DothtmlLiteralNode literalNode)
                {
                    // text content
                    return ProcessText(node, parentMetadata, dataContext, literalNode);
                }
                else if (node is DothtmlElementNode element)
                {
                    // HTML element
                    return ProcessObjectElement(element, dataContext);
                }
                else
                {
                    throw new NotSupportedException($"The node of type '{node.GetType()}' is not supported!");
                }
            }
            catch (DotvvmCompilationException ex)
            {
                if (ex.Tokens == null)
                {
                    ex.Tokens = node.Tokens;
                    ex.ColumnNumber = node.Tokens.First().ColumnNumber;
                    ex.LineNumber = node.Tokens.First().LineNumber;
                }
                if (!LogError(ex, node))
                    throw;
                else return null;
            }
            catch (Exception ex)
            {
                if (!LogError(ex, node))
                    throw new DotvvmCompilationException("", ex, node.Tokens);
                else return null;
            }
        }

        protected virtual bool LogError(Exception exception, DothtmlNode node)
        {
            return false;
        }

        private IAbstractControl ProcessText(DothtmlNode node, IControlResolverMetadata parentMetadata, IDataContextStack dataContext, DothtmlLiteralNode literalNode)
        {
            var whitespace = string.IsNullOrWhiteSpace(literalNode.Value);

            string text;
            if (literalNode.Escape)
            {
                text = WebUtility.HtmlEncode(literalNode.Value);
            }
            else
            {
                text = literalNode.Value;
            }

            var literal = treeBuilder.BuildControl(rawLiteralMetadata.Value, node, dataContext);
            literal.ConstructorParameters = new object[] { text.Replace("\r\n", "\n"), literalNode.Value.Replace("\r\n", "\n"), BoxingUtils.Box(whitespace) };
            return literal;
        }

        private IAbstractControl ProcessHtmlComment(DothtmlNode node, IDataContextStack dataContext, DotHtmlCommentNode commentNode)
        {
            var text = commentNode.IsServerSide ? "" : "<!--" + commentNode.Value + "-->";

            var literal = treeBuilder.BuildControl(rawLiteralMetadata.Value, node, dataContext);
            literal.ConstructorParameters = new object[] { text.Replace("\r\n", "\n"), "", BoxingUtils.True };
            return literal;
        }

        private IAbstractControl ProcessBindingInText(DothtmlNode node, IDataContextStack dataContext)
        {
            var bindingNode = (DothtmlBindingNode)node;
            var literal = treeBuilder.BuildControl(literalMetadata.Value, node, dataContext);

            var textBinding = ProcessBinding(bindingNode, dataContext, Literal.TextProperty);
            var textProperty = treeBuilder.BuildPropertyBinding(Literal.TextProperty, textBinding, null);
            treeBuilder.AddProperty(literal, textProperty, out _); // this can't fail

            var renderSpanElement = treeBuilder.BuildPropertyValue(Literal.RenderSpanElementProperty, BoxingUtils.False, null);
            treeBuilder.AddProperty(literal, renderSpanElement, out _);

            return literal;
        }

        /// <summary>
        /// Processes the HTML element that represents a new object.
        /// </summary>
        private IAbstractControl ProcessObjectElement(DothtmlElementNode element, IDataContextStack dataContext)
        {
            // build control
            var controlMetadata = controlResolver.ResolveControl(element.TagPrefix, element.TagName, out var constructorParameters);
            if (controlMetadata == null)
            {
                controlMetadata = controlResolver.ResolveControl("", element.TagName, out constructorParameters).NotNull();
                constructorParameters = new[] { element.FullTagName };
                element.AddError($"The control <{element.FullTagName}> could not be resolved! Make sure that the tagPrefix is registered in DotvvmConfiguration.Markup.Controls collection!");
            }
            var control = treeBuilder.BuildControl(controlMetadata, element, dataContext);
            control.ConstructorParameters = constructorParameters;

            // resolve data context
            var dataContextAttribute = element.Attributes.FirstOrDefault(a => a.AttributeName == "DataContext");
            if (dataContextAttribute != null)
            {
                ProcessAttribute(DotvvmBindableObject.DataContextProperty, dataContextAttribute, control, dataContext);
            }

            if (control.TryGetProperty(DotvvmBindableObject.DataContextProperty, out var dataContextProperty) && dataContextProperty is IAbstractPropertyBinding { Binding: var dataContextBinding } )
            {
                if (dataContextBinding?.ResultType != null)
                {
                    dataContext = CreateDataContextTypeStack(dataContextBinding.ResultType, parentDataContextStack: dataContext);
                }
                else
                {
                    dataContext = CreateDataContextTypeStack(null, dataContext);
                }
                control.DataContextTypeStack = dataContext;
            }
            if (controlMetadata.DataContextConstraint != null && dataContext != null && !controlMetadata.DataContextConstraint.IsAssignableFrom(dataContext.DataContextType))
            {
                ((DothtmlNode?)dataContextAttribute ?? element)
                   .AddError($"The control '{controlMetadata.Type.Name}' requires a DataContext of type '{controlMetadata.DataContextConstraint.FullName}'!");
            }

            ProcessAttributeProperties(control, element.Attributes.Where(a => a.AttributeName != "DataContext").ToArray(), dataContext!);

            // process control contents
            ProcessControlContent(control, element.Content);

            var unknownContent = control.Content.Where(c => !c.Metadata.Type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmControl))));
            foreach (var unknownControl in unknownContent)
            {
                unknownControl.DothtmlNode!.AddError($"The control '{ unknownControl.Metadata.Type.FullName }' does not inherit from DotvvmControl and thus cannot be used in content.");
            }

            return control;
        }

        /// <summary>
        /// Processes the binding node.
        /// </summary>
        public IAbstractBinding ProcessBinding(DothtmlBindingNode node, IDataContextStack? context, IPropertyDescriptor property)
        {
            var bindingOptions = controlResolver.ResolveBinding(node.Name);
            if (bindingOptions == null)
            {
                node.NameNode.AddError($"Binding {node.Name} could not be resolved.");
                bindingOptions = controlResolver.ResolveBinding("value").NotNull(); // just try it as with value binding
            }

            if (context?.NamespaceImports.Length > 0)
                bindingOptions = bindingOptions.AddImports(context.NamespaceImports);

            return CompileBinding(node, bindingOptions, context!, property);
        }

        protected virtual IAbstractDirective ProcessDirective(DothtmlDirectiveNode directiveNode)
        {
            if (string.Equals(ParserConstants.ImportNamespaceDirective, directiveNode.Name) || string.Equals(ParserConstants.ResourceNamespaceDirective, directiveNode.Name))
            {
                return ProcessImportDirective(directiveNode);
            }
            else if (string.Equals(ParserConstants.ViewModelDirectiveName, directiveNode.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ProcessViewModelDirective(directiveNode);
            }
            else if (string.Equals(ParserConstants.BaseTypeDirective, directiveNode.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ProcessBaseTypeDirective(directiveNode);
            }
            else if (string.Equals(ParserConstants.ServiceInjectDirective, directiveNode.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ProcessServiceInjectDirective(directiveNode);
            }
            else if (string.Equals(ParserConstants.ViewModuleDirective, directiveNode.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ProcessViewModuleDirective(directiveNode);
            }
            else if (string.Equals(ParserConstants.PropertyDeclarationDirective, directiveNode.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ProcessPropertyDeclarationDirective(directiveNode);
            }

            return treeBuilder.BuildDirective(directiveNode);
        }

        protected virtual IAbstractDirective ProcessViewModelDirective(DothtmlDirectiveNode directiveNode)
        {
            return this.treeBuilder.BuildViewModelDirective(directiveNode, ParseDirectiveTypeName(directiveNode));
        }

        protected virtual IAbstractDirective ProcessBaseTypeDirective(DothtmlDirectiveNode directiveNode)
        {
            return this.treeBuilder.BuildBaseTypeDirective(directiveNode, ParseDirectiveTypeName(directiveNode));
        }

        protected virtual BindingParserNode ParseDirectiveTypeName(DothtmlDirectiveNode directiveNode) => ParseDirectiveCore(directiveNode, p => p.ReadDirectiveTypeName());
        protected virtual BindingParserNode ParseImportDirectiveValue(DothtmlDirectiveNode directiveNode) => ParseDirectiveCore(directiveNode, p => p.ReadImportDirectiveValue());
        protected virtual BindingParserNode ParsePropertyDirectiveValue(DothtmlDirectiveNode directiveNode) => ParseDirectiveCore(directiveNode, p => p.ReadPropertyDirectiveValue());

        private static BindingParserNode ParseDirectiveCore(DothtmlDirectiveNode directiveNode, Func<BindingParser, BindingParserNode> parserFunc)
        {
            var tokenizer = new BindingTokenizer();
            tokenizer.Tokenize(directiveNode.ValueNode.Text);
            var parser = new BindingParser() {
                Tokens = tokenizer.Tokens
            };
            var valueSyntaxRoot = parserFunc(parser);
            if (!parser.OnEnd())
            {
                directiveNode.AddError($"Unexpected token: {parser.Peek()?.Text}.");
            }
            return valueSyntaxRoot;
        }

        protected virtual IAbstractDirective ProcessImportDirective(DothtmlDirectiveNode directiveNode)
        {
            var valueSyntaxRoot = ParseImportDirectiveValue(directiveNode);

            BindingParserNode? alias = null;
            BindingParserNode? name;
            if (valueSyntaxRoot is BinaryOperatorBindingParserNode assignment)
            {
                alias = assignment.FirstExpression;
                name = assignment.SecondExpression;
            }
            else
            {
                name = valueSyntaxRoot;
            }

            return treeBuilder.BuildImportDirective(directiveNode, alias, name);
        }

        protected virtual IAbstractDirective ProcessServiceInjectDirective(DothtmlDirectiveNode directiveNode)
        {
            var valueSyntaxRoot = ParseImportDirectiveValue(directiveNode);

            if (valueSyntaxRoot is BinaryOperatorBindingParserNode assignment)
            {
                var name = assignment.FirstExpression as SimpleNameBindingParserNode;
                if (name == null)
                {
                    directiveNode.AddError($"Identifier expected on the left side of the assignment.");
                    name = new SimpleNameBindingParserNode("service");
                }
                var type = assignment.SecondExpression;
                return treeBuilder.BuildServiceInjectDirective(directiveNode, name, type);
            }
            else
            {
                directiveNode.AddError($"Assignment operation expected - the correct form is `@{ParserConstants.ServiceInjectDirective} myStringService = ISomeService<string>`");
                return treeBuilder.BuildServiceInjectDirective(directiveNode, new SimpleNameBindingParserNode("service"), valueSyntaxRoot);
            }
        }

        protected virtual IAbstractDirective ProcessViewModuleDirective(DothtmlDirectiveNode directiveNode)
        {
            return treeBuilder.BuildViewModuleDirective(directiveNode, modulePath: directiveNode.Value, resourceName: directiveNode.Value);
        }

        protected virtual IAbstractDirective ProcessPropertyDeclarationDirective(DothtmlDirectiveNode directiveNode)
        {
            var valueSyntaxRoot = ParsePropertyDirectiveValue(directiveNode);

            var declaration = valueSyntaxRoot as PropertyDeclarationBindingParserNode;
            if(declaration == null)
            {
                directiveNode.AddError("Cannot resolve the property declaration.");
            }

            var type = declaration?.PropertyType as TypeReferenceBindingParserNode;
            if (type == null)
            {
                directiveNode.AddError($"Property type expected");
                type = new ActualTypeReferenceBindingParserNode(new SimpleNameBindingParserNode("string"));
            }

            var name = declaration?.Name as SimpleNameBindingParserNode;
            if (name == null)
            {
                directiveNode.AddError($"Property name expected.");
                name = new SimpleNameBindingParserNode("");
            }

            var initializer = declaration?.Initializer as LiteralExpressionBindingParserNode;
            if (declaration?.Initializer != null && initializer != null)
            {
                initializer = new LiteralExpressionBindingParserNode(null);
                directiveNode.AddError("Property initializer must be a constant.");
            }

            var attributeSyntaxes = (declaration?.Attributes ?? new List<BindingParserNode>());
            var resolvedAttributes = ProcessPropertyDirectiveAttributeReference(directiveNode, attributeSyntaxes)
                .Select(a => treeBuilder.BuildPropertyDeclarationAttributeReferenceDirective(directiveNode, a.name, a.type, a.initializer))
                .ToList();

            return treeBuilder.BuildPropertyDeclarationDirective(directiveNode, type, name, initializer, resolvedAttributes, valueSyntaxRoot);
        }

        private List<(ActualTypeReferenceBindingParserNode type, IdentifierNameBindingParserNode name, LiteralExpressionBindingParserNode initializer)> ProcessPropertyDirectiveAttributeReference(DothtmlDirectiveNode directiveNode, List<BindingParserNode> attributeReferences)
        {
            var result = new List<(ActualTypeReferenceBindingParserNode, IdentifierNameBindingParserNode, LiteralExpressionBindingParserNode)> ();
            foreach (var attributeReference in attributeReferences)
            {
                if (!(attributeReference is BinaryOperatorBindingParserNode assigment && assigment.Operator == BindingTokenType.AssignOperator))
                {
                    directiveNode.AddError("Property attributes must be in the form Attribute.Property = value.");
                    continue;
                }

                var attributePropertyReference = assigment.FirstExpression as MemberAccessBindingParserNode;
                var attributeTypeReference = attributePropertyReference?.TargetExpression;
                var attributePropertyNameReference = attributePropertyReference?.MemberNameExpression;
                var initializer = assigment.SecondExpression as LiteralExpressionBindingParserNode;

                if (attributeTypeReference == null || attributePropertyNameReference == null)
                {
                    directiveNode.AddError("Property attributes must be in the form Attribute.Property = value.");
                    continue;
                }
                if (initializer == null)
                {
                    directiveNode.AddError($"Value for property {attributeTypeReference.ToDisplayString()} of attribute {attributePropertyNameReference.ToDisplayString()} is missing or not a constant.");
                    continue;
                }
                result.Add((new ActualTypeReferenceBindingParserNode(attributeTypeReference), attributePropertyNameReference, initializer));
            }
            return result;
        }


        static HashSet<string> treatBindingAsHardCodedValue = new HashSet<string> { "resource" };

        private void ProcessAttributeProperties(IAbstractControl control, DothtmlAttributeNode[] nodes, IDataContextStack dataContext)
        {
            var doneAttributes = new HashSet<DothtmlAttributeNode>();
            string getName(DothtmlAttributeNode n) => n.AttributePrefix == null ? n.AttributeName : n.AttributePrefix + ":" + n.AttributeName;
            void resolveAttribute(DothtmlAttributeNode attribute)
            {
                var name = getName(attribute);
                if (!doneAttributes.Add(attribute)) return;

                var property = controlResolver.FindProperty(control.Metadata, name, MappingMode.Attribute);
                if (property == null)
                {
                    attribute.AddError($"The control '{control.Metadata.Type}' does not have a property '{attribute.AttributeName}' and does not allow HTML attributes!");
                }
                else
                {
                    var dependsOn = property.DataContextChangeAttributes.SelectMany(c => c.PropertyDependsOn);
                    foreach (var p in dependsOn.SelectMany(t => nodes.Where(n => t == getName(n))))
                        resolveAttribute(p);
                    ProcessAttribute(property, attribute, control, dataContext);
                }
            }
            // set properties from attributes
            foreach (var attr in nodes)
            {
                resolveAttribute(attr);
            }
        }

        /// <summary>
        /// Processes the attribute node.
        /// </summary>
        private void ProcessAttribute(IPropertyDescriptor property, DothtmlAttributeNode attribute, IAbstractControl control, IDataContextStack dataContext)
        {
            dataContext = GetDataContextChange(dataContext, control, property);

            if (!property.MarkupOptions.MappingMode.HasFlag(MappingMode.Attribute))
            {
                attribute.AddError($"The property '{property.FullName}' cannot be used as a control attribute!");
                return;
            }

            // set the property
            if (attribute.ValueNode == null)
            {
                // implicitly set boolean property
                if (property.PropertyType.IsEqualTo(new ResolvedTypeDescriptor(typeof(bool))) || property.PropertyType.IsEqualTo(new ResolvedTypeDescriptor(typeof(bool?))))
                {
                    if (!treeBuilder.AddProperty(control, treeBuilder.BuildPropertyValue(property, BoxingUtils.True, attribute), out var error)) attribute.AddError(error);
                }
                else if (property.MarkupOptions.AllowAttributeWithoutValue)
                {
                    if (!treeBuilder.AddProperty(control, treeBuilder.BuildPropertyValue(property, (property as DotVVM.Framework.Binding.DotvvmProperty)?.DefaultValue, attribute), out var error)) attribute.AddError(error);
                }
                else attribute.AddError($"The attribute '{property.Name}' on the control '{control.Metadata.Type.FullName}' must have a value!");
            }
            else if (attribute.ValueNode is DothtmlValueBindingNode valueBindingNode)
            {
                // binding
                var bindingNode = valueBindingNode.BindingNode;
                if (!treatBindingAsHardCodedValue.Contains(bindingNode.Name))
                {
                    if (!property.MarkupOptions.AllowBinding)
                        attribute.ValueNode.AddError($"The property '{ property.FullName }' cannot contain {bindingNode.Name} binding.");
                }
                var binding = ProcessBinding(bindingNode, dataContext, property);
                var bindingProperty = treeBuilder.BuildPropertyBinding(property, binding, attribute);
                if (!treeBuilder.AddProperty(control, bindingProperty, out var error)) attribute.AddError(error);
            }
            else
            {
                // hard-coded value in markup
                if (!property.MarkupOptions.AllowHardCodedValue)
                {
                    attribute.ValueNode.AddError($"The property '{ property.FullName }' cannot contain hard coded value.");
                }

                var textValue = (DothtmlValueTextNode)attribute.ValueNode;
                var value = ConvertValue(WebUtility.HtmlDecode(textValue.Text), property.PropertyType);
                var propertyValue = treeBuilder.BuildPropertyValue(property, value, attribute);
                if (!treeBuilder.AddProperty(control, propertyValue, out var error)) attribute.AddError(error);
            }
        }

        /// <summary>
        /// Processes the content of the control node.
        /// </summary>
        public void ProcessControlContent(IAbstractControl control, IEnumerable<DothtmlNode> nodes)
        {
            var content = new List<DothtmlNode>();
            bool properties = true;
            foreach (var node in nodes)
            {
                var element = node as DothtmlElementNode;
                if (element != null && properties)
                {
                    var property = controlResolver.FindProperty(control.Metadata, element.TagName, MappingMode.InnerElement);
                    if (property != null && string.IsNullOrEmpty(element.TagPrefix) && property.MarkupOptions.MappingMode.HasFlag(MappingMode.InnerElement))
                    {
                        content.Clear();
                        if (!treeBuilder.AddProperty(control, ProcessElementProperty(control, property, element.Content, element), out var error)) element.AddError(error);

                        foreach (var attr in element.Attributes)
                            attr.AddError("Attributes can't be set on element property.");
                    }
                    else
                    {
                        content.Add(node);
                        if (node.IsNotEmpty())
                        {
                            properties = false;
                        }
                    }
                }
                else
                {
                    content.Add(node);
                }
            }
            if (control.Metadata.DefaultContentProperty is IPropertyDescriptor contentProperty)
            {
                // don't assign the property, when content is empty
                if (content.All(c => !c.IsNotEmpty()))
                    return;

                if (control.HasProperty(contentProperty))
                {
                    foreach (var c in content)
                        if (c.IsNotEmpty())
                            c.AddError($"Property { contentProperty.FullName } was already set.");
                }
                else
                {
                    if (!treeBuilder.AddProperty(control, ProcessElementProperty(control, contentProperty, content, null), out var error))
                        content.First().AddError(error);
                }
            }
            else
            {
                if (!control.Metadata.IsContentAllowed)
                {
                    foreach (var item in content)
                    {
                        if (item.IsNotEmpty())
                        {
                            var compositeControlHelp =
                                control.Metadata.Type.IsAssignableTo(new ResolvedTypeDescriptor
                            (typeof(CompositeControl))) ?
                                " CompositeControls don't allow content by default and Content or ContentTemplate property is missing on this control." : "";
                            item.AddError($"Content not allowed inside {control.Metadata.Type.Name}.{compositeControlHelp}");
                        }
                    }
                }
                else ResolveControlContentImmediately(control, content);
            }
        }

        /// <summary>
        /// Resolves the content of the control immediately during the parsing.
        /// Override this method if you need lazy loading of tree node contents.
        /// </summary>
        protected virtual void ResolveControlContentImmediately(IAbstractControl control, List<DothtmlNode> content)
        {
            var dataContext = GetDataContextChange(control.DataContextTypeStack, control, null);
            foreach (var node in content)
            {
                var child = ProcessNode(control, node, control.Metadata, dataContext);
                if (child != null)
                {
                    treeBuilder.AddChildControl(control, child);
                    if (!child.Metadata.Type.IsAssignableTo(ResolvedTypeDescriptor.Create(typeof(DotvvmControl))))
                        node.AddError($"Content control must inherit from DotvvmControl, but {child.Metadata.Type} doesn't.");
                }
            }
        }

        /// <summary>
        /// Processes the element which contains property value.
        /// </summary>
        private IAbstractPropertySetter ProcessElementProperty(IAbstractControl control, IPropertyDescriptor property, IEnumerable<DothtmlNode> elementContent, DothtmlElementNode? propertyWrapperElement)
        {
            IEnumerable<IAbstractControl> filterByType(ITypeDescriptor type, IEnumerable<IAbstractControl> controls) =>
                FilterOrError(controls,
                        c => c is object && c.Metadata.Type.IsAssignableTo(type),
                        c => {
                            // empty nodes are only filtered, non-empty nodes cause errors
                            if (c.DothtmlNode.IsNotEmpty())
                                c.DothtmlNode.AddError($"Control type {c.Metadata.Type.FullName} can't be used in collection of type {type.FullName}.");
                        });

            // resolve data context
            var dataContext = control.DataContextTypeStack;
            dataContext = GetDataContextChange(dataContext, control, property);

            // the element is a property
            if (IsTemplateProperty(property))
            {
                // template
                return treeBuilder.BuildPropertyTemplate(property, ProcessTemplate(control, elementContent, dataContext), propertyWrapperElement);
            }
            else if (IsCollectionProperty(property))
            {
                var collectionType = GetCollectionType(property);
                // collection of elements
                var collection = elementContent.Select(childObject => ProcessNode(control, childObject, control.Metadata, dataContext)!);
                if (collectionType != null)
                {
                    collection = filterByType(collectionType, collection);
                }

                return treeBuilder.BuildPropertyControlCollection(property, collection.ToArray(), propertyWrapperElement);
            }
            else if (property.PropertyType.IsEqualTo(new ResolvedTypeDescriptor(typeof(string))))
            {
                // string property
                var strings = FilterNodes<DothtmlLiteralNode>(elementContent, property);
                var value = string.Concat(strings.Select(s => s.Value));
                return treeBuilder.BuildPropertyValue(property, value, propertyWrapperElement);
            }
            else if (IsControlProperty(property))
            {
                var children = filterByType(property.PropertyType, elementContent.Select(childObject => ProcessNode(control, childObject, control.Metadata, dataContext)!)).ToArray();
                if (children.Length > 1)
                {
                    // try with the empty nodes are excluded
                    children = children.Where(c => c.DothtmlNode.IsNotEmpty()).ToArray();
                    if (children.Length > 1)
                    {
                        foreach (var c in children.Skip(1))
                            c.DothtmlNode!.AddError($"The property '{property.MarkupOptions.Name}' can have only one child element!");
                    }
                }
                if (children.Length >= 1)
                {
                    return treeBuilder.BuildPropertyControl(property, children[0], propertyWrapperElement);
                }
                else
                {
                    return treeBuilder.BuildPropertyControl(property, null, propertyWrapperElement);
                }
            }
            else
            {
                control.DothtmlNode!.AddError($"The property '{property.FullName}' is not supported!");
                return treeBuilder.BuildPropertyValue(property, null, propertyWrapperElement);
            }
        }

        /// <summary>
        /// Processes the template contents.
        /// </summary>
        private List<IAbstractControl> ProcessTemplate(IAbstractTreeNode parent, IEnumerable<DothtmlNode> elementContent, IDataContextStack dataContext)
        {
            var content = elementContent.Select(e => ProcessNode(parent, e, placeholderMetadata.Value, dataContext)!).Where(e => e != null);
            return content.ToList();
        }

        protected IEnumerable<T> FilterOrError<T>(IEnumerable<T> controls, Predicate<T> predicate, Action<T> errorAction)
        {
            foreach (var item in controls)
            {
                if (predicate(item)) yield return item;
                else errorAction(item);
            }
        }

        /// <summary>
        /// Gets the inner property elements and makes sure that no other content is present.
        /// </summary>
        private IEnumerable<TNode> FilterNodes<TNode>(IEnumerable<DothtmlNode> nodes, IPropertyDescriptor property) where TNode : DothtmlNode
        {
            foreach (var child in nodes)
            {
                if (child is TNode)
                {
                    yield return (TNode)child;
                }
                else if (child.IsNotEmpty())
                {
                    child.AddError($"Content is not allowed inside the property '{property.FullName}'! (Conflicting node: Node {child.GetType().Name})");
                }
            }
        }

        /// <summary>
        /// Resolves the type of the wrapper.
        /// </summary>
        private ITypeDescriptor ResolveWrapperType(IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives, string fileName)
        {
            var wrapperType = GetDefaultWrapperType(fileName , directives);

            var baseControlDirective = !directives.ContainsKey(ParserConstants.BaseTypeDirective)
                ? null
                : (IAbstractBaseTypeDirective?)directives[ParserConstants.BaseTypeDirective].SingleOrDefault();

            if (baseControlDirective != null)
            {
                var baseType = baseControlDirective.ResolvedType;
                if (baseType == null)
                {
                    baseControlDirective.DothtmlNode!.AddError($"The type '{baseControlDirective.Value}' specified in baseType directive was not found!");
                }
                else if (!baseType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmMarkupControl))))
                {
                    baseControlDirective.DothtmlNode!.AddError("Markup controls must derive from DotvvmMarkupControl class!");
                    wrapperType = baseType;
                }
                else
                {
                    wrapperType = baseType;
                }
            }

            if(directives.TryGetValue(ParserConstants.PropertyDeclarationDirective, out var abstractDirectives))
            {
                wrapperType = CreateDymanicDeclaringType(wrapperType);
            }

            return wrapperType;
        }

        protected virtual ITypeDescriptor CreateDymanicDeclaringType(ITypeDescriptor? originalWrapperType)
        {

            var baseType = originalWrapperType?.CastTo<ResolvedTypeDescriptor>().Type ?? typeof(DotvvmMarkupControl);
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            var assemblyName = new AssemblyName($"DotvvmMarkupControlDynamicAssembly-{Guid.NewGuid()}");
            var assemblyBuilder =
                AssemblyBuilder.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.Run);

            // For a single-module assembly, the module name is usually
            // the assembly name plus an extension.
            var mb =
                assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var declaringTypeBuilder = mb.DefineType(
                $"DotvvmMarkupControl-{Guid.NewGuid()}",
                 TypeAttributes.Public, baseType);
            var declaringTypeDecriptor = new ResolvedTypeDescriptor(declaringTypeBuilder.CreateTypeInfo().AsType());
            return declaringTypeDecriptor;
        }

        /// <summary>
        /// Gets the default type of the wrapper for the view.
        /// </summary>
        private ITypeDescriptor GetDefaultWrapperType(string fileName, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives)
        {
            ITypeDescriptor wrapperType;
            if (fileName.EndsWith(".dotcontrol", StringComparison.Ordinal))
            {
                wrapperType = new ResolvedTypeDescriptor(typeof(DotvvmMarkupControl));
            }
            else
            {
                wrapperType = new ResolvedTypeDescriptor(typeof(DotvvmView));
            }
            return wrapperType;
        }

        protected virtual bool IsCollectionProperty(IPropertyDescriptor property)
        {
            return property.PropertyType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(ICollection)));
        }

        protected virtual ITypeDescriptor? GetCollectionType(IPropertyDescriptor property)
        {
            return property.PropertyType.TryGetArrayElementOrIEnumerableType();
        }


        protected virtual bool IsTemplateProperty(IPropertyDescriptor property)
        {
            return property.PropertyType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(ITemplate)));
        }

        protected virtual bool IsControlProperty(IPropertyDescriptor property)
        {
            return property.PropertyType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmControl)));
        }


        /// <summary>
        /// Gets the data context change behavior for the specified control property.
        /// </summary>
        [return: NotNullIfNotNull("dataContext")]
        protected virtual IDataContextStack? GetDataContextChange(IDataContextStack dataContext, IAbstractControl control, IPropertyDescriptor? property)
        {
            if (dataContext == null) return null;

            var manipulationAttribute = property != null ? property.DataContextManipulationAttribute : control.Metadata.DataContextManipulationAttribute;
            if (manipulationAttribute != null)
            {
                return manipulationAttribute.ChangeStackForChildren(dataContext, control, property!, (parent, changeType) => CreateDataContextTypeStack(changeType, parentDataContextStack: parent));
            }

            var attributes = property != null ? property.DataContextChangeAttributes : control.Metadata.DataContextChangeAttributes;
            if (attributes == null || attributes.Length == 0) return dataContext;

            try
            {
                var (type, extensionParameters) = ApplyContextChange(dataContext, attributes, control, property);

                if (type == null) return dataContext;
                else return CreateDataContextTypeStack(type, parentDataContextStack: dataContext, extensionParameters: extensionParameters.ToArray());
            }
            catch (Exception exception)
            {
                var node = property != null && control.TryGetProperty(property, out var v) ? v.DothtmlNode : control.DothtmlNode;
                node?.AddError($"Could not compute the type of DataContext: {exception}");

                return CreateDataContextTypeStack(null, parentDataContextStack: dataContext);
            }
        }

        public static (ITypeDescriptor? type, List<BindingExtensionParameter> extensionParameters) ApplyContextChange(IDataContextStack dataContext, DataContextChangeAttribute[] attributes, IAbstractControl control, IPropertyDescriptor? property)
        {
            var type = dataContext.DataContextType;
            var extensionParameters = new List<BindingExtensionParameter>();
            foreach (var attribute in attributes.OrderBy(a => a.Order))
            {
                if (type == null) break;
                extensionParameters.AddRange(attribute.GetExtensionParameters(type));
                type = attribute.GetChildDataContextType(type, dataContext, control, property);
            }
            return (type, extensionParameters);
        }


        /// <summary>
        /// Creates the IControlType identification of the control.
        /// </summary>
        protected abstract IControlType CreateControlType(ITypeDescriptor wrapperType, string virtualPath);

        /// <summary>
        /// Creates the data context type stack object.
        /// </summary>
        protected abstract IDataContextStack CreateDataContextTypeStack(ITypeDescriptor? viewModelType, IDataContextStack? parentDataContextStack = null, IReadOnlyList<NamespaceImport>? imports = null, IReadOnlyList<BindingExtensionParameter>? extensionParameters = null);

        /// <summary>
        /// Converts the value to the property type.
        /// </summary>
        protected abstract object? ConvertValue(string value, ITypeDescriptor propertyType);

        /// <summary>
        /// Compiles the binding.
        /// </summary>
        protected abstract IAbstractBinding CompileBinding(DothtmlBindingNode node, BindingParserOptions bindingOptions, IDataContextStack context, IPropertyDescriptor property);

    }
}
