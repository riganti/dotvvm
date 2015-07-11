using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Utils;
using System.Collections;
using ExpressionEvaluator;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class ControlTreeResolver : IControlTreeResolver
    {
        private IControlResolver controlResolver;
        public ControlTreeResolver(DotvvmConfiguration configuration)
        {
            controlResolver = configuration.ServiceLocator.GetService<IControlResolver>();
        }

        public ResolvedView ResolveTree(DothtmlRootNode root, string fileName)
        {
            var wrapperType = ResolveWrapperType(root);
            var viewMetadata = controlResolver.ResolveControl(new ControlType(wrapperType, virtualPath: fileName));
            var view = new ResolvedView(viewMetadata, root);

            foreach (var directive in root.Directives)
            {
                if(directive.Name != Constants.BaseTypeDirective)
                {
                    view.Directives.Add(directive.Name, directive.Value);
                }
            }

            foreach (var node in root.Content)
            {
                view.Content.Add(ProcessNode(node, viewMetadata));
            }
            return view;
        }

        private ResolvedControl ProcessNode(DothtmlNode node, ControlResolverMetadata parentMetadata)
        {
            if (node is DothtmlBindingNode)
            {
                EnsureContentAllowed(parentMetadata);

                // binding in text
                var binding = (DothtmlBindingNode)node;
                var literal = new ResolvedControl(controlResolver.ResolveControl(typeof(Literal)), node);
                literal.SetProperty(new ResolvedPropertyBinding(Literal.TextProperty, ProcessBinding(binding)));
                return literal;
            }
            else if (node is DothtmlLiteralNode)
            {
                // text content
                var literalValue = ((DothtmlLiteralNode)node).Value;
                if (node.IsNotEmpty())
                {
                    EnsureContentAllowed(parentMetadata);
                }
                var literal = new ResolvedControl(controlResolver.ResolveControl(typeof(Literal)), node);
                literal.SetPropertyValue(Literal.HtmlEncodeProperty, false);
                literal.SetPropertyValue(Literal.TextProperty, literalValue);
                return literal;
            }
            else if (node is DothtmlElementNode)
            {
                // HTML element
                var element = (DothtmlElementNode)node;
                EnsureContentAllowed(parentMetadata);

                // the element is the content
                return ProcessObjectElement(element);
            }
            else
            {
                throw new NotSupportedException($"{ node.GetType().Name } can't be inside element");      // TODO: exception handling
            }
        }

        /// <summary>
        /// Processes the HTML element that represents a new object.
        /// </summary>
        private ResolvedControl ProcessObjectElement(DothtmlElementNode element)
        {
            object[] constructorParameters;

            var controlMetadata = controlResolver.ResolveControl(element.TagPrefix, element.TagName, out constructorParameters);
            var control = new ResolvedControl(controlMetadata, element)
            {
                ContructorParameters = constructorParameters
            };
            // set properties from attributes
            foreach (var attribute in element.Attributes)
            {
                ProcessAttribute(attribute, control);
            }

            ProcessControlContent(element.Content, control);
            return control;
        }

        private ResolvedBinding ProcessBinding(DothtmlBindingNode node)
        {
            var c = new CompiledExpression(node.Value);
            return new ResolvedBinding()
            {
                Type = controlResolver.ResolveBinding(node.Name),
                Value = node.Value,
                Expression = c.Expression
            };
        }


        /// <summary>
        /// Processes the HTML attribute.
        /// </summary>
        private void ProcessAttribute(DothtmlAttributeNode attribute, ResolvedControl control)
        {
            if (!string.IsNullOrEmpty(attribute.AttributePrefix))
            {
                throw new NotSupportedException("Attributes with XML namespaces are not supported!"); // TODO: exception handling
            }
            // TODO: attribute prefixes (html:{name} will be translated to html attribute)
            // find the property
            var property = FindProperty(control.Metadata, attribute.AttributeName);
            if (property != null)
            {
                // set the property
                if (attribute.Literal == null)
                {
                    throw new NotSupportedException("empty attributes are not supported as DotvvmProperties");
                }
                else if (attribute.Literal is DothtmlBindingNode)
                {
                    // binding
                    var bindingNode = (DothtmlBindingNode)attribute.Literal;
                    var resolvedBinding = ProcessBinding(bindingNode);
                    control.SetProperty(new ResolvedPropertyBinding(property, resolvedBinding));
                }
                else
                {
                    // hard-coded value in markup
                    // TODO: smarter conversions
                    var value = ReflectionUtils.ConvertValue(attribute.Literal.Value, property.PropertyType);
                    control.SetPropertyValue(property, value);
                }
            }
            else if (control.Metadata.HasHtmlAttributesCollection)
            {
                // if the property is not found, add it as an HTML attribute
                object value = (attribute.Literal is DothtmlBindingNode) ?
                    (object)ProcessBinding((DothtmlBindingNode)attribute.Literal) :
                    attribute.Literal?.Value;

                if (control.HtmlAttributes == null) control.HtmlAttributes = new Dictionary<string, object>();
                control.HtmlAttributes.Add(attribute.AttributeName, value);
            }
            else
            {
                // TODO: exception handling
                throw new NotSupportedException(string.Format("The control {0} does not have a property {1}!", control.Metadata.Type, attribute.AttributeName));
            }
        }

        private void ProcessControlContent(IEnumerable<DothtmlNode> nodes, ResolvedControl control)
        {
            var content = new List<DothtmlNode>();
            bool properties = true;
            foreach (var node in nodes)
            {
                var element = node as DothtmlElementNode;
                if (element != null && properties)
                {
                    var property = FindProperty(control.Metadata, element.TagName);
                    if (property != null && string.IsNullOrEmpty(element.TagPrefix) && property.MarkupOptions.MappingMode == MappingMode.InnerElement)
                    {
                        content.Clear();
                        control.SetProperty(ProcessElementProperty(control, property, element.Content));
                    }
                    else properties = false;
                }
                if ((element != null && !properties) || element == null)
                    content.Add(node);
                if (properties && node.IsNotEmpty())
                {
                    properties = false;
                }
            }
            if (content.Any(DothtmlNodeHelper.IsNotEmpty))
            {
                if (control.Metadata.DefaultContentProperty != null)
                {
                    control.SetProperty(ProcessElementProperty(control, control.Metadata.DefaultContentProperty, content));
                }
                else
                {
                    foreach (var node in content)
                    {
                        control.Content.Add(ProcessNode(node, control.Metadata));
                    }
                }
            }
        }

        /// <summary>
        /// Processes the element which contains property value.
        /// </summary>
        private ResolvedPropertySetter ProcessElementProperty(ResolvedControl control, DotvvmProperty property, IEnumerable<DothtmlNode> elementContent)
        {
            // the element is a property 
            if (IsTemplateProperty(property))
            {
                // template
                return new ResolvedPropertyTemplate(property, ProcessTemplate(elementContent));
            }
            else if (IsCollectionProperty(property))
            {
                // collection of elements
                var collection = FilterNodes<DothtmlElementNode>(elementContent)
                    .Select(childObject => ProcessObjectElement(childObject));
                return new ResolvedPropertyControlCollection(property, collection.ToList());
            }
            else if (property.DeclaringType == typeof(string))
            {
                // string property
                var strings = FilterNodes<DothtmlLiteralNode>(elementContent);
                var value = string.Concat(strings.Select(s => s.Value));
                return new ResolvedPropertyValue(property, value);
            }
            else if (IsControlProperty(property))
            {
                // new object
                var children = FilterNodes<DothtmlElementNode>(elementContent).ToList();
                if (children.Count > 1)
                {
                    throw new NotSupportedException(string.Format("The property {0} can have only one child element!", property.MarkupOptions.Name)); // TODO: exception handling
                }
                else if(children.Count == 1)
                {
                    return new ResolvedPropertyControl(property, ProcessObjectElement(children[0]));
                }
                else
                {
                    return new ResolvedPropertyControl(property, null);
                }
            }
            else throw new NotSupportedException($"property type { property.DeclaringType.FullName } is not supported");
        }

        private List<ResolvedControl> ProcessTemplate(IEnumerable<DothtmlNode> elementContent)
        {
            var placeholderMetadata = controlResolver.ResolveControl(typeof(Placeholder));
            var content = elementContent.Select(e => ProcessNode(e, placeholderMetadata));
            return content.ToList();
        }

        /// <summary>
        /// Gets the inner property elements and makes sure that no other content is present.
        /// </summary>
        private IEnumerable<TNode> FilterNodes<TNode>(IEnumerable<DothtmlNode> nodes)
            where TNode : DothtmlNode
        {
            foreach (var child in nodes)
            {
                if (child is TNode)
                {
                    yield return (TNode)child;
                }
                else if (child.IsNotEmpty())
                {
                    throw new NotSupportedException("Content cannot be inside collection inner property!"); // TODO: exception handling
                }
            }
        }


        /// <summary>
        /// Resolves the type of the wrapper.
        /// </summary>
        private Type ResolveWrapperType(DothtmlRootNode node)
        {
            var wrapperType = typeof(DotvvmView);

            var baseControlDirective = node.Directives.SingleOrDefault(d => d.Name == Constants.BaseTypeDirective);
            if (baseControlDirective != null)
            {
                wrapperType = Type.GetType(baseControlDirective.Value);
                if (wrapperType == null)
                {
                    throw new Exception(string.Format(Resources.Controls.ViewCompiler_TypeSpecifiedInBaseTypeDirectiveNotFound, baseControlDirective.Value));
                }
                if (!typeof(DotvvmMarkupControl).IsAssignableFrom(wrapperType))
                {
                    throw new Exception(string.Format(Resources.Controls.ViewCompiler_MarkupControlMustDeriveFromDotvvmMarkupControl));
                }
            }

            return wrapperType;
        }

        private DotvvmProperty FindProperty(ControlResolverMetadata parentMetadata, string name)
        {
            return parentMetadata.FindProperty(name) ?? DotvvmProperty.ResolveProperty(name);
        }

        private static void EnsureContentAllowed(ControlResolverMetadata controlMetadata)
        {
            if (!controlMetadata.IsContentAllowed)
            {
                throw new Exception(string.Format("The content is not allowed inside the <{0}></{0}> control!", controlMetadata.Name));
            }
        }

        private static bool IsCollectionProperty(DotvvmProperty parentProperty)
        {
            return parentProperty.PropertyType.GetInterfaces().Contains(typeof(ICollection));
        }

        private static bool IsTemplateProperty(DotvvmProperty parentProperty)
        {
            return parentProperty.PropertyType == typeof(ITemplate);
        }

        private static bool IsControlProperty(DotvvmProperty property)
        {
            return typeof(DotvvmControl).IsAssignableFrom(property.PropertyType);
        }
    }
}
