using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp.RuntimeBinder;
using Redwood.Framework.Binding;
using Redwood.Framework.Configuration;
using Redwood.Framework.Controls;
using Redwood.Framework.Controls.Infrastructure;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Redwood.Framework.Utils;

namespace Redwood.Framework.Runtime.Compilation
{
    public class DefaultViewCompiler : IViewCompiler
    {
        private object locker = new object();

        public DefaultViewCompiler(RedwoodConfiguration configuration)
        {
            this.configuration = configuration;
            this.controlResolver = configuration.ServiceLocator.GetService<IControlResolver>();
            this.assemblyCache = CompiledAssemblyCache.Instance;
        }


        private readonly CompiledAssemblyCache assemblyCache;
        private readonly IControlResolver controlResolver;
        private readonly RedwoodConfiguration configuration;

        private DefaultViewCompilerCodeEmitter emitter;
        private int currentTemplateIndex = 0;



        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        public IControlBuilder CompileView(IReader reader, string fileName, string assemblyName, string namespaceName, string className)
        {
            lock (locker)
            {
                emitter = new DefaultViewCompilerCodeEmitter();

                // parse the document
                var tokenizer = new RwHtmlTokenizer();
                tokenizer.Tokenize(reader);
                var parser = new RwHtmlParser();
                var node = parser.Parse(tokenizer.Tokens);

                // determine wrapper type
                string wrapperClassName;
                var wrapperType = ResolveWrapperType(node, className, out wrapperClassName);
                var metadata = controlResolver.ResolveControl(new ControlType(wrapperType, virtualPath: fileName));

                // build the statements
                emitter.PushNewMethod(DefaultViewCompilerCodeEmitter.BuildControlFunctionName);
                var pageName = wrapperClassName == null ? emitter.EmitCreateObject(wrapperType) : emitter.EmitCreateObject(wrapperClassName);
                emitter.EmitSetAttachedProperty(pageName, typeof(Internal).FullName, Internal.UniqueIDProperty.Name, pageName);
                foreach (var child in node.Content)
                {
                    ProcessNode(child, pageName, metadata);
                }

                var directivesToApply = node.Directives.Where(d => d.Name != Constants.BaseTypeDirective).ToList();
                if (wrapperType.IsAssignableFrom(typeof(RedwoodView)))
                {
                    foreach (var directive in directivesToApply)
                    {
                        emitter.EmitAddDirective(pageName, directive.Name, directive.Value);
                    }
                }
                emitter.EmitReturnClause(pageName);
                emitter.PopMethod();

                // create the assembly
                var assembly = BuildAssembly(assemblyName, namespaceName, className);
                var controlBuilder = (IControlBuilder)assembly.CreateInstance(namespaceName + "." + className);
                metadata.ControlBuilderType = controlBuilder.GetType();
                return controlBuilder;
            }
        }

        /// <summary>
        /// Resolves the type of the wrapper.
        /// </summary>
        private Type ResolveWrapperType(RwHtmlRootNode node, string className, out string controlClassName)
        {
            var wrapperType = typeof(RedwoodView);

            var baseControlDirective = node.Directives.SingleOrDefault(d => d.Name == Constants.BaseTypeDirective);
            if (baseControlDirective != null)
            {
                wrapperType = Type.GetType(baseControlDirective.Value);
                if (wrapperType == null)
                {
                    throw new Exception(string.Format(Resources.Controls.ViewCompiler_TypeSpecifiedInBaseTypeDirectiveNotFound, baseControlDirective.Value));
                }
                if (!typeof(RedwoodMarkupControl).IsAssignableFrom(wrapperType))
                {
                    throw new Exception(string.Format(Resources.Controls.ViewCompiler_MarkupControlMustDeriveFromRedwoodMarkupControl));
                }

                controlClassName = null;
            }
            else
            {
                controlClassName = className + "Control";
                emitter.EmitControlClass(wrapperType, className);
            }

            return wrapperType;
        }

