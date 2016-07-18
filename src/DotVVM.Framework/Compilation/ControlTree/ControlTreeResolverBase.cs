﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Compilation.ControlTree
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

		public static HashSet<string> MultiValueDirectives = new HashSet<string> { ParserConstants.BaseTypeDirective, ParserConstants.MasterPageDirective, ParserConstants.ResourceTypeDirective, ParserConstants.ViewModelDirectiveName };

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
			var dataContextTypeStack = ResolveViewModel(fileName, root, wrapperType);
			var view = treeBuilder.BuildTreeRoot(this, viewMetadata, root, dataContextTypeStack);

			if (dataContextTypeStack != null) dataContextTypeStack.NamespaceImports = new NamespaceImport[0];
			foreach (var directive in root.Directives.GroupBy(d => d.Name))
			{
				if (!string.Equals(directive.Key, ParserConstants.BaseTypeDirective, StringComparison.InvariantCultureIgnoreCase))
				{
					var list = directive.Select(d => d.Value).ToList();
					if (MultiValueDirectives.Contains(directive.Key) && list.Count > 1)
					{
						foreach (var d in directive)
						{
							d.AddError($"Directive '{d.Name}' can not be present multiple times.");
						}
						view.Directives[directive.Key] = list.Take(1).ToList();
					}
					else view.Directives[directive.Key] = list;
					if ((directive.Key == ParserConstants.ImportNamespaceDirective || directive.Key == ParserConstants.ResourceNamespaceDirective) && dataContextTypeStack != null)
					{
						dataContextTypeStack.NamespaceImports = directive.Select(d =>
						{
							var split = d.Value.Split('=');
							if (split.Length == 1)
								return new NamespaceImport(split[0].Trim());
							else return new NamespaceImport(split[1].Trim(), split[0].Trim());
						}).Concat(dataContextTypeStack.NamespaceImports).ToArray();
					}
				}
			}

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
				var child = ProcessNode(view, node, viewMetadata, view.DataContextTypeStack);
				if (child != null) treeBuilder.AddChildControl(view, child);
			}
		}

		/// <summary>
		/// Resolves the view model for the root node.
		/// </summary>
		protected virtual IDataContextStack ResolveViewModel(string fileName, DothtmlRootNode root, ITypeDescriptor wrapperType)
		{
			var viewModelDirective = root.Directives.FirstOrDefault(d => d.Name == ParserConstants.ViewModelDirectiveName);
			if (viewModelDirective == null)
			{
				root.AddError($"The @viewModel directive is missing in the page '{fileName}'!");
				return null;
			}

			var viewModelType = FindType(viewModelDirective.Value);
			if (viewModelType == null)
			{
				viewModelDirective.AddError($"The type '{viewModelDirective.Value}' required in the @viewModel directive was not found!");
				return null;
			}
			return CreateDataContextTypeStack(viewModelType, wrapperType);
		}

		/// <summary>
		/// Processes the parser node and builds a control.
		/// </summary>
		public IAbstractControl ProcessNode(IAbstractTreeNode parent, DothtmlNode node, IControlResolverMetadata parentMetadata, IDataContextStack dataContext)
		{
			try
			{
				if (node is DothtmlBindingNode)
				{
					// binding in text
					EnsureContentAllowed(parentMetadata, node);
					return ProcessBindingInText(node, dataContext);
				}
				else if (node is DotHtmlCommentNode)
				{
					// HTML comment
					var commentNode = node as DotHtmlCommentNode;
					return ProcessHtmlComment(node, dataContext, commentNode);
				}
				else if (node is DothtmlLiteralNode)
				{
					// text content
					var literalNode = (DothtmlLiteralNode)node;
					return ProcessText(node, parentMetadata, dataContext, literalNode);
				}
				else if (node is DothtmlElementNode)
				{
					// HTML element
					EnsureContentAllowed(parentMetadata, node);
					var element = (DothtmlElementNode)node;
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
			if (!whitespace)
			{
				EnsureContentAllowed(parentMetadata, node);
			}

			string text;
			if (literalNode.Escape)
			{
				text = WebUtility.HtmlEncode(literalNode.Value);
			}
			else
			{
				text = literalNode.Value;
			}

			var rawLiteralMetadata = controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(RawLiteral)));
			var literal = treeBuilder.BuildControl(rawLiteralMetadata, node, dataContext);
			literal.ConstructorParameters = new object[] { text, literalNode.Value, whitespace };
			return literal;
		}

		private IAbstractControl ProcessHtmlComment(DothtmlNode node, IDataContextStack dataContext, DotHtmlCommentNode commentNode)
		{
			var text = commentNode.IsServerSide ? "" : "<!--" + commentNode.Value + "-->";

			var rawLiteralMetadata = controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(RawLiteral)));
			var literal = treeBuilder.BuildControl(rawLiteralMetadata, node, dataContext);
			literal.ConstructorParameters = new object[] { text, commentNode.Value, true };
			return literal;
		}

		private IAbstractControl ProcessBindingInText(DothtmlNode node, IDataContextStack dataContext)
		{
			var bindingNode = (DothtmlBindingNode)node;
			var literalMetadata = controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(Literal)));
			var literal = treeBuilder.BuildControl(literalMetadata, node, dataContext);

			var textBinding = ProcessBinding(bindingNode, dataContext);
			var textProperty = treeBuilder.BuildPropertyBinding(Literal.TextProperty, textBinding);
			treeBuilder.SetProperty(literal, textProperty);

			var renderSpanElement = treeBuilder.BuildPropertyValue(Literal.RenderSpanElementProperty, false);
			treeBuilder.SetProperty(literal, renderSpanElement);

			return literal;
		}

		/// <summary>
		/// Processes the HTML element that represents a new object.
		/// </summary>
		private IAbstractControl ProcessObjectElement(DothtmlElementNode element, IDataContextStack dataContext)
		{
			object[] constructorParameters;

			// build control
			var controlMetadata = controlResolver.ResolveControl(element.TagPrefix, element.TagName, out constructorParameters);
			if (controlMetadata == null)
			{
				controlMetadata = controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(HtmlGenericControl)));
				constructorParameters = new[] { element.FullTagName };
				element.AddError($"The control <{element.FullTagName}> could not be resolved! Make sure that the tagPrefix is registered in DotvvmConfiguration.Markup.Controls collection!");
			}
			var control = treeBuilder.BuildControl(controlMetadata, element, dataContext);
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
				var dataContextBinding = ((IAbstractPropertyBinding)dataContextProperty).Binding;
				if (dataContextBinding?.ResultType != null && dataContext != null)
				{
					dataContext = CreateDataContextTypeStack(dataContextBinding?.ResultType, parentDataContextStack: dataContext);
				}
				else if (dataContext != null)
				{
					dataContext = CreateDataContextTypeStack(null, null, dataContext);
				}
				else
				{
					dataContext = null;
				}
				control.DataContextTypeStack = dataContext;
			}
			if (controlMetadata.DataContextConstraint != null && dataContext != null && !controlMetadata.DataContextConstraint.IsAssignableFrom(dataContext.DataContextType))
			{
				((DothtmlNode)dataContextAttribute ?? element)
				   .AddError($"The control '{controlMetadata.Type.Name}' requires a DataContext of type '{controlMetadata.DataContextConstraint.FullName}'!");
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
				element.AddError($"The control '{ control.Metadata.Type.FullName }' is missing required properties: { string.Join(", ", missingProperties.Select(p => "'" + p.Name + "'")) }.");
			}
			return control;
		}

		/// <summary>
		/// Processes the binding node.
		/// </summary>
		public IAbstractBinding ProcessBinding(DothtmlBindingNode node, IDataContextStack context)
		{
			var bindingOptions = controlResolver.ResolveBinding(node.Name);
			if (bindingOptions == null)
			{
				node.NameNode.AddError($"Binding {node.Name} could not be resolved.");
				bindingOptions = controlResolver.ResolveBinding("value"); // just try it as with value binding
			}

			if (context != null && context.NamespaceImports.Length > 0)
				bindingOptions = bindingOptions.AddImports(context.NamespaceImports);

			return CompileBinding(node, bindingOptions, context);
		}

		static HashSet<string> treatBindingAsHardCodedValue = new HashSet<string> { "resource" };

		/// <summary>
		/// Processes the attribute node.
		/// </summary>
		private void ProcessAttribute(DothtmlAttributeNode attribute, IAbstractControl control, IDataContextStack dataContext)
		{
			if (attribute.AttributePrefix == "html")
			{
				if (!control.Metadata.HasHtmlAttributesCollection)
				{
					attribute.AddError($"The control '{control.Metadata.Type.FullName}' cannot use HTML attributes!");
				}
				else
				{
					try
					{
						treeBuilder.SetHtmlAttribute(control, attribute.AttributeName, ProcessAttributeValue(attribute.ValueNode, dataContext));
					}
					catch (NotSupportedException ex)
					{
						if (ex.InnerException == null) throw;
						else attribute.AddError(ex.Message);
					}
				}
				return;
			}

			if (!string.IsNullOrEmpty(attribute.AttributePrefix))
			{
				attribute.AddError("Attributes with XML namespaces are not supported!");
				return;
			}

			// find the property
			var property = FindProperty(control.Metadata, attribute.AttributeName);
			if (property != null)
			{
				if (control.HasProperty(property)) attribute.AttributeNameNode.AddError($"control '{ ((DothtmlElementNode)control.DothtmlNode).FullTagName }' already has property '{ attribute.AttributeName }'.");

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
                    if (property.PropertyType.IsEqualTo(new ResolvedTypeDescriptor(typeof(bool))))
                    {
                        treeBuilder.SetProperty(control, treeBuilder.BuildPropertyValue(property, true));
                    }
					attribute.AddError($"The attribute '{property.Name}' on the control '{control.Metadata.Type.FullName}' must have a value!");
				}
				else if (attribute.ValueNode is DothtmlValueBindingNode)
				{
					// binding
					var bindingNode = (attribute.ValueNode as DothtmlValueBindingNode).BindingNode;
					if (property.IsVirtual)
					{
						attribute.ValueNode.AddError($"The property '{ property.FullName }' cannot contain bindings because it's not DotvvmProperty.");
					}
					else if (treatBindingAsHardCodedValue.Contains(bindingNode.Name))
					{
						if (!property.MarkupOptions.AllowHardCodedValue)
							attribute.ValueNode.AddError($"The property '{ property.FullName }' cannot contain {bindingNode.Name} binding, it can contain only hard-coded value or resource binding.");
					}
					else
					{
						if (!property.MarkupOptions.AllowBinding)
							attribute.ValueNode.AddError($"The property '{ property.FullName }' cannot contain {bindingNode.Name} binding.");
					}
					var binding = ProcessBinding(bindingNode, dataContext);
					var bindingProperty = treeBuilder.BuildPropertyBinding(property, binding);
					treeBuilder.SetProperty(control, bindingProperty);
				}
				else
				{
					// hard-coded value in markup
					if (!property.MarkupOptions.AllowHardCodedValue)
					{
						attribute.ValueNode.AddError($"The property '{ property.FullName }' cannot contain hard coded value.");
					}

					var textValue = attribute.ValueNode as DothtmlValueTextNode;
					var value = ConvertValue(textValue.Text, property.PropertyType);
					var propertyValue = treeBuilder.BuildPropertyValue(property, value);
					treeBuilder.SetProperty(control, propertyValue);
				}
			}
			else if (control.Metadata.HasHtmlAttributesCollection)
			{
				// if the property is not found, add it as an HTML attribute
				try
				{
					treeBuilder.SetHtmlAttribute(control, attribute.AttributeName, ProcessAttributeValue(attribute.ValueNode, dataContext));
				}
				catch (NotSupportedException ex)
				{
					if (ex.InnerException == null) throw;
					else attribute.AddError(ex.Message);
				}
			}
			else
			{
				attribute.AddError($"The control '{control.Metadata.Type}' does not have a property '{attribute.AttributeName}' and does not allow HTML attributes!");
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
				if (control.HasProperty(control.Metadata.DefaultContentProperty))
				{
					foreach (var c in content)
						if (c.IsNotEmpty())
							c.AddError($"Property { control.Metadata.DefaultContentProperty.FullName } was already set.");
				}
				else if (!content.All(c => c is DothtmlLiteralNode && string.IsNullOrWhiteSpace(((DothtmlLiteralNode)c).Value)))
					treeBuilder.SetProperty(control, ProcessElementProperty(control, control.Metadata.DefaultContentProperty, content));
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
			foreach (var node in content)
			{
				var child = ProcessNode(control, node, control.Metadata, control.DataContextTypeStack);
				if (child != null) treeBuilder.AddChildControl(control, child);
			}
		}

		/// <summary>
		/// Processes the element which contains property value.
		/// </summary>
		private IAbstractPropertySetter ProcessElementProperty(IAbstractControl control, IPropertyDescriptor property, IEnumerable<DothtmlNode> elementContent)
		{
			// resolve data context
			var dataContext = control.DataContextTypeStack;
			dataContext = GetDataContextChange(dataContext, control, property);

			// the element is a property 
			if (IsTemplateProperty(property))
			{
				// template
				return treeBuilder.BuildPropertyTemplate(property, ProcessTemplate(control, elementContent, dataContext));
			}
			else if (IsCollectionProperty(property))
			{
				var collectionType = GetCollectionType(property);
				// collection of elements
				var collection =
						FilterNodes<DothtmlElementNode>(elementContent, property)
						.Select(childObject => ProcessObjectElement(childObject, dataContext));
				if (collectionType != null)
				{
					collection = FilterOrError(collection,
						c => c.Metadata.Type.IsAssignableTo(collectionType),
						c => c.DothtmlNode.AddError($"Control type {c.Metadata.Type.FullName} can't be used in collection of type {collectionType.FullName}."));
				}

				return treeBuilder.BuildPropertyControlCollection(property, collection.ToArray());
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
					foreach (var c in children.Skip(1)) c.AddError($"The property '{property.MarkupOptions.Name}' can have only one child element!");
					children = children.Take(1).ToList();
				}
				if (children.Count == 1)
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
				control.DothtmlNode.AddError($"The property '{property.FullName}' is not supported!");
				return treeBuilder.BuildPropertyValue(property, null);
			}
		}

		/// <summary>
		/// Processes the template contents.
		/// </summary>
		private List<IAbstractControl> ProcessTemplate(IAbstractTreeNode parent, IEnumerable<DothtmlNode> elementContent, IDataContextStack dataContext)
		{
			var placeholderMetadata = controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(PlaceHolder)));
			var content = elementContent.Select(e => ProcessNode(parent, e, placeholderMetadata, dataContext)).Where(e => e != null);
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
		private ITypeDescriptor ResolveWrapperType(DothtmlRootNode node, string fileName)
		{
			var wrapperType = GetDefaultWrapperType(fileName);

			var baseControlDirective = node.Directives.SingleOrDefault(d => string.Equals(d.Name, ParserConstants.BaseTypeDirective, StringComparison.InvariantCultureIgnoreCase));
			if (baseControlDirective != null)
			{
				var baseType = FindType(baseControlDirective.Value);
				if (baseType == null)
				{
					baseControlDirective.AddError($"The type '{baseControlDirective.Value}' specified in baseType directive was not found!");
				}
				else if (!baseType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmMarkupControl))))
				{
					baseControlDirective.AddError("Markup controls must derive from DotvvmMarkupControl class!");
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
		private void EnsureContentAllowed(IControlResolverMetadata controlMetadata, DothtmlNode node)
		{
			if (!controlMetadata.IsContentAllowed)
			{
				node.AddError($"The content is not allowed inside the control '{controlMetadata.Type.FullName}'!");
			}
		}

		protected virtual bool IsCollectionProperty(IPropertyDescriptor property)
		{
			return property.PropertyType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(ICollection)));
		}

		protected virtual ITypeDescriptor GetCollectionType(IPropertyDescriptor property)
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
		protected virtual IDataContextStack GetDataContextChange(IDataContextStack dataContext, IAbstractControl control, IPropertyDescriptor property)
		{
			if (dataContext == null) return null;

			var manipulationAttribute = property != null ? property.DataContextManipulationAttribute : control.Metadata.DataContextManipulationAttribute;
			if (manipulationAttribute != null)
			{
				return manipulationAttribute.ChangeStackForChildren(dataContext, control, property, (parent, changeType) => CreateDataContextTypeStack(changeType, parentDataContextStack: parent));
			}

			var attributes = property != null ? property.DataContextChangeAttributes : control.Metadata.DataContextChangeAttributes;
			if (attributes == null || attributes.Length == 0) return dataContext;

			var type = dataContext.DataContextType;
			foreach (var attribute in attributes.OrderBy(a => a.Order))
			{
				type = attribute.GetChildDataContextType(type, dataContext, control, property);
				if (type == null) break;
			}
			if (type == null) return dataContext;
			else return CreateDataContextTypeStack(type, parentDataContextStack: dataContext);
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
