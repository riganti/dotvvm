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

        public DefaultViewCompiler(IControlResolver controlResolver, RedwoodConfiguration configuration, CompiledAssemblyCache assemblyCache)
        {
            this.controlResolver = controlResolver;
            this.configuration = configuration;
            this.assemblyCache = assemblyCache;
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
                var wrapperType = ResolveWrapperType(node);
                var metadata = controlResolver.ResolveControl(wrapperType);

                // build the statements
                emitter.PushNewMethod("BuildControl");
                var pageName = emitter.EmitCreateObject(wrapperType);
                emitter.EmitSetAttachedProperty(pageName, typeof(Internal).FullName, Internal.UniqueIDProperty.Name, pageName);
                foreach (var child in node.Content)
                {
                    ProcessNode(child, pageName, metadata);
                }

                var directivesToApply = node.Directives.Where(d => d.Name != Constants.BaseTypeDirective).ToList();
                if (wrapperType.IsAssignableFrom(typeof (RedwoodView)))
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
        private Type ResolveWrapperType(RwHtmlRootNode node)
        {
            var wrapperType = typeof (RedwoodView);
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
                var dynamicReferences = emitter.UsedControlBuilderTypes.Select(t => t.Assembly).Distinct()
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
                    throw new Exception("The compilation failed!"); // TODO: exception handling
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
                var currentObjectName = emitter.EmitCreateObject(typeof(Literal), new object[] { ((RwHtmlLiteralNode)node).Value });
                emitter.EmitAddCollectionItem(parentName, currentObjectName);
            }
            else if (node is RwHtmlElementNode)
            {
                // HTML element
                var element = (RwHtmlElementNode)node;
                var parentProperty = FindProperty(parentMetadata, element.TagName);
                if (parentProperty != null && string.IsNullOrEmpty(element.TagPrefix) && parentProperty.MarkupOptions.MappingMode == MappingMode.InnerElement)
                {
                    // the element is a property 
                    if (IsTemplateProperty(parentProperty))
                    {
                        // template
                        var templateName = ProcessTemplate(element);
                        emitter.EmitSetValue(parentName, parentProperty.DescriptorFullName, templateName);
                    }
                    else if (IsCollectionProperty(parentProperty))
                    {
                        // collection of elements
                        foreach (var child in GetInnerPropertyElements(element, parentProperty))
                        {
                            var childObject = ProcessObjectElement(child);
                            emitter.EmitAddCollectionItem(parentName, childObject, parentProperty.Name);
                        }
                    }
                    else
                    {
                        // new object
                        var children = GetInnerPropertyElements(element, parentProperty).ToList();
                        if (children.Count > 1)
                        {
                            throw new NotSupportedException(string.Format("The property {0} can have only one child element!", parentProperty.MarkupOptions.Name));   // TODO: exception handling
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
                }
                else
                {
                    // the element is the content
                    var currentObjectName = ProcessObjectElement(element);
                    emitter.EmitAddCollectionItem(parentName, currentObjectName);
                }
            }
            else
            {
                throw new NotSupportedException();      // TODO: exception handling
            }
        }

        private RedwoodProperty FindProperty(ControlResolverMetadata parentMetadata, string name)
        {
            return parentMetadata.FindProperty(name) ?? RedwoodProperty.ResolveProperty(name);
        }

        /// <summary>
        /// Gets the inner property elements and makes sure that no other content is present.
        /// </summary>
        private IEnumerable<RwHtmlElementNode> GetInnerPropertyElements(RwHtmlElementNode element, RedwoodProperty parentProperty)
        {
            foreach (var child in element.Content)
            {
                if (child is RwHtmlElementNode)
                {
                    yield return (RwHtmlElementNode)child;
                }
                else if (child is RwHtmlLiteralNode && string.IsNullOrWhiteSpace(((RwHtmlLiteralNode)child).Value))
                {
                    continue;
                }
                throw new NotSupportedException("Content be inside collection inner property!"); // TODO: exception handling
            }
        }


        /// <summary>
        /// Processes the template.
        /// </summary>
        private string ProcessTemplate(RwHtmlElementNode element)
        {
            var templateName = emitter.EmitCreateObject(typeof(DelegateTemplate));
            emitter.EmitSetProperty(
                templateName,
                ReflectionUtils.GetPropertyNameFromExpression<DelegateTemplate>(t => t.BuildContentBody),
                emitter.EmitIdentifier(CompileTemplate(element)));
            return templateName;
        }

        /// <summary>
        /// Compiles the template.
        /// </summary>
        private string CompileTemplate(RwHtmlElementNode element)
        {
            var methodName = "BuildTemplate" + currentTemplateIndex;
            currentTemplateIndex++;
            emitter.PushNewMethod(methodName);

            // build the statements
            var wrapperType = typeof(Placeholder);
            var parentName = emitter.EmitCreateObject(wrapperType);
            foreach (var child in element.Content)
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
                currentObjectName = emitter.EmitInvokeControlBuilder(controlMetadata.Type, controlMetadata.ControlBuilderType);
            }
            emitter.EmitSetAttachedProperty(currentObjectName, typeof(Internal).FullName, Internal.UniqueIDProperty.Name, currentObjectName);

            // set properties from attributes
            foreach (var attribute in element.Attributes)
            {
                ProcessAttribute(attribute, controlMetadata, currentObjectName);
            }

            // process inner elements
            foreach (var child in element.Content)
            {
                ProcessNode(child, currentObjectName, controlMetadata);
            }
            return currentObjectName;
        }

        /// <summary>
        /// Processes the HTML attribute.
        /// </summary>
        private void ProcessAttribute(RwHtmlAttributeNode attribute, ControlResolverMetadata controlMetadata, string currentObjectName)
        {
            if (!string.IsNullOrEmpty(attribute.Prefix))
            {
                throw new NotSupportedException("Attributes with XML namespaces are not supported!"); // TODO: exception handling
            }

            // find the property
            var property = FindProperty(controlMetadata, attribute.Name);
            if (property != null)
            {
                // set the property
                if (attribute.Literal is RwHtmlBindingNode)
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
                    emitter.EmitAddHtmlAttribute(currentObjectName, attribute.Name, emitter.EmitIdentifier(bindingObjectName));
                }
                else
                {
                    emitter.EmitAddHtmlAttribute(currentObjectName, attribute.Name, attribute.Literal.Value);
                }
            }
            else
            {
                // TODO: exception handling
                throw new NotSupportedException(string.Format("The control {0} does not have a property {1}!", controlMetadata.Type, attribute.Name));
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
    }
}