        /// <summary>
        /// Builds the assembly.
        /// </summary>
        private Assembly BuildAssembly(string assemblyName, string namespaceName, string className)
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
                    throw new Exception("The compilation failed! This is most probably bug in the Redwood framework.\r\n\r\n"
                        + string.Join("\r\n", result.Diagnostics)
                        + "\r\n\r\n" + compilation.SyntaxTrees[0] + "\r\n\r\n");
                }
            }
        }

        /// <summary>
        /// Processes the node.
        /// </summary>
        private void ProcessNode(RwHtmlNode node, string parentName, ControlResolverMetadata parentMetadata)
        {
            if (node is RwHtmlBindingNode)
            {
                EnsureContentAllowed(parentMetadata);

                // binding in text
                var binding = (RwHtmlBindingNode)node;
                var currentObjectName = emitter.EmitCreateObject(typeof(Literal), new object[] { ((RwHtmlLiteralNode)node).Value, true });
                var bindingObjectName = emitter.EmitCreateObject(controlResolver.ResolveBinding(binding.Name), new object[] { binding.Value });
                emitter.EmitSetBinding(currentObjectName, Literal.TextProperty.DescriptorFullName, bindingObjectName);
                emitter.EmitAddCollectionItem(parentName, currentObjectName);
            }
            else if (node is RwHtmlLiteralNode)
            {
                // text content
                var literalValue = ((RwHtmlLiteralNode)node).Value;
                if (!string.IsNullOrWhiteSpace(literalValue))
                {
                    EnsureContentAllowed(parentMetadata);
                }
                var currentObjectName = emitter.EmitCreateObject(typeof(Literal), new object[] { literalValue });
                emitter.EmitAddCollectionItem(parentName, currentObjectName);
            }
            else if (node is RwHtmlElementNode)
            {
                // HTML element
                var element = (RwHtmlElementNode)node;
                EnsureContentAllowed(parentMetadata);

                // the element is the content
                var currentObjectName = ProcessObjectElement(element);
                emitter.EmitAddCollectionItem(parentName, currentObjectName);
            }
            else
            {
                throw new NotSupportedException($"{ node.GetType().Name } can't be inside element");      // TODO: exception handling
            }
        }

        private void EnsureContentAllowed(ControlResolverMetadata controlMetadata)
        {
            if (!controlMetadata.IsContentAllowed)
            {
                throw new Exception(string.Format("The content is not allowed inside the <{0}></{0}> control!", controlMetadata.Name));
            }
        }

        /// <summary>
        /// Processes the element which contains property value.
        /// </summary>
        private void ProcessElementProperty(string parentName, RedwoodProperty parentProperty, IEnumerable<RwHtmlNode> elementContent)
        {
            // the element is a property 
            if (IsTemplateProperty(parentProperty))
            {
                // template
                var templateName = ProcessTemplate(elementContent);
                emitter.EmitSetValue(parentName, parentProperty.DescriptorFullName, templateName);
            }
            else if (IsCollectionProperty(parentProperty))
            {
                // collection of elements
                foreach (var child in FilterNodes<RwHtmlElementNode>(elementContent))
                {
                    var childObject = ProcessObjectElement(child);
                    emitter.EmitAddCollectionItem(parentName, childObject, parentProperty.Name);
                }
            }
            else if (parentProperty.DeclaringType == typeof(string))
            {
                // string property
                var strings = FilterNodes<RwHtmlLiteralNode>(elementContent);
                var value = string.Concat(strings.Select(s => s.Value));
                emitter.EmitSetValue(parentName, parentProperty.DescriptorFullName, emitter.EmitValue(value));
            }
            else if (IsControlProperty(parentProperty))
            {
                // new object
                var children = FilterNodes<RwHtmlElementNode>(elementContent).ToList();
                if (children.Count > 1)
                {
                    throw new NotSupportedException(string.Format("The property {0} can have only one child element!", parentProperty.MarkupOptions.Name)); // TODO: exception handling
                }
                else if (children.Count == 1)
                {
                    var childObject = ProcessObjectElement(children[0]);
                    emitter.EmitSetValue(parentName, parentProperty.DescriptorFullName, childObject);
                }
                else
                {
                    emitter.EmitSetValue(parentName, parentProperty.DescriptorFullName, emitter.EmitIdentifier("null"));
                }
            }
            else throw new NotSupportedException($"property type { parentProperty.DeclaringType.FullName } is not supported");
        }

        private RedwoodProperty FindProperty(ControlResolverMetadata parentMetadata, string name)
        {
            return parentMetadata.FindProperty(name) ?? RedwoodProperty.ResolveProperty(name);
        }

        /// <summary>
        /// Gets the inner property elements and makes sure that no other content is present.
        /// </summary>
        private IEnumerable<TNode> FilterNodes<TNode>(IEnumerable<RwHtmlNode> nodes)
            where TNode : RwHtmlNode
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
        /// Processes the template.
        /// </summary>
        private string ProcessTemplate(IEnumerable<RwHtmlNode> elementContent)
        {
            var templateName = emitter.EmitCreateObject(typeof(DelegateTemplate));
            emitter.EmitSetProperty(
                templateName,
                ReflectionUtils.GetPropertyNameFromExpression<DelegateTemplate>(t => t.BuildContentBody),
                emitter.EmitIdentifier(CompileTemplate(elementContent)));
            return templateName;
        }

        /// <summary>
        /// Compiles the template.
        /// </summary>
        private string CompileTemplate(IEnumerable<RwHtmlNode> elementContent)
        {
            var methodName = DefaultViewCompilerCodeEmitter.BuildTemplateFunctionName + currentTemplateIndex;
            currentTemplateIndex++;
            emitter.PushNewMethod(methodName);

            // build the statements
            var wrapperType = typeof(Placeholder);
            var parentName = emitter.EmitCreateObject(wrapperType);
            foreach (var child in elementContent)
            {
                object[] activationParameters;
                ProcessNode(child, parentName, controlResolver.ResolveControl(RedwoodConfiguration.RedwoodControlTagPrefix, typeof(Placeholder).Name, out activationParameters));
            }
            emitter.EmitReturnClause(parentName);
            emitter.PopMethod();
            return methodName;
        }

        /// <summary>
        /// Processes the HTML element that represents a new object.
        /// </summary>
        private string ProcessObjectElement(RwHtmlElementNode element)
        {
            object[] constructorParameters;
            string currentObjectName;

            var controlMetadata = controlResolver.ResolveControl(element.TagPrefix, element.TagName, out constructorParameters);
            if (controlMetadata.ControlBuilderType == null)
            {
                // compiled control
                currentObjectName = emitter.EmitCreateObject(controlMetadata.Type, constructorParameters);
            }
            else
            {
                // markup control
                currentObjectName = emitter.EmitInvokeControlBuilder(controlMetadata.Type, controlMetadata.VirtualPath);
            }
            emitter.EmitSetAttachedProperty(currentObjectName, typeof(Internal).FullName, Internal.UniqueIDProperty.Name, currentObjectName);

            // set properties from attributes
            foreach (var attribute in element.Attributes)
            {
                ProcessAttribute(attribute, controlMetadata, currentObjectName);
            }

            ProcessControlContent(element.Content, currentObjectName, controlMetadata);
            return currentObjectName;
        }

        public void ProcessControlContent(IEnumerable<RwHtmlNode> nodes, string parentName, ControlResolverMetadata metadata)
        {
            var content = new List<RwHtmlNode>();
            bool properties = true;
            foreach (var node in nodes)
            {
                var element = node as RwHtmlElementNode;
                if (element != null && properties)
                {
                    var property = FindProperty(metadata, element.TagName);
                    if (property != null && string.IsNullOrEmpty(element.TagPrefix) && property.MarkupOptions.MappingMode == MappingMode.InnerElement)
                    {
                        content.Clear();
                        ProcessElementProperty(parentName, property, element.Content);
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
            if (content.Any(RwHtmlNodeHelper.IsNotEmpty))
            {
                if (metadata.DefaultContentProperty != null)
                {
                    ProcessElementProperty(parentName, metadata.DefaultContentProperty, content);
                }
                else
                {
                    foreach (var node in content)
                    {
                        ProcessNode(node, parentName, metadata);
                    }
                }
            }
        }


        /// <summary>
        /// Processes the HTML attribute.
        /// </summary>
        private void ProcessAttribute(RwHtmlAttributeNode attribute, ControlResolverMetadata controlMetadata, string currentObjectName)
        {
            if (!string.IsNullOrEmpty(attribute.AttributePrefix))
            {
                throw new NotSupportedException("Attributes with XML namespaces are not supported!"); // TODO: exception handling
            }

            // find the property
            var property = FindProperty(controlMetadata, attribute.AttributeName);
            if (property != null)
            {
                // set the property
                if(attribute.Literal == null)
                {
                    throw new NotSupportedException("empty attributes are not supported in RedwoodPrperties");
                }
                else if (attribute.Literal is RwHtmlBindingNode)
                {
                    // binding
                    var binding = (RwHtmlBindingNode)attribute.Literal;
                    var bindingObjectName = emitter.EmitCreateObject(controlResolver.ResolveBinding(binding.Name), new object[] { attribute.Literal.Value });
                    emitter.EmitSetBinding(currentObjectName, property.DescriptorFullName, bindingObjectName);
                }
                else
                {
                    // hard-coded value in markup
                    var value = ReflectionUtils.ConvertValue(attribute.Literal.Value, property.PropertyType);
                    emitter.EmitSetValue(currentObjectName, property.DescriptorFullName, emitter.EmitValue(value));
                }
            }
            else if (controlMetadata.HasHtmlAttributesCollection)
            {
                // if the property is not found, add it as an HTML attribute
                if (attribute.Literal is RwHtmlBindingNode)
                {
                    var binding = (RwHtmlBindingNode)attribute.Literal;
                    var bindingObjectName = emitter.EmitCreateObject(controlResolver.ResolveBinding(binding.Name), new object[] { attribute.Literal.Value });
                    emitter.EmitAddHtmlAttribute(currentObjectName, attribute.AttributeName, emitter.EmitIdentifier(bindingObjectName));
                }
                else
                {
                    emitter.EmitAddHtmlAttribute(currentObjectName, attribute.AttributeName, attribute.Literal?.Value);
                }
            }
            else
            {
                // TODO: exception handling
                throw new NotSupportedException(string.Format("The control {0} does not have a property {1}!", controlMetadata.Type, attribute.AttributeName));
            }
        }



        private static bool IsCollectionProperty(RedwoodProperty parentProperty)
        {
            return parentProperty.PropertyType.GetInterfaces().Contains(typeof(ICollection));
        }

        private static bool IsTemplateProperty(RedwoodProperty parentProperty)
        {
            return parentProperty.PropertyType == typeof(ITemplate);
        }

        private static bool IsControlProperty(RedwoodProperty property)
        {
            return typeof(RedwoodControl).IsAssignableFrom(property.PropertyType);
        }
    }
}