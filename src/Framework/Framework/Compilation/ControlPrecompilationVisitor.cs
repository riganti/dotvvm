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
using FastExpressionCompiler;

namespace DotVVM.Framework.Compilation
{
    /// <summary> Evaluates GetContents method on composite controls with <see cref="ControlMarkupOptionsAttribute.Precompile" /> set. </summary>
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
            placeholderMetadata = new(() => controlResolver.ResolveControl(new ResolvedTypeDescriptor(typeof(PrecompiledControlPlaceholder))));
            this.config = services.GetRequiredService<DotvvmConfiguration>();
        }


        public override void VisitControl(ResolvedControl control)
        {
            var mode = control.Metadata.PrecompilationMode;
            if (mode == ControlPrecompilationMode.Never)
            {
                base.VisitControl(control);
                return;
            }

            var type = control.Metadata.Type;

            if (!typeof(CompositeControl).IsAssignableFrom(type))
            {
                throw new DotvvmCompilationException($"Control {type.ToCode(stripNamespace: true)} cannot be precompiled, because it does not extend CompositeControl. Set ControlMarkupOptionsAttribute.Precompile to None.", control.DothtmlNode?.Tokens);
            }

            Debug.Assert(control.Metadata.PrecompilationMode != ControlPrecompilationMode.InServerSideStyles, "A control PrecompilationMode.InServerSideStyles should not appear here, it's supposed to be evaluated and removed in style evaluation.");
            try
            {
                var replacement = Precompile(control, mode, services);

                if (replacement is not null)
                {
                    // Replace the control with PrecompiledControlPlaceholder, all properties are left as-is.
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
                if (mode is ControlPrecompilationMode.Always or ControlPrecompilationMode.InServerSideStyles)
                {
                    var error = $"Failed to precompile control {type.ToCode(stripNamespace: true)}, precompilation is mandatory but the control has attempted to skip it: {ex.Message}";
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
                    // exception is ignored, but placing a warning won't hurt
                    var error = $"Control precompilation of '{type.ToCode(stripNamespace: true)}' failed, but it's in the IfPossibleAndIgnoreExceptions mode: {ex.Message}";
                    control.DothtmlNode?.AddWarning(error);
                }
                else
                {
                    var error = $"Failed to precompile control '{type.ToCode(stripNamespace: true)}': {ex.Message}";
                    control.DothtmlNode?.AddError(error);
                    throw new DotvvmCompilationException(error, ex, control.DothtmlNode?.Tokens);
                }
            }

            base.VisitControl(control);
        }

        /// <summary> Tries to precompile the specified control. Null is returned only in "IfPossible" precompilation mode when there is a binding which cannot be passed into the control. </summary>
        internal static ResolvedControl[]? Precompile(
            ResolvedControl control,
            ControlPrecompilationMode mode,
            IServiceProvider services)
        {
            var controlInfo = CompositeControl.GetControlInfo(control.Metadata.Type);

            if (controlInfo.Properties.Contains(Internal.RequestContextProperty))
            {
                throw new Exception($"The GetContents references IDotvvmRequestContext, which is not allowed during precompilation.");
            }


            bool abortCompilation = false;

            // check that all properties which have bindings allow it, since we can not evaluate resource bindings during compilation
            void checkProperty(IControlAttributeDescriptor property, Type targetType)
            {
                if (!AllowsBindings(targetType))
                {
                    if (mode is ControlPrecompilationMode.Always or ControlPrecompilationMode.InServerSideStyles)
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
                    else
                    {
                        // properties which aren't used in the constructor, or in capabilities without a mapping are not checked
                        // it does not matter too much, if the control tries to evaluate the binding, it will crash anyway
                    }
                }
            }

            if (abortCompilation)
                return null;

            var runtimeControl = (CompositeControl)control.ToRuntimeControl(services);

            var content = runtimeControl.ExecuteGetContents(null!);

            var config = services.GetService<DotvvmConfiguration>();
            return content.Select(c => ResolvedControlHelper.FromRuntimeControl(c, control.DataContextTypeStack, config)).ToArray();
        }

        /// Returns true if we can send binding into the property without evaluating it
        /// -> true for ValueOrBinding, IBinding, or IReadOnlyDictionary{_, T}, IDictionary{_, T} where T allows bindings
        static bool AllowsBindings(Type t) =>
            t.IsValueOrBinding(out _) ||
                typeof(IBinding).IsAssignableFrom(t) ||
                t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) || t.GetGenericTypeDefinition() == typeof(IDictionary<,>)) && AllowsBindings(DotvvmCapabilityProperty.Helpers.GetDictionaryElement(t));
    }

    /// <summary> When thrown, precompilation will be skipped, even though normal exceptions are not ignored. If precompilation mode is set to Always, the compilation fails with the specified message. </summary>
    public record SkipPrecompilationException(string Message = "Precompilation not possible.", Exception? InnerException = null): RecordException(Message, InnerException);
}
