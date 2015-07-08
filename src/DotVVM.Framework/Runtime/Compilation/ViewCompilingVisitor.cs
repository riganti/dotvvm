using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class ViewCompilingVisitor: ResolvedControlTreeVisitor
    {
        protected readonly DefaultViewCompilerCodeEmitter emitter;

        protected int currentTemplateIndex;
        protected string className;
        protected ControlResolverMetadata lastMetadata;
        protected string controlName;

        public ViewCompilingVisitor(DefaultViewCompilerCodeEmitter emitter, string className)
        {
            this.emitter = emitter;
            this.className = className;
        }

        public override void VisitView(ResolvedView view)
        {
            lastMetadata = view.Metadata;
            var wrapperClassName = CreateControlClass(className, view.Metadata.Type);

            // build the statements
            emitter.PushNewMethod(DefaultViewCompilerCodeEmitter.BuildControlFunctionName);
            var pageName = emitter.EmitCreateObject(wrapperClassName);
            emitter.EmitSetAttachedProperty(pageName, typeof(Internal).FullName, Internal.UniqueIDProperty.Name, pageName);
            if (view.Metadata.Type.IsAssignableFrom(typeof(DotvvmView)))
            {
                foreach (var directive in view.Directives)
                {
                    emitter.EmitAddDirective(pageName, directive.Key, directive.Value);
                }
            }

            controlName = pageName;

            base.VisitView(view);

            emitter.EmitReturnClause(pageName);
            emitter.PopMethod();
        }

        /// <summary>
        /// Processes the node.
        /// </summary>
        public override void VisitControl(ResolvedControl node)
        {
            var parentName = controlName;
            controlName = CreateControl(node);

            base.VisitControl(node);

            emitter.EmitAddCollectionItem(parentName, controlName);
            controlName = parentName;
        }

        public override void VisitPropertyValue(ResolvedPropertyValue propertyValue)
        {
            emitter.EmitSetValue(controlName, propertyValue.Property.DescriptorFullName, emitter.EmitValue(propertyValue.Value));
            base.VisitPropertyValue(propertyValue);
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            emitter.EmitSetBinding(controlName, propertyBinding.Property.DescriptorFullName, ProcessBinding(propertyBinding.Binding));
            base.VisitPropertyBinding(propertyBinding);
        }

        public override void VisitPropertyControl(ResolvedPropertyControl propertyControl)
        {
            var control = propertyControl.Control;
            var parentName = controlName;
            controlName = CreateControl(control);
            // compile control content
            base.VisitControl(control);
            // set the property
            emitter.EmitSetValue(parentName, propertyControl.Property.DescriptorFullName, controlName);
            controlName = parentName;
        }

        public override void VisitPropertyControlCollection(ResolvedPropertyControlCollection propertyControlCollection)
        {
            var parentName = controlName;
            foreach (var control in propertyControlCollection.Controls)
            {
                controlName = CreateControl(control);
                // compile control content
                base.VisitControl(control);
                // add to collection in property
                emitter.EmitAddCollectionItem(parentName, controlName, propertyControlCollection.Property.Name);
            }
            controlName = parentName;
        }

        public override void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate)
        {
            var parentName = controlName;
            var methodName = DefaultViewCompilerCodeEmitter.BuildTemplateFunctionName + currentTemplateIndex;
            currentTemplateIndex++;
            emitter.PushNewMethod(methodName);
            // build the statements
            controlName = emitter.EmitCreateObject(typeof(Placeholder));
            
            base.VisitPropertyTemplate(propertyTemplate);

            emitter.EmitReturnClause(controlName);
            emitter.PopMethod();
            controlName = parentName;

            var templateName = CreateTemplate(methodName);
            emitter.EmitSetValue(controlName, propertyTemplate.Property.DescriptorFullName, templateName);
        }

        protected void ProcessHtmlAttributes(string controlName, IDictionary<string, object> attributes)
        {
            foreach (var attr in attributes)
            {
                var value = ProcessBindingOrValue(attr.Value);
                emitter.EmitAddHtmlAttribute(controlName, attr.Key, value);
            }
        }

        /// <summary>
        /// Emits value or binding and returns 
        /// </summary>
        protected ExpressionSyntax ProcessBindingOrValue(object obj)
        {
            var binding = obj as ResolvedBinding;
            if (binding != null) return SyntaxFactory.IdentifierName(ProcessBinding(binding));
            else return emitter.EmitValue(obj);
        }

        /// <summary>
        /// Emits control class definition if wrapper is DotvvmView and returns class name
        /// </summary>
        protected string CreateControlClass(string className, Type wrapperType)
        {
            if (wrapperType == typeof(DotvvmView))
            {
                var controlClassName = className + "Control";
                emitter.EmitControlClass(wrapperType, controlClassName);
                return controlClassName;
            }
            else return wrapperType.FullName;
        }


        /// <summary>
        /// Processes the HTML element that represents a new object.
        /// </summary>
        protected string CreateControl(ResolvedControl control)
        {
            string name;
            
            if (control.Metadata.ControlBuilderType == null)
            {
                // compiled control
                name = emitter.EmitCreateObject(control.Metadata.Type, control.ContructorParameters);
            }
            else
            {
                // markup control
                name = emitter.EmitInvokeControlBuilder(control.Metadata.Type, control.Metadata.VirtualPath);
            }
            emitter.EmitSetAttachedProperty(name, typeof(Internal).FullName, Internal.UniqueIDProperty.Name, name);
            if(control.HtmlAttributes != null && control.Metadata.HasHtmlAttributesCollection)
            {
                ProcessHtmlAttributes(name, control.HtmlAttributes);
            }
            return name;
        }

        /// <summary>
        /// Emits binding contructor and returns variable name
        /// </summary>
        protected string ProcessBinding(ResolvedBinding binding)
        {
            return emitter.EmitCreateObject(binding.Type, new object[] { binding.Value });
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
