using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.Binding;
using System.Diagnostics;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    public class ViewCompilingVisitor : ResolvedControlTreeVisitor
    {
        protected readonly DefaultViewCompilerCodeEmitter emitter;
        private readonly CompiledAssemblyCache compiledAssemblyCache;
        protected readonly IBindingCompiler bindingCompiler;

        protected int currentTemplateIndex;
        protected string className;
        protected ControlResolverMetadata lastMetadata;
        protected string controlName;

        public ViewCompilingVisitor(DefaultViewCompilerCodeEmitter emitter, CompiledAssemblyCache compiledAssemblyCache, IBindingCompiler bindingCompiler,
            string className)
        {
            this.emitter = emitter;
            this.compiledAssemblyCache = compiledAssemblyCache;
            this.className = className;
            this.bindingCompiler = bindingCompiler;
        }

        public override void VisitView(ResolvedTreeRoot view)
        {
            lastMetadata = view.Metadata;

            var createsCustomDerivedType = view.Metadata.Type == typeof(DotvvmView);

            if (createsCustomDerivedType)
            {
                var resultControlType = className + "Control";
                emitter.ResultControlTypeSyntax = SyntaxFactory.ParseTypeName(resultControlType);
                emitter.EmitControlClass(view.Metadata.Type, resultControlType);
            }
            else
            {
                emitter.ResultControlTypeSyntax = emitter.ParseTypeName(view.Metadata.Type);
            }

            emitter.UseType(view.Metadata.Type);
            emitter.Descriptor = view.ControlBuilderDescriptor;
            // build the statements
            emitter.PushNewMethod(nameof(IControlBuilder.BuildControl), typeof(DotvvmControl), emitter.EmitControlBuilderParameters());

            var pageName =
                createsCustomDerivedType ? emitter.EmitCreateObject(emitter.ResultControlTypeSyntax) :
                                           this.EmitCreateControl(view.Metadata.Type, new object[0]);
            emitter.RegisterDotvvmProperties(pageName);

            emitter.EmitSetDotvvmProperty(pageName, Internal.UniqueIDProperty, pageName);
            emitter.EmitSetDotvvmProperty(pageName, Internal.MarkupFileNameProperty, view.Metadata.VirtualPath);
            emitter.EmitSetDotvvmProperty(pageName, Internal.DataContextTypeProperty, emitter.EmitValue(view.DataContextTypeStack));

            if (typeof(DotvvmView).IsAssignableFrom(view.Metadata.Type))
                emitter.EmitSetProperty(pageName, nameof(DotvvmView.ViewModelType),
                    emitter.EmitValue(view.DataContextTypeStack.DataContextType));

            if (typeof(DotvvmView).IsAssignableFrom(view.Metadata.Type) ||
                typeof(DotvvmMarkupControl).IsAssignableFrom(view.Metadata.Type))
            {
                foreach (var directive in view.Directives)
                {
                    emitter.EmitAddDirective(pageName, directive.Key, directive.Value.First().Value);
                }
            }

            controlName = pageName;

            base.VisitView(view);

            emitter.CommitDotvvmProperties(pageName);

            emitter.EmitReturnClause(pageName);
            emitter.PopMethod();
        }

        private TypeSyntax ResolveTypeSyntax(string typeName)
        {
            return ReflectionUtils.IsFullName(typeName)
                ? emitter.ParseTypeName(compiledAssemblyCache.FindType(typeName))
                : SyntaxFactory.ParseTypeName(typeName);
        }

        protected string EmitCreateControl(Type type, object[] arguments)
        {
            // if marked with [RequireDependencyInjection] attribute, invoke injected factory
            if (type.GetTypeInfo().GetCustomAttribute(typeof(DependencyInjection.RequireDependencyInjectionAttribute)) is DependencyInjection.RequireDependencyInjectionAttribute requireDiAttr)
                return emitter.EmitCustomInjectionFactoryInvocation(requireDiAttr.FactoryType, type);
            // if matching ctor exists, invoke it directly
            else if (type.GetConstructors().FirstOrDefault(ctor =>
                ctor.GetParameters().Count(p => !p.HasDefaultValue) <= (arguments?.Length ?? 0) &&
                ctor.GetParameters().Length >= (arguments?.Length ?? 0) &&
                ctor.GetParameters().Zip(arguments ?? Enumerable.Empty<object>(),
                        (p, a) => TypeConversion.ImplicitConversion(Expression.Constant(a), p.ParameterType))
                    .All(a => a != null)) is ConstructorInfo constructor)
            {
                var optionalArguments =
                    constructor.GetParameters().Skip(arguments?.Length ?? 0)
                    .Select(a =>
                        a.ParameterType == typeof(bool) && a.Name == "allowImplicitLifecycleRequirements" ? false :
                        a.DefaultValue
                    );
                return emitter.EmitCreateObject(type, arguments == null ? optionalArguments.ToArray() : arguments.Concat(optionalArguments).ToArray());
            }
            // otherwise invoke DI factory
            else
                return emitter.EmitInjectionFactoryInvocation(
                    type,
                    (arguments ?? Enumerable.Empty<object>()).Select(a => (a.GetType(), emitter.EmitValue(a))).ToArray(),
                    emitter.InvokeDefaultInjectionFactory
                );
        }


        /// <summary>
        /// Processes the node.
        /// </summary>
        public override void VisitControl(ResolvedControl node)
        {
            var parentName = controlName;
            var localControlName = controlName = CreateControl(node);

            base.VisitControl(node);

            Debug.Assert(localControlName == controlName);

            emitter.CommitDotvvmProperties(controlName);

            emitter.EmitAddCollectionItem(parentName, controlName);
            controlName = parentName;
        }

        private void SetProperty(string controlName, DotvvmProperty property, ExpressionSyntax value)
        {
            // set special properties as fields
            if (property == LifecycleRequirementsAssigningVisitor.CompileTimeLifecycleRequirementsProperty)
                emitter.EmitSetProperty(controlName, nameof(DotvvmControl.LifecycleRequirements), value);

            else emitter.EmitSetDotvvmProperty(controlName, property, value);
        }

        private void SetPropertyValue(string controlName, DotvvmProperty property, object value)
            => SetProperty(controlName, property, emitter.EmitValue(value));

        public override void VisitPropertyValue(ResolvedPropertyValue propertyValue)
        {
            SetPropertyValue(controlName, propertyValue.Property, propertyValue.Value);
            base.VisitPropertyValue(propertyValue);
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            SetProperty(controlName, propertyBinding.Property, ProcessBinding(propertyBinding.Binding));
            base.VisitPropertyBinding(propertyBinding);
        }

        public override void VisitPropertyControl(ResolvedPropertyControl propertyControl)
        {
            var control = propertyControl.Control;
            var parentName = controlName;
            controlName = CreateControl(control);
            // compile control content
            base.VisitControl(control);
            emitter.CommitDotvvmProperties(controlName);
            emitter.EmitSetProperty(controlName, nameof(DotvvmControl.Parent), SyntaxFactory.IdentifierName(parentName));
            // set the property
            SetProperty(parentName, propertyControl.Property, SyntaxFactory.IdentifierName(controlName));
            controlName = parentName;
        }

        public override void VisitPropertyControlCollection(ResolvedPropertyControlCollection propertyControlCollection)
        {
            var parentName = controlName;
            var collectionName = emitter.EmitEnsureCollectionInitialized(parentName, propertyControlCollection.Property);

            foreach (var control in propertyControlCollection.Controls)
            {
                controlName = CreateControl(control);

                // compile control content
                base.VisitControl(control);

                emitter.CommitDotvvmProperties(controlName);

                // add to collection in property
                emitter.EmitSetProperty(controlName, nameof(DotvvmControl.Parent), SyntaxFactory.IdentifierName(parentName));
                emitter.EmitAddCollectionItem(collectionName, controlName, null);
            }
            controlName = parentName;
        }

        public override void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate)
        {
            var parentName = controlName;
            var methodName = DefaultViewCompilerCodeEmitter.BuildTemplateFunctionName + $"_{propertyTemplate.Property.DeclaringType.Name}_{propertyTemplate.Property.Name}_{currentTemplateIndex++}";
            emitter.PushNewMethod(methodName, typeof(void), emitter.EmitControlBuilderParameters().Concat(new [] { emitter.EmitParameter("templateContainer", typeof(DotvvmControl))}).ToArray());
            // build the statements
            controlName = "templateContainer";

            base.VisitPropertyTemplate(propertyTemplate);

            emitter.PopMethod();
            controlName = parentName;

            var templateName = CreateTemplate(methodName);
            SetProperty(controlName, propertyTemplate.Property, SyntaxFactory.IdentifierName(templateName));
        }

        /// <summary>
        /// Processes the HTML element that represents a new object.
        /// </summary>
        protected string CreateControl(ResolvedControl control)
        {
            string name;

            if (control.Metadata.VirtualPath == null)
            {
                // compiled control
                name = EmitCreateControl(control.Metadata.Type, control.ConstructorParameters);
            }
            else
            {
                // markup control
                name = emitter.EmitInvokeControlBuilder(control.Metadata.Type, control.Metadata.VirtualPath);
            }
            emitter.RegisterDotvvmProperties(name);
            // set unique id
            emitter.EmitSetDotvvmProperty(name, Internal.UniqueIDProperty, name);

            if (control.DothtmlNode != null && control.DothtmlNode.Tokens.Count > 0)
            {
                // set line number
                emitter.EmitSetDotvvmProperty(name, Internal.MarkupLineNumberProperty, control.DothtmlNode.Tokens.First().LineNumber);
            }

            return name;
        }

        /// <summary>
        /// Emits binding constructor and returns variable name
        /// </summary>
        protected ExpressionSyntax ProcessBinding(ResolvedBinding binding)
        {
            return bindingCompiler.EmitCreateBinding(emitter, binding);
        }

        /// <summary>
        /// Processes the template.
        /// </summary>
        protected string CreateTemplate(string builderMethodName)
        {
            var templateName = emitter.EmitCreateObject(typeof(DelegateTemplate));
            emitter.EmitSetProperty(templateName,
                nameof(DelegateTemplate.BuildContentBody),
                emitter.EmitIdentifier(builderMethodName));
            return templateName;
        }
    }
}
