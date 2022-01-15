using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Binding;
using System;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Utils;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Styles;
using System.Diagnostics;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Configuration;
using RecordExceptions;

namespace DotVVM.Framework.Compilation
{
    /// <summary> Evalues GetContents method on controls with <see cref="ControlMarkupOptionsAttribute.Precompile" /> set to true. </summary>
    sealed class ControlPrecompilationVisitor : ResolvedControlTreeVisitor
    {
        private readonly Lazy<IControlResolverMetadata> placeholderMetadata;
        private readonly DotvvmConfiguration config;
        private readonly IServiceProvider services;

        public ControlPrecompilationVisitor(
            IServiceProvider services)
        {
            this.services = services;
            var controlResolver = services.GetRequiredService<IControlResolver>();
            placeholderMetadata = new Lazy<IControlResolverMetadata>(() => controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(PrecompiledControlPlaceholder))));
            this.config = services.GetRequiredService<DotvvmConfiguration>();
        }


        public override void VisitControl(ResolvedControl control)
        {
            var mode = control.Metadata.PrecompilationMode;
            if (typeof(CompositeControl).IsAssignableFrom(control.Metadata.Type) && mode != ControlPrecompilationMode.Never)
            {
                var name = control.Metadata.Type.Name;
                try
                {
                    var type = control.Metadata.Type;
                    var replacement = Precompile(control, mode);

                    if (replacement is not null)
                    {

                        control.Metadata = (ControlResolverMetadata)placeholderMetadata.Value;
                        control.ConstructorParameters = new object[] { type };
                        control.Content.Clear();
                        control.Content.AddRange(replacement);

                        foreach (var c in replacement)
                            c.Parent = control;
                    }
                }
                catch (SkipPrecompilationException ex)
                {
                    if (mode == ControlPrecompilationMode.Always)
                    {
                        var error = $"Failed to precompile control {name}, precompilation is mandatory but it was skipped: {ex.Message}";
                        control.DothtmlNode?.AddError(error);
                        throw new DotvvmCompilationException(error, ex, control.DothtmlNode?.Tokens);
                    }
                    else
                    {
                        // exception is ignored
                    }
                }
                catch (Exception ex)
                {
                    if (mode == ControlPrecompilationMode.IfPossibleAndIgnoreExceptions)
                    {
                        // exception is ignored
                    }
                    else
                    {
                        var error = $"Failed to precompile control '{name}': {ex.Message}";
                        control.DothtmlNode?.AddError(error);
                        throw new DotvvmCompilationException(error, ex, control.DothtmlNode?.Tokens);
                    }
                }
            }

            base.VisitControl(control);
        }

        ResolvedControl[]? Precompile(ResolvedControl control, ControlPrecompilationMode mode)
        {
            var controlInfo = CompositeControl.GetControlInfo(control.Metadata.Type);

            if (controlInfo.Properties.Contains(Internal.RequestContextProperty))
            {
                if (mode == ControlPrecompilationMode.Always)
                    throw new Exception($"The GetContents references IDotvvmRequestContext, which is not allowed during precompilation.");
                else
                    return null;
            }


            bool abortCompilation = false;

            // check that all properties which have bindings allow it, since we can not evaluate resource bindings during compilation
            void checkProperty(IControlAttributeDescriptor property, Type targetType)
            {
                if (!AllowsBindings(targetType))
                {
                    if (mode == ControlPrecompilationMode.Always)
                        throw new Exception($"The property '{property.Name}' does not allow bindings.");
                    else
                        abortCompilation = true;
                }
            }
            var methodParams = controlInfo.GetContentsMethod.GetParameters();
            foreach (var prop in control.Properties.Values)
            {
                var descriptor = prop.Property is GroupedDotvvmProperty gp ? gp.PropertyGroup : (IControlAttributeDescriptor)prop.Property;
                if (prop is ResolvedPropertyBinding binding)
                {
                    var argIndex = controlInfo.Properties.IndexOf(descriptor);
                    if (argIndex >= 0)
                    {
                        checkProperty(binding.Property, methodParams[argIndex].ParameterType);
                    }
                    else if (prop.Property.OwningCapability is { PropertyMapping: {} } owner)
                    {
                        var field = owner.PropertyMapping.Value.FirstOrDefault(p => p.dotvvmProperty == descriptor).prop ??
                            owner.PropertyGroupMapping!.Value.FirstOrDefault(p => p.dotvvmPropertyGroup == descriptor).prop;
                        if (field is {})
                            checkProperty(binding.Property, field.PropertyType);
                    }
                }
            }

            if (abortCompilation)
                return null;

            var runtimeControl = (CompositeControl)control.ToRuntimeControl(services);

            var content = runtimeControl.ExecuteGetContents(null!);

            return content.Select(c => ResolvedControlHelper.FromRuntimeControl(c, control.DataContextTypeStack, config)).ToArray();
        }

        static bool AllowsBindings(Type t) =>
            t.IsValueOrBinding(out _) ||
                typeof(IBinding).IsAssignableFrom(t) ||
                t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) || t.GetGenericTypeDefinition() == typeof(IDictionary<,>)) && AllowsBindings(DotvvmCapabilityProperty.Helpers.GetDictionaryElement(t));
    }

    /// <summary> When thrown, precompilation will be skipped, even though normal exceptions are not ignored. If precompilation mode is set to Always, the compilation fails with the specified message. </summary>
    public record SkipPrecompilationException(string Message, Exception? InnerException = null): RecordException(Message, InnerException);
}
