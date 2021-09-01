using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotVVM.Framework.Compilation.Javascript;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    [Flags]
    public enum ControlLifecycleRequirements : short
    {
        None = 0,
        RealtimePreInit = 1 << 0,
        RealtimeInit = 1 << 1,
        RealtimeLoad = 1 << 2,
        RealtimePreRender = 1 << 3,
        RealtimePreRenderComplete = 1 << 4,
        InvokeMissingPreInit = 1 << 5,
        InvokeMissingInit = 1 << 6,
        InvokeMissingLoad = 1 << 7,
        InvokeMissingPreRender = 1 << 8,
        InvokeMissingPreRenderComplete = 1 << 9,

        OnlyRealtime = RealtimePreInit | RealtimeInit | RealtimeLoad | RealtimePreRender | RealtimePreRenderComplete,
        OnlyMissing = InvokeMissingPreInit | InvokeMissingInit | InvokeMissingLoad | InvokeMissingPreRender | InvokeMissingPreRenderComplete,
        All = OnlyRealtime | OnlyMissing,

        PreInit = RealtimePreInit | InvokeMissingPreInit,
        Init = RealtimeInit | InvokeMissingInit,
        Load = RealtimeLoad | InvokeMissingLoad,
        PreRender = RealtimePreRender | InvokeMissingPreRender,
        PreRenderComplete = RealtimePreRenderComplete | InvokeMissingPreRenderComplete,
    }

    /// <summary>
    /// Represents a base class for all DotVVM controls.
    /// </summary>
    public abstract class DotvvmControl : DotvvmBindableObject, IDotvvmControl
    {
        /// <summary>
        /// Gets the child controls.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public DotvvmControlCollection Children { get; private set; }

        // automatically assign requirements
        public ControlLifecycleRequirements LifecycleRequirements = ControlLifecycleRequirements.Init | ControlLifecycleRequirements.Load | ControlLifecycleRequirements.PreRender;

        /// <summary>
        /// Gets or sets the unique control ID.
        /// </summary>
        [MarkupOptions]
        public string? ID
        {
            get { return (string?)GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly DotvvmProperty IDProperty =
            DotvvmProperty.Register<string?, DotvvmControl>(c => c.ID, isValueInherited: false);

        /// <summary>
        /// Gets id of the control that will be written in 'id' attribute. Returns null if the IDProperty is not set.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public string? ClientID => (string?)GetValue(ClientIDProperty) ?? CreateAndSaveClientId();
        public static readonly DotvvmProperty ClientIDProperty
            = DotvvmProperty.Register<string?, DotvvmControl>(c => c.ClientID, null);

        string? CreateAndSaveClientId()
        {
            var id = CreateClientId();
            if (id != null) SetValue(ClientIDProperty, id);
            return (string?)id;
        }


        /// <summary>
        /// Gets or sets the client ID generation algorithm.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public ClientIDMode ClientIDMode
        {
            get { return (ClientIDMode)GetValue(ClientIDModeProperty)!; }
            set { SetValue(ClientIDModeProperty, value); }
        }

        public static readonly DotvvmProperty ClientIDModeProperty =
            DotvvmProperty.Register<ClientIDMode, DotvvmControl>(c => c.ClientIDMode, ClientIDMode.Static, isValueInherited: true);

        /// <summary>
        /// Gets or sets whether the control is included in the DOM of the page.
        /// </summary>
        /// <remarks>
        /// Essentially wraps Knockout's 'if' binding.
        /// </remarks>
        [MarkupOptions(AllowHardCodedValue = false)]
        public bool IncludeInPage
        {
            get { return (bool)GetValue(IncludeInPageProperty)!; }
            set { SetValue(IncludeInPageProperty, value); }
        }

        DotvvmControlCollection IDotvvmControl.Children => throw new NotImplementedException();

        ClientIDMode IDotvvmControl.ClientIDMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IDotvvmControl.ID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        DotvvmBindableObject? IDotvvmControl.Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public static readonly DotvvmProperty IncludeInPageProperty =
            DotvvmProperty.Register<bool, DotvvmControl>(t => t.IncludeInPage, true);

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmControl"/> class.
        /// </summary>
        public DotvvmControl()
        {
            Children = new DotvvmControlCollection(this);
        }

        /// <summary>
        /// Gets this control and all of its descendants.
        /// </summary>
        public IEnumerable<DotvvmControl> GetThisAndAllDescendants(Func<DotvvmControl, bool>? enumerateChildrenCondition = null)
        {
            // PERF: non-linear complexity
            yield return this;
            if (enumerateChildrenCondition == null || enumerateChildrenCondition(this))
            {
                foreach (var descendant in GetAllDescendants(enumerateChildrenCondition))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// Gets all descendant controls of this control.
        /// </summary>
        public IEnumerable<DotvvmControl> GetAllDescendants(Func<DotvvmControl, bool>? enumerateChildrenCondition = null)
        {
            // PERF: non-linear complexity
            foreach (var child in Children)
            {
                yield return child;

                if (enumerateChildrenCondition == null || enumerateChildrenCondition(child))
                {
                    foreach (var grandChild in child.GetAllDescendants(enumerateChildrenCondition))
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the control has only white space content.
        /// </summary>
        public bool HasOnlyWhiteSpaceContent() =>
            Children.HasOnlyWhiteSpaceContent();

        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        public virtual void Render(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            this.Children.ValidateParentsLifecycleEvents(); // debug check

            writer.SetErrorContext(this);

            if (properties.Contains(PostBack.UpdateProperty))
            {
                AddDotvvmUniqueIdAttribute();
            }

            try
            {
                RenderControl(writer, context);
            }
            catch (DotvvmControlException) { throw; }
            catch (Exception e)
            {
                throw new DotvvmControlException(this, "Error occurred in Render method", e);
            }
        }

        /// <summary>
        /// Adds 'data-dotvvm-id' attribute with generated unique id to the control. You can find control by this id using FindControlByUniqueId method.
        /// </summary>
        protected void AddDotvvmUniqueIdAttribute()
        {
            var htmlAttributes = this as IControlWithHtmlAttributes;
            if (htmlAttributes == null)
            {
                throw new DotvvmControlException(this, "Postback.Update can not be set on property which don't render html attributes.");
            }
            htmlAttributes.Attributes["data-dotvvm-id"] = GetDotvvmUniqueId();
        }

        protected struct RenderState
        {
            internal object? IncludeInPage;
            internal IValueBinding? DataContext;
            internal bool HasActives;
            internal bool HasActiveGroups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool TouchProperty(DotvvmProperty property, object? val, ref RenderState r)
        {
            if (property == DotvvmControl.IncludeInPageProperty)
                r.IncludeInPage = val;
            else if (property == DotvvmControl.DataContextProperty)
                r.DataContext = val as IValueBinding;
            else if (property is ActiveDotvvmProperty)
                r.HasActives = true;
            else if (property is GroupedDotvvmProperty groupedProperty && groupedProperty.PropertyGroup is ActiveDotvvmPropertyGroup)
                r.HasActiveGroups = true;
            else return false;
            return true;
        }
        /// <returns>true means that rendering of the rest of this control should be skipped</returns>
        protected bool RenderBeforeControl(in RenderState r, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (r.IncludeInPage != null && !(r.IncludeInPage is IValueBinding) && !this.IncludeInPage)
                return true;

            if (r.DataContext != null)
            {
                var parent = Parent ?? throw new DotvvmControlException(this, "Can not set DataContext binding on the root control");
                writer.WriteKnockoutWithComment(r.DataContext.GetKnockoutBindingExpression(parent));
            }

            if (r.IncludeInPage != null && r.IncludeInPage is IValueBinding binding)
            {
                writer.WriteKnockoutDataBindComment("if", binding.GetKnockoutBindingExpression(this));
            }

            if (r.HasActives) foreach (var item in properties)
            {
                if (item.Key is ActiveDotvvmProperty activeProp)
                {
                    activeProp.AddAttributesToRender(writer, context, this);
                }
            }

            if (r.HasActiveGroups)
            {
                var groups = properties
                    .Where(p => p.Key is GroupedDotvvmProperty gp && gp.PropertyGroup is ActiveDotvvmPropertyGroup)
                    .GroupBy(p => ((GroupedDotvvmProperty)p.Key).PropertyGroup);
                foreach (var item in groups)
                {
                    ((ActiveDotvvmPropertyGroup)item.Key).AddAttributesToRender(writer, context, this, item.Select(i => i.Key));
                }
            }

            return false;
        }

        protected void RenderAfterControl(in RenderState r, IHtmlWriter writer)
        {
            if (r.DataContext != null)
            {
                writer.WriteKnockoutDataBindEndComment();
            }

            if (r.IncludeInPage != null && r.IncludeInPage is IValueBinding binding)
            {
                writer.WriteKnockoutDataBindEndComment();
            }
        }


        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        protected virtual void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderState r = default;
            foreach (var (prop, value) in properties)
                TouchProperty(prop, value, ref r);
            if (RenderBeforeControl(in r, writer, context))
                return;

            AddAttributesToRender(writer, context);
            RenderBeginTag(writer, context);
            RenderContents(writer, context);
            RenderEndTag(writer, context);

            RenderAfterControl(in r, writer);
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected virtual void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Renders the control begin tag.
        /// </summary>
        protected virtual void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected virtual void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderChildren(writer, context);
        }

        /// <summary>
        /// Renders the control end tag.
        /// </summary>
        protected virtual void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Renders the children.
        /// </summary>
        protected void RenderChildren(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            foreach (var child in Children)
            {
                child.Render(writer, context);
            }
        }

        [Obsolete("Use FindControlInContainer instead. Or FindControlByClientId if you want to be limited only to this container.")]
        public DotvvmControl? FindControl(string id, bool throwIfNotFound = false) => FindControlInContainer(id, throwIfNotFound);
        [Obsolete("Use FindControlInContainer instead. Or FindControlByClientId if you want to be limited only to this container.")]
        public T FindControl<T>(string id, bool throwIfNotFound = false) where T : DotvvmControl => FindControlInContainer<T>(id, throwIfNotFound);

        /// <summary>
        /// Finds a control by its ID coded in markup. Does not recurse into naming containers. Returns null if the <paramref name="throwIfNotFound" /> is false and the control is not found.
        /// </summary>
        public DotvvmControl? FindControlInContainer(string id, bool throwIfNotFound = false)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var control = GetAllDescendants(c => !IsNamingContainer(c)).SingleOrDefault(c => c.ID == id);
            if (control == null && throwIfNotFound)
            {
                throw new Exception(string.Format("The control with ID '{0}' was not found.", id));
            }
            return control;
        }

        /// <summary>
        /// Finds a control by its ID coded in markup. Does not recurse into naming containers.
        /// </summary>
        public T FindControlInContainer<T>(string id, bool throwIfNotFound = false) where T : DotvvmControl
        {
            var control = FindControlInContainer(id, throwIfNotFound);
            if (!(control is T)) // TODO: this does not work
            {
                throw new DotvvmControlException(this, $"The control with ID '{id}' was found, however it is not an instance of the desired type '{typeof(T)}'.");
            }
            return (T)control;
        }

        /// <summary>
        /// Finds a control by its ClientId - the id rendered to output html.
        /// </summary>
        public DotvvmControl? FindControlByClientId(string id, bool throwIfNotFound = false)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var control = GetAllDescendants().SingleOrDefault(c => c.ClientID == id);
            if (control == null && throwIfNotFound)
            {
                throw new Exception(string.Format("The control with ID '{0}' was not found.", id));
            }
            return control;
        }

        /// <summary>
        /// Finds a control by its ClientId - the id rendered to output html.
        /// </summary>
        public T FindControlByClientId<T>(string id, bool throwIfNotFound = false) where T : DotvvmControl
        {
            var control = FindControlByClientId(id, throwIfNotFound);
            if (!(control is T))
            {
                throw new DotvvmControlException(this, $"The control with ID '{id}' was found, however it is not an instance of the desired type '{typeof(T)}'.");
            }
            return (T)control;
        }

        /// <summary>
        /// Finds a control by its unique ID. Returns null if the control is not found.
        /// </summary>
        public DotvvmControl? FindControlByUniqueId(string controlUniqueId)
        {
            var parts = controlUniqueId.Split('_');
            DotvvmControl result = this;
            for (var i = 0; i < parts.Length; i++)
            {
                result = result.GetAllDescendants(c => !IsNamingContainer(c))
                    .SingleOrDefault(c => c.GetValue(Internal.UniqueIDProperty) as string == parts[i]);
                if (result == null)
                {
                    return null;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the naming container of the current control.
        /// </summary>
        public DotvvmControl GetNamingContainer()
        {
            var control = this;
            while (!IsNamingContainer(control) && control.Parent is DotvvmControl parent)
            {
                control = parent;
            }
            return control;
        }

        /// <summary>
        /// Determines whether the specified control is a naming container.
        /// </summary>
        public static bool IsNamingContainer(DotvvmBindableObject control)
        {
            return (bool)control.GetValue(Internal.IsNamingContainerProperty)!;
        }

        /// <summary>
        /// Occurs after the viewmodel tree is complete.
        /// </summary>
        internal virtual void OnPreInit(IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Called right before the rendering shall occur.
        /// </summary>
        internal virtual void OnPreRenderComplete(IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Occurs before the viewmodel is applied to the page.
        /// </summary>
        protected internal virtual void OnInit(IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Occurs after the viewmodel is applied to the page IHtmlWriter writer and before the commands are executed.
        /// </summary>
        protected internal virtual void OnLoad(IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Occurs after the page commands are executed.
        /// </summary>
        protected internal virtual void OnPreRender(IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Gets the internal unique ID of the control. Returns either string or IValueBinding.
        /// </summary>
        public object GetDotvvmUniqueId() =>
            // build the client ID
            JoinValuesOrBindings(GetUniqueIdFragments());

        private object JoinValuesOrBindings(IList<object?> fragments)
        {
            if (fragments.All(f => f is string))
            {
                return string.Join("_", fragments);
            }
            else
            {
                BindingCompilationService? service = null;
                var result = new ParametrizedCode.Builder();
                var first = true;
                foreach (var f in fragments)
                {
                    if (!first | (first = false))
                        result.Add("+'_'+");
                    if (f is IValueBinding binding)
                    {
                        service = service ?? binding.GetProperty<BindingCompilationService>(ErrorHandlingMode.ReturnNull);
                        result.Add(binding.GetParametrizedKnockoutExpression(this, unwrapped: true), 14);
                    }
                    else result.Add(JavascriptCompilationHelper.CompileConstant(f));
                }
                if (service == null) throw new NotSupportedException();
                return ValueBindingExpression.CreateBinding<string?>(service.WithoutInitialization(), h => null, result.Build(new OperatorPrecedence()), this.GetDataContextType());
            }
        }

        /// <summary>
        /// Adds the corresponding attribute for the Id property.
        /// </summary>
        protected virtual object? CreateClientId() =>
            !this.IsPropertySet(IDProperty) ? null :
                // build the client ID
                GetClientIdFragments()?.Apply(JoinValuesOrBindings);

        private IList<object?> GetUniqueIdFragments()
        {
            var fragments = new List<object?> { GetValue(Internal.UniqueIDProperty) };
            foreach (var ancestor in GetAllAncestors())
            {
                if (IsNamingContainer(ancestor))
                {
                    fragments.Add(ancestor.GetValueRaw(Internal.ClientIDFragmentProperty) ?? ancestor.GetValueRaw(Internal.UniqueIDProperty));
                }
            }
            fragments.Reverse();
            return fragments;
        }

        private IList<object?>? GetClientIdFragments()
        {
            var rawId = GetValue(IDProperty);
            // can't generate ID from nothing
            if (rawId == null) return null;

            if (ClientIDMode == ClientIDMode.Static)
            {
                // just rewrite Static mode ID
                return new[] { GetValueRaw(IDProperty) };
            }

            var fragments = new List<object?> { rawId };
            DotvvmControl? childContainer = null;
            bool searchingForIdElement = false;
            foreach (DotvvmControl ancestor in GetAllAncestors())
            {
                if (IsNamingContainer(ancestor))
                {
                    if (searchingForIdElement)
                    {
                        fragments.Add(childContainer!.GetDotvvmUniqueId());
                    }
                    searchingForIdElement = false;

                    var clientIdExpression = ancestor.GetValueRaw(Internal.ClientIDFragmentProperty);
                    if (clientIdExpression is IValueBinding)
                    {
                        fragments.Add(clientIdExpression);
                    }
                    else if (ancestor.GetValueRaw(IDProperty) is object ancestorId && "" != ancestorId as string)
                    {
                        // add the ID fragment
                        fragments.Add(ancestorId);
                    }
                    else
                    {
                        searchingForIdElement = true;
                        childContainer = ancestor;
                    }
                }

                if (searchingForIdElement && ancestor.IsPropertySet(ClientIDProperty))
                {
                    fragments.Add(ancestor.GetValueRaw(ClientIDProperty));
                    searchingForIdElement = false;
                }

                if (ancestor.ClientIDMode == ClientIDMode.Static)
                {
                    break;
                }
            }
            if (searchingForIdElement)
            {
                fragments.Add(childContainer!.GetDotvvmUniqueId());
            }
            fragments.Reverse();
            return fragments;
        }


        /// <summary>
        /// Verifies that the control contains only a plain text content and tries to extract it.
        /// </summary>
        protected bool TryGetTextContent(out string textContent)
        {
            textContent = string.Join(string.Empty, Children.OfType<RawLiteral>().Where(l => !l.IsWhitespace).Select(l => l.UnencodedText));
            return Children.All(c => c is RawLiteral);
        }

        public override IEnumerable<DotvvmBindableObject> GetLogicalChildren()
        {
            return Children;
        }

        IEnumerable<DotvvmBindableObject> IDotvvmControl.GetAllAncestors(bool includingThis) => this.GetAllAncestors(includingThis);
    }
}
