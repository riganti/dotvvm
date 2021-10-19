using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.Styles
{
    public interface IStyleApplicator
    {
        void ApplyStyle(ResolvedControl control, IStyleMatchContext context);
    }

    public sealed class MonoidStyleApplicator : IStyleApplicator
    {
        private readonly IEnumerable<IStyleApplicator> applicators;
        private MonoidStyleApplicator(IEnumerable<IStyleApplicator> applicators)
        {
            this.applicators = applicators;
        }

        public void ApplyStyle(ResolvedControl control, IStyleMatchContext context)
        {
            foreach (var a in this.applicators)
                a.ApplyStyle(control, context);
        }

        public static IStyleApplicator Empty = new MonoidStyleApplicator(Enumerable.Empty<IStyleApplicator>());
        public static IStyleApplicator Combine(IEnumerable<IStyleApplicator> applicators)
        {
            applicators = applicators.Except(new [] { Empty });
            var count = applicators.Take(2).Count();
            if (count == 0)
                return Empty;
            else if (count == 1)
                return applicators.First();
            else
                return new MonoidStyleApplicator(applicators);
        }

        public override string ToString() =>
            !applicators.Any() ? "Empty" :
            $"All[ {string.Join(" , ", applicators)} ]";
    }

    class PropertyStyleApplicator : IStyleApplicator
    {
        readonly DotvvmProperty property;
        readonly object? value;
        readonly StyleOverrideOptions options;

        public PropertyStyleApplicator(DotvvmProperty property, object? value, StyleOverrideOptions options)
        {
            this.property = property;
            this.value = value;
            this.options = options;
        }

        public void ApplyStyle(ResolvedControl control, IStyleMatchContext context)
        {
            var dataContext = property.GetDataContextType(control);
            var setter = ResolvedControlHelper.TranslateProperty(property, value, dataContext, context.Configuration);
            if (!control.SetProperty(setter, options, out var error))
                throw new DotvvmCompilationException("Can not apply style property: " + error, control.DothtmlNode?.Tokens);
        }

        public override string ToString() => $"{value} (on conflict {options})";
    }

    public class GenericPropertyStyleApplicator<T> : IStyleApplicator
    {
        readonly DotvvmProperty property;
        readonly Func<IStyleMatchContext<T>, object?> value;
        readonly StyleOverrideOptions options;

        public GenericPropertyStyleApplicator(DotvvmProperty property, Func<IStyleMatchContext<T>, object?> value, StyleOverrideOptions options)
        {
            this.property = property;
            this.value = value;
            this.options = options;
        }

        public void ApplyStyle(ResolvedControl control, IStyleMatchContext context)
        {
            if (context.IsType<T>(out var c))
            {
                var dataContext = property.GetDataContextType(control);
                var v = value(c);
                var setter = ResolvedControlHelper.TranslateProperty(property, v, dataContext, context.Configuration);
                if (!control.SetProperty(setter, options, out var error))
                    throw new DotvvmCompilationException("Can not apply style property: " + error, control.DothtmlNode?.Tokens);
            }
        }
        public override string ToString() => $"GenericPropertyStyleApplicator {property} (on conflict {options})";
    }

    internal class PropertyStyleBindingApplicator : IStyleApplicator
    {
        private readonly DotvvmProperty property;
        private readonly string binding;
        private readonly StyleOverrideOptions options;
        private readonly BindingParserOptions bindingOptions;
        private readonly bool allowChangingBindingType;

        public PropertyStyleBindingApplicator(DotvvmProperty property, string binding, StyleOverrideOptions options, BindingParserOptions bindingOptions, bool allowChangingBindingType = false)
        {
            if (property == DotvvmBindableObject.DataContextProperty)
                throw new NotSupportedException("Can not set the DataContext property using styles. This property affects the compilation itself, and styles are applied after that.");
            if (!property.MarkupOptions.AllowBinding && bindingOptions.BindingType != typeof(ResourceBindingExpression))
                throw new Exception($"Property {property} does not allow bindings to be set. You could maybe use a resource binding (set bindingOptions to BindingParserOptions.Resource).");

            this.property = property;
            this.binding = binding;
            this.options = options;
            this.bindingOptions = bindingOptions;
            this.allowChangingBindingType = allowChangingBindingType;
        }

        public void ApplyStyle(ResolvedControl control, IStyleMatchContext context)
        {
            var dataContext = property.GetDataContextType(control);
            var bindingOptions = this.bindingOptions;
            if (allowChangingBindingType && control.Properties.GetValueOrDefault(property) is ResolvedPropertyBinding rb)
            {
                // when merging attributes, we need to have the same binding type
                // for example, when there already is a resource binding, we'll also put in a resource instead of failing compilation
                bindingOptions = rb.Binding.Binding.GetProperty<BindingParserOptions>();
            }
            var b = new ResolvedBinding(
                context.Configuration.ServiceProvider.GetRequiredService<BindingCompilationService>(),
                bindingOptions,
                dataContext,
                code: binding,
                property: property
            );
            if (!control.SetProperty(new ResolvedPropertyBinding(property, b), options, out var error))
                throw new DotvvmCompilationException("Can not apply style property binding: " + error, control.DothtmlNode?.Tokens);
        }

        public override string ToString() => $"{property}={{{bindingOptions.BindingType.Name}: {binding}}} (on conflict {options})";
    }

    internal class PropertyControlCollectionStyleApplicator : IStyleApplicator
    {

        readonly DotvvmProperty property;
        readonly DotvvmBindableObject prototypeControl;
        readonly IStyle innerControlStyle;
        readonly StyleOverrideOptions options;

        public PropertyControlCollectionStyleApplicator(
            DotvvmProperty property,
            StyleOverrideOptions options,
            DotvvmBindableObject prototypeControl,
            IStyle innerControlStyle)
        {
            this.property = property;
            this.options = options;
            this.prototypeControl = prototypeControl;
            this.innerControlStyle = innerControlStyle;
        }


        public void ApplyStyle(ResolvedControl control, IStyleMatchContext context)
        {
            var dataContext = property.GetDataContextType(control);
            var innerControl = ResolvedControlHelper.FromRuntimeControl(this.prototypeControl, dataContext, context.Configuration);
            innerControl.Parent = control;
            innerControlStyle.Applicator.ApplyStyle(innerControl, new StyleMatchContext<DotvvmBindableObject>(context, innerControl, context.Configuration));

            var value = ResolvedControlHelper.TranslateProperty(property, innerControl, dataContext, context.Configuration);
            if (!control.SetProperty(value, options, out var error))
                throw new DotvvmCompilationException("Can not apply style property: " + error, control.DothtmlNode?.Tokens);
        }
        public override string ToString()
        {
            var innerStyleFmt = innerControlStyle.Applicator == MonoidStyleApplicator.Empty ? "" : "\n    " + innerControlStyle;
            var verb = options == StyleOverrideOptions.Ignore ? $"Set {property} if empty to " : $"{options} {property}: ";
            return $"{verb}{prototypeControl.DebugString()}{innerStyleFmt}";
        }
    }

    internal class ChildrenStyleApplicator : IStyleApplicator
    {
        readonly FunctionOrValue<IStyleMatchContext, IEnumerable<DotvvmBindableObject>> prototypeControls;
        readonly IStyle innerControlsStyle;
        readonly StyleOverrideOptions options;

        public ChildrenStyleApplicator(
            StyleOverrideOptions options,
            FunctionOrValue<IStyleMatchContext, IEnumerable<DotvvmBindableObject>> prototypeControls,
            IStyle innerControlsStyle)
        {
            this.options = options;
            this.prototypeControls = prototypeControls;
            this.innerControlsStyle = innerControlsStyle;
        }

        public void ApplyStyle(ResolvedControl control, IStyleMatchContext context)
        {
            if (!context.AllowsContent())
                throw new NotSupportedException($"Control {control.Metadata.Name} is not allowed to have content (it was attempted to set it using Styles). If you want to apply this style only controls that can have content, use the context.AllowsContent() method in the style condition.");

            var dataContext = context.ChildrenDataContextStack();
            var innerControls = this.prototypeControls.Invoke(context).Select(c =>
                ResolvedControlHelper.FromRuntimeControl(c, dataContext, context.Configuration))
                .ToArray();
            foreach (var c in innerControls)
            {
                if (c is null)
                    continue;
                c.Parent = control;
                innerControlsStyle.Applicator.ApplyStyle(c, new StyleMatchContext<DotvvmBindableObject>(context, c, context.Configuration));
            }

            ResolvedControlHelper.SetContent(control, innerControls, options);
        }

        public override string ToString()
        {
            var innerStyleFmt = innerControlsStyle.Applicator == MonoidStyleApplicator.Empty ? "" : "\n    " + innerControlsStyle;
            var controlFmt = prototypeControls.DebugString(c => string.Join("\n", c.Select(c => c.DebugString())), "[the content is computed]");
            var verb = options == StyleOverrideOptions.Ignore ? "Set content if empty" : $"{options} children";
            return $"{verb} {controlFmt}{innerStyleFmt}";
        }
    }
}
