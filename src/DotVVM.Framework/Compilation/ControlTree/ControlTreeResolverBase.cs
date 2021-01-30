#nullable enable
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

namespace DotVVM.Framework.Compilation.ControlTree
{
    /// <summary>
    /// An abstract base class for control tree resolver.
    /// </summary>
    public abstract class ControlTreeResolverBase : IControlTreeResolver
    {
        protected readonly IControlResolver controlResolver;
        protected readonly IAbstractTreeBuilder treeBuilder;

        protected Lazy<IControlResolverMetadata> rawLiteralMetadata;
        protected Lazy<IControlResolverMetadata> literalMetadata;
        protected Lazy<IControlResolverMetadata> placeholderMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlTreeResolverBase"/> class.
        /// </summary>
        public ControlTreeResolverBase(IControlResolver controlResolver, IAbstractTreeBuilder treeBuilder)
        {
            this.controlResolver = controlResolver;
            this.treeBuilder = treeBuilder;

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
             .Concat(viewModule is null ? new BindingExtensionParameter[0] : new [] { viewModule.Value.extensionParameter }).ToArray());


            var view = treeBuilder.BuildTreeRoot(this, viewMetadata, root, dataContextTypeStack, directives, masterPage);
            view.FileName = fileName;
            treeBuilder.AddProperty(
                view,
                treeBuilder.BuildPropertyValue(Internal.ReferencedViewModuleInfoProperty, viewModule?.resource, null),
                out _
            );

            ValidateMasterPage(view, masterPage, masterPageDirective?.First());

            ResolveRootContent(root, view, viewMetadata);

            return view;
        }

        /// <summary>
        /// Resolves the content of the root node.
        /// </summary>
        protected virtual void ResolveRootContent(DothtmlRootNode root, IAbstractContentNode view, IControlResolverMetadata viewMetadata)
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

            foreach (var node in root.Content)
            {
                var child = ProcessNode(view, node, viewMetadata, view.DataContextTypeStack);
                if (child != null) treeBuilder.AddChildControl(view, child);
            }
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
            return viewmodelDirective.ResolvedType;
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
                        d.AddError($"Directive '{d.Name}' can not be present multiple times.");
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
            .Select(d => new InjectedServiceExtensionParameter(d.NameSyntax.Name, d.Type))
            .ToImmutableList();

