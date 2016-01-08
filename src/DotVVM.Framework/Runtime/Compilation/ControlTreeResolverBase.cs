using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser;
using System.Collections;
using DotVVM.Framework.Exceptions;
using System.Net;
using DotVVM.Framework.Runtime.ControlTree;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Runtime.Compilation
{
    /// <summary>
    /// An abstract base class for control tree resolver.
    /// </summary>
    public abstract class ControlTreeResolverBase : IControlTreeResolver
    {
        protected readonly IControlResolver controlResolver;
        protected readonly IAbstractTreeBuilder treeBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlTreeResolverBase"/> class.
        /// </summary>
        public ControlTreeResolverBase(IControlResolver controlResolver, IAbstractTreeBuilder treeBuilder)
        {
            this.controlResolver = controlResolver;
            this.treeBuilder = treeBuilder;
        }

        /// <summary>
        /// Resolves the control tree.
        /// </summary>
        public IAbstractTreeRoot ResolveTree(DothtmlRootNode root, string fileName)
        {
            var wrapperType = ResolveWrapperType(root, fileName);

            // We need to call BuildControlMetadata instead of ResolveControl. The control builder for the control doesn't have to be compiled yet so the 
            // metadata would be incomplete and ResolveControl caches them internally. BuildControlMetadata just builds the metadata and the control is
            // actually resolved when the control builder is ready and the metadata are complete.
            var viewMetadata = controlResolver.BuildControlMetadata(CreateControlType(wrapperType, fileName));
            
            // build the root node
            var view = treeBuilder.BuildRoot(this, viewMetadata, root);
            view.DataContextTypeStack = ResolveViewModel(fileName, view, wrapperType);
            ResolveRootContent(root, view, viewMetadata);

            return view;
        }

        /// <summary>
        /// Resolves the content of the root node.
        /// </summary>
        private void ResolveRootContent(DothtmlRootNode root, IAbstractTreeRoot view, IControlResolverMetadata viewMetadata)
        {
            foreach (var node in root.Content)
            {
                treeBuilder.AddChildControl(view, ProcessNode(node, viewMetadata, view.DataContextTypeStack));
            }
        }

        /// <summary>
        /// Resolves the view model for the root node.
        /// </summary>
        protected virtual IDataContextStack ResolveViewModel(string fileName, IAbstractTreeRoot view, ITypeDescriptor wrapperType)
        {
            if (!view.Directives.ContainsKey(Constants.ViewModelDirectiveName))
            {
                throw new DotvvmCompilationException($"The @viewModel directive is missing in the page '{fileName}'!", view.DothtmlNode.Tokens.Take(1));
            }

            var viewModelTypeName = view.Directives[Constants.ViewModelDirectiveName];
            var viewModelType = FindType(viewModelTypeName);
            if (viewModelType == null)
            {
                throw new DotvvmCompilationException($"The type '{viewModelTypeName}' required in the @viewModel directive in was not found!",
                    (view.DothtmlNode as DothtmlRootNode)?.Directives?.FirstOrDefault(d => string.Equals(d.Name, Constants.ViewModelDirectiveName, StringComparison.InvariantCultureIgnoreCase))?.Tokens);
            }
            return CreateDataContextTypeStack(viewModelType, wrapperType);
        }

        /// <summary>
        /// Processes the parser node and builds a control.
        /// </summary>
        public IAbstractControl ProcessNode(DothtmlNode node, IControlResolverMetadata parentMetadata, IDataContextStack dataContext)
        {
            try
            {
                if (node is DothtmlBindingNode)
                {
                    EnsureContentAllowed(parentMetadata);

                    // binding in text
                    var binding = (DothtmlBindingNode)node;
                    var literal = treeBuilder.BuildResolvedControl(controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(Literal))), node, dataContext);
                    treeBuilder.SetPropertyBinding(literal, Literal.TextProperty, ProcessBinding(binding, dataContext));
                    treeBuilder.SetPropertyValue(literal, Literal.RenderSpanElementProperty, false);
                    return literal;
                }
                else if (node is DotHtmlCommentNode)
                {
                    var commentNode = node as DotHtmlCommentNode;

                    // HTML comment
                    var text = commentNode.IsServerSide ? "" : "<!--" + commentNode.Value + "-->";
                    var literal = treeBuilder.BuildResolvedControl(controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(RawLiteral))), node, dataContext);
                    literal.ConstructorParameters = new object[] { text, commentNode.Value, true };
                    return literal;
                }
                else if (node is DothtmlLiteralNode)
                {
                    var literalNode = (DothtmlLiteralNode)node;
                    // text content
                    var whitespace = string.IsNullOrWhiteSpace(literalNode.Value);
                    if (!whitespace) EnsureContentAllowed(parentMetadata);

                    string text;
                    if (literalNode.Escape)
                    {
                        text = WebUtility.HtmlEncode(literalNode.Value);
                    }
                    else
                    {
                        text = literalNode.Value;
                    }

                    var literal = treeBuilder.BuildResolvedControl(controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(RawLiteral))), node, dataContext);
                    literal.ConstructorParameters = new object[] { text, literalNode.Value, whitespace };
                    return literal;
                }
                else if (node is DothtmlElementNode)
                {
                    // HTML element
                    var element = (DothtmlElementNode)node;
                    EnsureContentAllowed(parentMetadata);

                    // the element is the content
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
                throw;
            }
            catch (Exception ex)
            {
                throw new DotvvmCompilationException("", ex, node.Tokens);
            }
        }

        /// <summary>
        /// Processes the HTML element that represents a new object.
        /// </summary>
        private IAbstractControl ProcessObjectElement(DothtmlElementNode element, IDataContextStack dataContext)
        {
            object[] constructorParameters;

            // build control
            var controlMetadata = controlResolver.ResolveControl(element.TagPrefix, element.TagName, out constructorParameters);
            var control = treeBuilder.BuildResolvedControl(controlMetadata, element, dataContext);
            control.ConstructorParameters = constructorParameters;

            // resolve data context
            var dataContextAttribute = element.Attributes.FirstOrDefault(a => a.AttributeName == "DataContext");
            if (dataContextAttribute != null)
            {
                ProcessAttribute(dataContextAttribute, control, dataContext);
            }
            IAbstractPropertySetter dataContextProperty;
            if (control.TryGetProperty(DotvvmBindableObject.DataContextProperty, out dataContextProperty) && dataContextProperty is IAbstractPropertyBinding)
            {
                dataContext = CreateDataContextTypeStack(((IAbstractPropertyBinding)dataContextProperty).Binding.ResultType, parentDataContextStack: dataContext);
                control.DataContextTypeStack = dataContext;
            }
            if (controlMetadata.DataContextConstraint != null && !controlMetadata.DataContextConstraint.IsAssignableFrom(dataContext.DataContextType))
            {
                throw new DotvvmCompilationException($"The control '{controlMetadata.Type.Name}' requires a DataContext of type '{controlMetadata.DataContextConstraint.FullName}'!", element.Tokens);
            }

            // set properties from attributes
            foreach (var attribute in element.Attributes.Where(a => a.AttributeName != "DataContext"))
            {
                ProcessAttribute(attribute, control, dataContext);
            }
            
            // process control contents
            ProcessControlContent(control, element.Content);

            // check required properties
            IAbstractPropertySetter missingProperty;
            var missingProperties = control.Metadata.AllProperties.Where(p => p.MarkupOptions.Required && !control.TryGetProperty(p, out missingProperty)).ToList();
            if (missingProperties.Any())
            {
                throw new DotvvmCompilationException($"The control '{ control.Metadata.Type.FullName }' is missing required properties: { string.Join(", ", missingProperties.Select(p => "'" + p.Name + "'")) }.", control.DothtmlNode.Tokens);
            }
            return control;
        }

        /// <summary>
        /// Processes the binding node.
        /// </summary>
        public IAbstractBinding ProcessBinding(DothtmlBindingNode node, IDataContextStack context)
        {
            var bindingOptions = controlResolver.ResolveBinding(node.Name);
            return CompileBinding(node, bindingOptions, context);
        }

        /// <summary>
        /// Processes the attribute node.
        /// </summary>
        private void ProcessAttribute(DothtmlAttributeNode attribute, IAbstractControl control, IDataContextStack dataContext)
        {
            if (attribute.AttributePrefix == "html")
            {
                if (!control.Metadata.HasHtmlAttributesCollection)
                {
                    throw new DotvvmCompilationException($"The control '{control.Metadata.Type.FullName}' cannot use HTML attributes!", attribute.Tokens);
                }
                treeBuilder.SetHtmlAttribute(control, attribute.AttributeName, ProcessAttributeValue(attribute.ValueNode, dataContext));
                return;
            }

            if (!string.IsNullOrEmpty(attribute.AttributePrefix))
            {
                throw new DotvvmCompilationException("Attributes with XML namespaces are not supported!", attribute.Tokens);
            }

            // find the property
            var property = FindProperty(control.Metadata, attribute.AttributeName);
            if (property != null)
            {
                if (property.IsBindingProperty)
                {
                    var newDataContextType = GetDataContextChange(dataContext, control, property);
                    if (newDataContextType != null)
                    {
                        dataContext = CreateDataContextTypeStack(newDataContextType, parentDataContextStack: dataContext);
                    }
                }

                if (!property.MarkupOptions.MappingMode.HasFlag(MappingMode.Attribute))
                {
                    throw new DotvvmCompilationException($"The property '{property.FullName}' cannot be used as a control attribute!", attribute.Tokens);
                }

                // set the property
                if (attribute.ValueNode == null)
                {
                    throw new DotvvmCompilationException($"The attribute '{property.Name}' on the control '{control.Metadata.Type.FullName}' must have a value!", attribute.Tokens);
                }
                else if (attribute.ValueNode is DothtmlValueBindingNode)
                {
                    // binding
                    var bindingNode = (attribute.ValueNode as DothtmlValueBindingNode).BindingNode;
                    if (!property.MarkupOptions.AllowBinding)
                    {
                        throw new DotvvmCompilationException($"The property '{ property.FullName }' cannot contain binding.", bindingNode.Tokens);
                    }
                    var resolvedBinding = ProcessBinding(bindingNode, dataContext);
                    treeBuilder.SetPropertyBinding(control, property, resolvedBinding);
                }
                else
                {
                    // hard-coded value in markup
                    if (!property.MarkupOptions.AllowHardCodedValue)
                    {
                        throw new DotvvmCompilationException($"The property '{ property.FullName }' cannot contain hard coded value.", attribute.ValueNode.Tokens);
                    }

                    var textValue = attribute.ValueNode as DothtmlValueTextNode;
                    var value = ConvertValue(textValue.Text, property.PropertyType);
                    treeBuilder.SetPropertyValue(control, property, value);
                }
            }
            else if (control.Metadata.HasHtmlAttributesCollection)
            {
                // if the property is not found, add it as an HTML attribute
                treeBuilder.SetHtmlAttribute(control, attribute.AttributeName, ProcessAttributeValue(attribute.ValueNode, dataContext));
            }
            else
            {
                throw new DotvvmCompilationException($"The control '{control.Metadata.Type}' does not have a property '{attribute.AttributeName}' and does not allow HTML attributes!");
            }
        }

        private object ProcessAttributeValue(DothtmlValueNode valueNode, IDataContextStack dataContext)
        {
            if (valueNode is DothtmlValueBindingNode)
            {
                return ProcessBinding((valueNode as DothtmlValueBindingNode).BindingNode, dataContext);
            }
            else
            {
                return (valueNode as DothtmlValueTextNode)?.Text;
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
                    var property = FindProperty(control.Metadata, element.TagName);
                    if (property != null && string.IsNullOrEmpty(element.TagPrefix) && property.MarkupOptions.MappingMode.HasFlag(MappingMode.InnerElement))
                    {
                        content.Clear();
                        treeBuilder.SetProperty(control, ProcessElementProperty(control, property, element.Content));
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
            if (content.Any(DothtmlNodeHelper.IsNotEmpty))
            {
                if (control.Metadata.DefaultContentProperty != null)
                {
                    treeBuilder.SetProperty(control, ProcessElementProperty(control, control.Metadata.DefaultContentProperty, content));
                }
                else
                {
                    foreach (var node in content)
                    {
                        // TODO: data context from parent
                        treeBuilder.AddChildControl(control, ProcessNode(node, control.Metadata, control.DataContextTypeStack));
                    }
                }
            }
        }

        /// <summary>
        /// Processes the element which contains property value.
        /// </summary>
        private IAbstractPropertySetter ProcessElementProperty(IAbstractControl control, IPropertyDescriptor property, IEnumerable<DothtmlNode> elementContent)
        {
            // resolve data context
            var dataContext = control.DataContextTypeStack;
            var newDataContextType = GetDataContextChange(dataContext, control, property);
            if (newDataContextType != null)
            {
                dataContext = CreateDataContextTypeStack(newDataContextType, parentDataContextStack: dataContext);
            }

            // the element is a property 
            if (IsTemplateProperty(property))
            {
                // template
                return treeBuilder.BuildPropertyTemplate(property, ProcessTemplate(elementContent, dataContext));
            }
            else if (IsCollectionProperty(property))
            {
                // collection of elements
                var collection = FilterNodes<DothtmlElementNode>(elementContent, property)
                    .Select(childObject => ProcessObjectElement(childObject, dataContext));
                return treeBuilder.BuildPropertyControlCollection(property, collection);
            }
            else if (property.PropertyType.IsEqualTo(new ResolvedTypeDescriptor(typeof(string))))
            {
                // string property
                var strings = FilterNodes<DothtmlLiteralNode>(elementContent, property);
                var value = string.Concat(strings.Select(s => s.Value));
                return treeBuilder.BuildPropertyValue(property, value);
            }
            else if (IsControlProperty(property))
            {
                // new object
                var children = FilterNodes<DothtmlElementNode>(elementContent, property).ToList();
                if (children.Count > 1)
                {
                    throw new DotvvmCompilationException($"The property '{property.MarkupOptions.Name}' can have only one child element!");
                }
                else if (children.Count == 1)
                {
                    return treeBuilder.BuildPropertyControl(property, ProcessObjectElement(children[0], dataContext));
                }
                else
                {
                    return treeBuilder.BuildPropertyControl(property, null);
                }
            }
            else
            {
                throw new DotvvmCompilationException($"The property '{property.FullName}' is not supported!");
            }
        }

        /// <summary>
        /// Processes the template contents.
        /// </summary>
        private List<IAbstractControl> ProcessTemplate(IEnumerable<DothtmlNode> elementContent, IDataContextStack dataContext)
        {
            var placeholderMetadata = controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(PlaceHolder)));
            var content = elementContent.Select(e => ProcessNode(e, placeholderMetadata, dataContext));
            return content.ToList();
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
                    throw new DotvvmCompilationException($"Content is not allowed inside the property '{property.FullName}'! (Conflicting node: Node {child.GetType().Name})");
                }
            }
        }

        /// <summary>
        /// Resolves the type of the wrapper.
        /// </summary>
        private ITypeDescriptor ResolveWrapperType(DothtmlRootNode node, string fileName)
        {
            var wrapperType = GetDefaultWrapperType(fileName);

            var baseControlDirective = node.Directives.SingleOrDefault(d => string.Equals(d.Name, Constants.BaseTypeDirective, StringComparison.InvariantCultureIgnoreCase));
            if (baseControlDirective != null)
            {
                wrapperType = FindType(baseControlDirective.Value);
                if (wrapperType == null)
                {
                    throw new DotvvmCompilationException($"The type '{baseControlDirective.Value}' specified in baseType directive was not found!");
                }
                if (!wrapperType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmMarkupControl))))
                {
                    throw new DotvvmCompilationException("Markup controls must derive from DotvvmMarkupControl class!");
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

        /// <summary>
        /// Finds the property in the control metadata.
        /// </summary>
        protected IPropertyDescriptor FindProperty(IControlResolverMetadata parentMetadata, string name)
        {
            // try to find the property in metadata
            IPropertyDescriptor property;
            if (parentMetadata.TryGetProperty(name, out property))
            {
                return property;
            }

            // try to find an attached property
            if (name.Contains("."))
            {
                return FindGlobalProperty(name);
            }

            return null;
        }

        /// <summary>
        /// Checks that the element can have inner contents.
        /// </summary>
        private void EnsureContentAllowed(IControlResolverMetadata controlMetadata)
        {
            if (!controlMetadata.IsContentAllowed)
            {
                throw new DotvvmCompilationException($"The content is not allowed inside the control '{controlMetadata.Type.FullName}'!");
            }
        }

        protected virtual bool IsCollectionProperty(IPropertyDescriptor property)
        {
            return property.PropertyType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(ICollection)));
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
        /// Creates the IControlType identification of the control.
        /// </summary>
        protected abstract IControlType CreateControlType(ITypeDescriptor wrapperType, string virtualPath);

        /// <summary>
        /// Creates the data context type stack object.
        /// </summary>
        protected abstract IDataContextStack CreateDataContextTypeStack(ITypeDescriptor viewModelType, ITypeDescriptor wrapperType = null, IDataContextStack parentDataContextStack = null);

        /// <summary>
        /// Converts the value to the property type.
        /// </summary>
        protected abstract object ConvertValue(string value, ITypeDescriptor propertyType);

        /// <summary>
        /// Gets the data context change behavior for the specified control property.
        /// </summary>
        protected abstract ITypeDescriptor GetDataContextChange(IDataContextStack dataContext, IAbstractControl control, IPropertyDescriptor property);

        /// <summary>
        /// Finds the type descriptor for full type name.
        /// </summary>
        protected abstract ITypeDescriptor FindType(string fullTypeNameWithAssembly);

        /// <summary>
        /// Finds the DotVVM property in the global property store.
        /// </summary>
        protected abstract IPropertyDescriptor FindGlobalProperty(string name);

        /// <summary>
        /// Compiles the binding.
        /// </summary>
        protected abstract IAbstractBinding CompileBinding(DothtmlBindingNode node, BindingParserOptions bindingOptions, IDataContextStack context);

    }
}
