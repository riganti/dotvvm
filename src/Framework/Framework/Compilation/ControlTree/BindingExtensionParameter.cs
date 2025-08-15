using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.ControlTree
{
    /// Base class for defining an extension parameter.
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$typeSerialized", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
    [JsonDerivedType(typeof(CurrentMarkupControlExtensionParameter))]
    [JsonDerivedType(typeof(CurrentCollectionIndexExtensionParameter))]
    [JsonDerivedType(typeof(BindingPageInfoExtensionParameter))]
    [JsonDerivedType(typeof(BindingCollectionInfoExtensionParameter))]
    [JsonDerivedType(typeof(InjectedServiceExtensionParameter))]
    [JsonDerivedType(typeof(BindingApiExtensionParameter))]
    [JsonDerivedType(typeof(JsExtensionParameter))]
    [JsonDerivedType(typeof(CurrentUserExtensionParameter))]
    [JsonDerivedType(typeof(JavascriptTranslationVisitor.FakeExtensionParameter))]
    [JsonDerivedType(typeof(Configuration.RestApiRegistrationHelpers.ApiExtensionParameter))]
    public abstract class BindingExtensionParameter
    {
        [JsonInclude]
        [JsonPropertyName("$type")]
        internal Type JsonHackThisType => this.GetType();

        /// A name that will be used in binding expressions to reference this parameter
        public string Identifier { get; }
        /// Type of the parameter. When used in a binding, the expression will have this type.
        public ITypeDescriptor ParameterType { get; }
        /// When the extension parameter is introduced in a specific data context, this parameter controls if the parameter will also be valid in child data contexts.
        public bool Inherit { get; }

        public BindingExtensionParameter(string identifier, ITypeDescriptor? type, bool inherit)
        {
            this.Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            this.ParameterType = type ?? ResolvedTypeDescriptor.Create(typeof(object));
            this.Inherit = inherit;
        }

        /// Returns an expression that is evaluated when value of this parameter is needed when running on server
        public abstract Expression GetServerEquivalent(Expression controlParameter);
        /// Returns a JS expression that is put into the emitted JS code on the place of the parameter
        public abstract JsExpression GetJsTranslation(JsExpression dataContext);

        public override bool Equals(object? obj) =>
            obj is BindingExtensionParameter other && Equals(other);

        public virtual bool Equals(BindingExtensionParameter other) =>
            string.Equals(Identifier, other.Identifier) && Inherit == other.Inherit && ParameterType.IsEqualTo(other.ParameterType);

        public override int GetHashCode() =>
            unchecked(((Identifier?.GetHashCode() ?? 0) * 397) ^ (Inherit.GetHashCode() * 17) + ParameterType.FullName.GetHashCode());
    }

    public class CurrentMarkupControlExtensionParameter : BindingExtensionParameter
    {
        public CurrentMarkupControlExtensionParameter(ITypeDescriptor controlType) : base("_control", controlType, true)
        {
        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.Convert(ExpressionUtils.Replace((DotvvmBindableObject c) => c.GetClosestControlBindingTarget(), controlParameter), ResolvedTypeDescriptor.ToSystemType(ParameterType));
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return dataContext.Member("$control").WithAnnotation(new ViewModelInfoAnnotation(ResolvedTypeDescriptor.ToSystemType(this.ParameterType), isControl: true, containsObservables: true));
        }

        public static CurrentMarkupControlExtensionParameter refserializer_create(ITypeDescriptor parameterType) => new CurrentMarkupControlExtensionParameter(parameterType);
    }

    public class CurrentCollectionIndexExtensionParameter : BindingExtensionParameter
    {
        public CurrentCollectionIndexExtensionParameter() : base("_index", new ResolvedTypeDescriptor(typeof(int)), true)
        {

        }

        internal static int GetIndex(DotvvmBindableObject c)
        {
            c.NotNull("control is null, is the binding executed in the right data context?");
            for (var ancestor = c; ancestor != null; ancestor = ancestor.Parent)
            {
                if (ancestor is DataItemContainer container)
                {
                    return container.DataItemIndex ?? throw new DotvvmControlException(c, "Nearest DataItemContainer does have the collection index specified.");
                }
            }
            throw new DotvvmControlException(c, "Could not find ancestor DataItemContainer that stores the current collection index.");
        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return ExpressionUtils.Replace((DotvvmBindableObject c) => GetIndex(c), controlParameter);
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return dataContext.Member("$index").Invoke();
        }
    }

    public class BindingPageInfoExtensionParameter : BindingExtensionParameter
    {
        public BindingPageInfoExtensionParameter() : base("_page", new ResolvedTypeDescriptor(typeof(BindingPageInfo)), true)
        {

        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.New(typeof(BindingPageInfo));
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return new JsObjectExpression(
                new JsObjectProperty(nameof(BindingPageInfo.EvaluatingOnClient), new JsLiteral(true)),
                new JsObjectProperty(nameof(BindingPageInfo.EvaluatingOnServer), new JsLiteral(false)),
                new JsObjectProperty(nameof(BindingPageInfo.IsPostbackRunning), new JsIdentifierExpression("dotvvm").Member("isPostbackRunning").Invoke())
            );
        }
    }

    public class BindingCollectionInfoExtensionParameter : BindingExtensionParameter
    {
        public BindingCollectionInfoExtensionParameter(string identifier) : base(identifier, new ResolvedTypeDescriptor(typeof(BindingCollectionInfo)), true)
        {
        }

        public override Expression GetServerEquivalent(Expression controlParameter) =>
            ExpressionUtils.Replace((DotvvmBindableObject c) => new BindingCollectionInfo(CurrentCollectionIndexExtensionParameter.GetIndex(c)), controlParameter);

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            JsExpression index() => dataContext.Clone().Member("$index").Invoke();
            return new JsObjectExpression(
                new JsObjectProperty(nameof(BindingCollectionInfo.Index), index()),
                new JsObjectProperty(nameof(BindingCollectionInfo.IsFirst), new JsBinaryExpression(index(), BinaryOperatorType.Equal, new JsLiteral(0))),
                new JsObjectProperty(nameof(BindingCollectionInfo.IsOdd), new JsBinaryExpression(new JsBinaryExpression(index(), BinaryOperatorType.Modulo, new JsLiteral(2)), BinaryOperatorType.Equal, new JsLiteral(1))),
                new JsObjectProperty(nameof(BindingCollectionInfo.IsEven), new JsBinaryExpression(new JsBinaryExpression(index(), BinaryOperatorType.Modulo, new JsLiteral(2)), BinaryOperatorType.Equal, new JsLiteral(0)))
            );
        }
    }

    public class InjectedServiceExtensionParameter : BindingExtensionParameter
    {
        public InjectedServiceExtensionParameter(string identifier, ITypeDescriptor type)
            : base(identifier, type, inherit: true) { }

        private static MethodInfo ResolveStaticCommandServiceMethod = typeof(InjectedServiceExtensionParameter).GetMethod(nameof(ResolveStaticCommandService), BindingFlags.NonPublic | BindingFlags.Static)!;

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            var type = ResolvedTypeDescriptor.ToSystemType(this.ParameterType);
            return Expression.Call(
                ResolveStaticCommandServiceMethod.MakeGenericMethod(type),
                controlParameter
            );
        }

        private static T ResolveStaticCommandService<T>(DotvvmBindableObject control)
        {
            if (control is null)
                throw new ArgumentNullException(nameof(control), "control is null, is the binding executed in the right data context?");
            var context = (IDotvvmRequestContext)control.GetValue(Internal.RequestContextProperty, true)
                .NotNull("Current control does not not have the Internal.RequestContextProperty property");
#pragma warning disable CS0618
            return (T)context.Services.GetRequiredService<IStaticCommandServiceLoader>().GetStaticCommandService(typeof(T), context);
#pragma warning restore CS0618
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            throw new InvalidOperationException($"Can't use injected services in javascript-translated bindings.");
        }
    }

    public class BindingApiExtensionParameter : BindingExtensionParameter
    {
        public BindingApiExtensionParameter() : base("_api", new ResolvedTypeDescriptor(typeof(BindingApi)), true)
        {

        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.New(typeof(BindingApi));
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return new JsObjectExpression();
        }
    }

    public class JsExtensionParameter : BindingExtensionParameter
    {
        public string? Id { get; internal set; }

        public bool IsMarkupControl { get; }

        public JsExtensionParameter(string? id, bool isMarkupControl)
            : base("_js", new ResolvedTypeDescriptor(typeof(JsBindingApi)), true)
        {
            Id = id;
            IsMarkupControl = isMarkupControl;
        }
        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.New(typeof(JsBindingApi));
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            if (Id == null)
                throw new ArgumentException($"{nameof(Id)} has not been set.");

            return new JsIdentifierExpression("dotvvm").Member("viewModules").WithAnnotation(new ViewModuleAnnotation(Id, IsMarkupControl));
        }

        public class ViewModuleAnnotation
        {
            public ViewModuleAnnotation(string id, bool isMarkupControl)
            {
                Id = id;
                IsMarkupControl = isMarkupControl;
            }
            public string Id { get; }
            public bool IsMarkupControl { get; }
        }
    }

    public class CurrentUserExtensionParameter : BindingExtensionParameter
    {
        public CurrentUserExtensionParameter() : base("_user", new ResolvedTypeDescriptor(typeof(ClaimsPrincipal)), true)
        {
        }
        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.Call(typeof(CurrentUserExtensionParameter), "GetUser", Type.EmptyTypes, controlParameter);
        }

        internal static ClaimsPrincipal? GetUser(DotvvmBindableObject control)
        {
            var context = control.NotNull("control is null, is the binding executed in the right data context?").GetValue(Internal.RequestContextProperty) as IDotvvmRequestContext;
            return context?.HttpContext?.User ?? new ClaimsPrincipal();
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            throw new NotSupportedException($"_user cannot be used in value or staticCommand bindings. Use `resource` binding instead to evaluate it on the server.");
        }
    }
}