        private (JsExtensionParameter extensionParameter, ViewModuleReferenceInfo resource)? ResolveImportedViewModules(string id, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives, bool isMarkupControl)
        {
            if (!directives.TryGetValue(ParserConstants.ViewModuleDirective, out var moduleDirectives))
                return null;

            var resources =
                moduleDirectives
                .Cast<IAbstractViewModuleDirective>()
                .Select(x => x.ImportedResourceName)
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
            if (!masterPage.DataContextType.IsAssignableFrom(viewModel))
            {
                masterPageDirective!.DothtmlNode!.AddError($"Viewmodel {viewModel.Name} is not assignable to the masterPage viewmodel {masterPage.DataContextType.Name}");
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
            literal.ConstructorParameters = new object[] { text, literalNode.Value, whitespace };
            return literal;
        }

        private IAbstractControl ProcessHtmlComment(DothtmlNode node, IDataContextStack dataContext, DotHtmlCommentNode commentNode)
        {
            var text = commentNode.IsServerSide ? "" : "<!--" + commentNode.Value + "-->";

            var literal = treeBuilder.BuildControl(rawLiteralMetadata.Value, node, dataContext);
            literal.ConstructorParameters = new object[] { text, commentNode.Value, true };
            return literal;
        }

        private IAbstractControl ProcessBindingInText(DothtmlNode node, IDataContextStack dataContext)
        {
            var bindingNode = (DothtmlBindingNode)node;
            var literal = treeBuilder.BuildControl(literalMetadata.Value, node, dataContext);

            var textBinding = ProcessBinding(bindingNode, dataContext, Literal.TextProperty);
            var textProperty = treeBuilder.BuildPropertyBinding(Literal.TextProperty, textBinding, null);
            treeBuilder.AddProperty(literal, textProperty, out _); // this can't fail

            var renderSpanElement = treeBuilder.BuildPropertyValue(Literal.RenderSpanElementProperty, false, null);
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
                controlMetadata = controlResolver.ResolveControl("", element.TagName, out constructorParameters);
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

            IAbstractPropertySetter dataContextProperty;
            if (control.TryGetProperty(DotvvmBindableObject.DataContextProperty, out dataContextProperty) && dataContextProperty is IAbstractPropertyBinding)
            {
                var dataContextBinding = ((IAbstractPropertyBinding)dataContextProperty).Binding;
                if (dataContextBinding?.ResultType != null)
                {
                    dataContext = CreateDataContextTypeStack(dataContextBinding?.ResultType, parentDataContextStack: dataContext);
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

            // check required properties
            IAbstractPropertySetter missingProperty;
            var missingProperties = control.Metadata.AllProperties.Where(p => p.MarkupOptions.Required && !control.TryGetProperty(p, out missingProperty)).ToList();
            if (missingProperties.Any())
            {
                element.AddError($"The control '{ control.Metadata.Type.FullName }' is missing required properties: { string.Join(", ", missingProperties.Select(p => "'" + p.Name + "'")) }.");
            }

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
                bindingOptions = controlResolver.ResolveBinding("value"); // just try it as with value binding
            }

            if (context?.NamespaceImports.Count > 0)
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

        protected virtual BindingParserNode ParseDirectiveTypeName(DothtmlDirectiveNode directiveNode)
        {
            var tokenizer = new BindingTokenizer();
            tokenizer.Tokenize(directiveNode.ValueNode.Text);
            var parser = new BindingParser() {
                Tokens = tokenizer.Tokens
            };
            var valueSyntaxRoot = parser.ReadDirectiveTypeName();
            if (!parser.OnEnd())
            {
                directiveNode.AddError($"Unexpected token: {parser.Peek()?.Text}.");
            }
            return valueSyntaxRoot;
        }

        protected BindingParserNode ParseImportDirectiveValue(DothtmlDirectiveNode directiveNode)
        {
            var tokenizer = new BindingTokenizer();
            tokenizer.Tokenize(directiveNode.ValueNode.Text);
            var parser = new BindingParser() {
                Tokens = tokenizer.Tokens
            };
            var result = parser.ReadDirectiveValue();

            if (!parser.OnEnd())
                directiveNode.AddError($"Unexpected token: {parser.Peek()?.Text}.");

            return result;
        }

        protected virtual IAbstractDirective ProcessImportDirective(DothtmlDirectiveNode directiveNode)
        {
            var valueSyntaxRoot = ParseImportDirectiveValue(directiveNode);

            BindingParserNode? alias = null;
            BindingParserNode? name = null;
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

        static HashSet<string> treatBindingAsHardCodedValue = new HashSet<string> { "resource" };

        private void ProcessAttributeProperties(IAbstractControl control, DothtmlAttributeNode[] nodes, IDataContextStack dataContext)
        {
            var doneAttributes = new HashSet<DothtmlAttributeNode>();
            string getName(DothtmlAttributeNode n) => n.AttributePrefix == null ? n.AttributeName : n.AttributePrefix + ":" + n.AttributeName;
            void resolveAttribute(DothtmlAttributeNode attribute)
            {
                var name = getName(attribute);
                if (!doneAttributes.Add(attribute)) return;

                var property = controlResolver.FindProperty(control.Metadata, name);
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
        if (property.IsBindingProperty || property.DataContextManipulationAttribute != null) // when DataContextManipulationAttribute is set, lets hope that author knows what is he doing.
        {
                dataContext = GetDataContextChange(dataContext, control, property);
            }

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
                    if (!treeBuilder.AddProperty(control, treeBuilder.BuildPropertyValue(property, true, attribute), out var error)) attribute.AddError(error);
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
                if (property.IsVirtual && !property.IsBindingProperty && property.PropertyType.FullName != "System.Object")
                {
                    attribute.ValueNode.AddError($"The property '{ property.FullName }' cannot contain bindings because it's not DotvvmProperty.");
                }
                else if (!treatBindingAsHardCodedValue.Contains(bindingNode.Name))
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
                    var property = controlResolver.FindProperty(control.Metadata, element.TagName);
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
            if (control.Metadata.DefaultContentProperty != null)
            {
                // don't assign the property, when content is empty
                if (content.All(c => !c.IsNotEmpty()))
                    return;

                if (control.HasProperty(control.Metadata.DefaultContentProperty))
                {
                    foreach (var c in content)
                        if (c.IsNotEmpty())
                            c.AddError($"Property { control.Metadata.DefaultContentProperty.FullName } was already set.");
                }
                else
                {
                    if (!treeBuilder.AddProperty(control, ProcessElementProperty(control, control.Metadata.DefaultContentProperty, content, null), out var error))
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
                            item.AddError($"Content not allowed inside {control.Metadata.Type.Name}.");
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
                if (child != null) treeBuilder.AddChildControl(control, child);
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
            var wrapperType = GetDefaultWrapperType(fileName);

            var baseControlDirective = !directives.ContainsKey(ParserConstants.BaseTypeDirective)
                ? null
                : (IAbstractBaseTypeDirective)directives[ParserConstants.BaseTypeDirective].SingleOrDefault();

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

            return wrapperType;
        }

        /// <summary>
        /// Gets the default type of the wrapper for the view.
        /// </summary>
        private ITypeDescriptor GetDefaultWrapperType(string fileName)
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
                return manipulationAttribute.ChangeStackForChildren(dataContext, control, property, (parent, changeType) => CreateDataContextTypeStack(changeType, parentDataContextStack: parent));
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
