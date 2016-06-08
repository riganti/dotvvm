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

namespace DotVVM.Framework.Controls
{
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

        /// <summary>
        /// Gets or sets the unique control ID.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string ID
        {
            get { return (string)GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly DotvvmProperty IDProperty =
            DotvvmProperty.Register<string, DotvvmControl>(c => c.ID, isValueInherited: false);

        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public string ClientID => (string)GetValue(ClientIDProperty) ?? CreateAndSaveClientId();
        public static readonly DotvvmProperty ClientIDProperty
            = DotvvmProperty.Register<string, DotvvmControl>(c => c.ClientID, null);

        string CreateAndSaveClientId()
        {
            var id = CreateClientId();
            if (id != null) SetValue(ClientIDProperty, id);
            return (string)id;
        }


        /// <summary>
        /// Gets or sets the client ID generation algorithm.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public ClientIDMode ClientIDMode
        {
            get { return (ClientIDMode)GetValue(ClientIDModeProperty); }
            set { SetValue(ClientIDModeProperty, value); }
        }

        public static readonly DotvvmProperty ClientIDModeProperty =
            DotvvmProperty.Register<ClientIDMode, DotvvmControl>(c => c.ClientIDMode, ClientIDMode.Static, isValueInherited: true);

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
        public IEnumerable<DotvvmControl> GetThisAndAllDescendants(Func<DotvvmControl, bool> enumerateChildrenCondition = null)
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
        public IEnumerable<DotvvmControl> GetAllDescendants(Func<DotvvmControl, bool> enumerateChildrenCondition = null)
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
        public bool HasOnlyWhiteSpaceContent()
        {
            return Children.All(c => (c is RawLiteral && ((RawLiteral)c).IsWhitespace));
        }

        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        public virtual void Render(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (Properties.ContainsKey(PostBack.UpdateProperty))
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
                throw new DotvvmControlException(this, "Error occured in Render method", e);
            }
        }

        protected void AddDotvvmUniqueIdAttribute()
        {
            var htmlAttributes = this as IControlWithHtmlAttributes;
            if (htmlAttributes == null)
            {
                throw new DotvvmControlException(this, "Postback.Update can not be set on property which don't render html attributes.");
            }
            htmlAttributes.Attributes["data-dotvvm-id"] = GetDotvvmUniqueId();
        }

        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        protected virtual void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderBeginWithDataBindAttribute(writer);

            foreach (var item in properties.Keys.ToArray())
            {
                if (item is ActiveDotvvmProperty)
                {
                    ((ActiveDotvvmProperty)item).AddAttributesToRender(writer, context, this);
                }
            }

            AddAttributesToRender(writer, context);
            RenderBeginTag(writer, context);
            RenderContents(writer, context);
            RenderEndTag(writer, context);

            RenderEndWithDataBindAttribute(writer);
        }

        private void RenderBeginWithDataBindAttribute(IHtmlWriter writer)
        {
            // if the DataContext is set, render the "with" binding
            if (HasBinding(DataContextProperty))
            {
                writer.WriteKnockoutWithComment(GetValueBinding(DataContextProperty).GetKnockoutBindingExpression());
            }
        }

        private void RenderEndWithDataBindAttribute(IHtmlWriter writer)
        {
            if (HasBinding(DataContextProperty))
            {
                writer.WriteKnockoutDataBindEndComment();
            }
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
        public DotvvmControl FindControl(string id, bool throwIfNotFound = false) => FindControlInContainer(id, throwIfNotFound);
        [Obsolete("Use FindControlInContainer instead. Or FindControlByClientId if you want to be limited only to this container.")]
        public T FindControl<T>(string id, bool throwIfNotFound = false) where T : DotvvmControl => FindControlInContainer<T>(id, throwIfNotFound);

        /// <summary>
        /// Finds the control by its ID coded in markup. Does not recurse into naming containers.
        /// </summary>
        public DotvvmControl FindControlInContainer(string id, bool throwIfNotFound = false)
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
        /// Finds the control by its ID coded in markup. Does not recurse into naming containers.
        /// </summary>
        public T FindControlInContainer<T>(string id, bool throwIfNotFound = false) where T : DotvvmControl
        {
            var control = FindControlInContainer(id, throwIfNotFound);
            if (!(control is T))
            {
                throw new DotvvmControlException(this, $"The control with ID '{id}' was found, however it is not an instance of the desired type '{typeof(T)}'.");
            }
            return (T)control;
        }

        /// <summary>
        /// Finds the control by its ClientId - the id rendered to output html.
        /// </summary>
        public DotvvmControl FindControlByClientId(string id, bool throwIfNotFound = false)
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
        /// Finds the control by its ClientId - the id rendered to output html.
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
        /// Finds the control by its unique ID.
        /// </summary>
        public DotvvmControl FindControlByUniqueId(string controlUniqueId)
        {
            var parts = controlUniqueId.Split('_');
            DotvvmControl result = this;
            for (var i = 0; i < parts.Length; i++)
            {
                result = result.GetAllDescendants(c => !IsNamingContainer(c))
                    .SingleOrDefault(c => c.GetValue(Internal.UniqueIDProperty).Equals(parts[i]));
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
            while (!IsNamingContainer(control) && control.Parent != null)
            {
                control = control.Parent;
            }
            return control;
        }

        /// <summary>
        /// Determines whether the specified control is a naming container.
        /// </summary>
        public static bool IsNamingContainer(DotvvmControl control)
        {
            return (bool)control.GetValue(Internal.IsNamingContainerProperty);
        }

        /// <summary>
        /// Occurs after the viewmodel tree is complete.
        /// </summary>
        internal virtual void OnPreInit(Hosting.IDotvvmRequestContext context)
        {
            foreach (var property in GetDeclaredProperties())
            {
                property.OnControlInitialized(this);
            }
        }

        /// <summary>
        /// Called right before the rendering shall occur.
        /// </summary>
        internal virtual void OnPreRenderComplete(Hosting.IDotvvmRequestContext context)
        {
            // events on properties
            foreach (var property in GetDeclaredProperties())
            {
                property.OnControlRendering(this);
            }
        }

        /// <summary>
        /// Occurs before the viewmodel is applied to the page.
        /// </summary>
        protected internal virtual void OnInit(Hosting.IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Occurs after the viewmodel is applied to the page IHtmlWriter writerand before the commands are executed.
        /// </summary>
        protected internal virtual void OnLoad(Hosting.IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Occurs after the page commands are executed.
        /// </summary>
        protected internal virtual void OnPreRender(Hosting.IDotvvmRequestContext context)
        {
        }

        /// <summary>
        /// Gets the client ID of the control. Returns null if the ID cannot be calculated.
        /// </summary>
        public object GetDotvvmUniqueId()
        {
            // build the client ID
            var fragments = GetUniqueIdFragments();
            if (fragments.Any(f => f.IsExpression))
            {
                var binding = string.Join("+'_'+", fragments.Reverse().Select(f => f.ToJavascriptExpression()));
                return new ValueBindingExpression(h => null, binding);
            }
            else
            {
                return ComposeStaticClientId(fragments);
            }
        }

        private static string ComposeStaticClientId(IList<ClientIDFragment> fragments)
        {
            var sb = new StringBuilder();
            for (int i = fragments.Count - 1; i >= 0; i--)
            {
                if (sb.Length > 0)
                {
                    sb.Append("_");
                }
                sb.Append(fragments[i].Value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Adds the corresponding attribute for the Id property.
        /// </summary>
        protected virtual object CreateClientId()
        {
            if (!string.IsNullOrEmpty(ID))
            {
                // build the client ID
                var fragments = GetClientIdFragments();
                if (!fragments.Any(f => f.IsExpression))
                {
                    // generate ID attribute
                    return ComposeStaticClientId(fragments);
                }
                else
                {
                    var binding = string.Join("+'_'+", fragments.Reverse().Select(f => f.ToJavascriptExpression()));
                    return new ValueBindingExpression(h => { throw new NotSupportedException("Can't evaluate dynamic ID."); }, binding);
                }
            }
            return null;
        }

        private IList<ClientIDFragment> GetUniqueIdFragments()
        {
            var fragments = new List<ClientIDFragment> { new ClientIDFragment((string)GetValue(Internal.UniqueIDProperty)) };
            var dataContextChanges = HasBinding(DataContextProperty) ? 1 : 0;
            foreach (var ancestor in GetAllAncestors())
            {
                if (ancestor.HasBinding(DataContextProperty))
                {
                    dataContextChanges++;
                }

                if (IsNamingContainer(ancestor))
                {
                    var clientIdExpression = (string)ancestor.GetValue(Internal.ClientIDFragmentProperty);
                    if (clientIdExpression != null)
                    {
                        // generate the expression
                        for (int i = 0; i < dataContextChanges; i++)
                        {
                            throw new NotImplementedException(); // todo
                        }
                        fragments.Add(new ClientIDFragment(clientIdExpression, isExpression: true));
                    }
                    else
                    {
                        fragments.Add(new ClientIDFragment((string)ancestor.GetValue(Internal.UniqueIDProperty)));
                    }
                }
            }
            return fragments;
        }

        private IList<ClientIDFragment> GetClientIdFragments()
        {
            var rawId = GetValue(IDProperty);
            // can't generate ID from nothing
            if (rawId == null) return null;

            if (ClientIDMode == ClientIDMode.Static)
            {
                // just rewrite Static mode ID
                return new[] { new ClientIDFragment(ID) };
            }

            var fragments = new List<ClientIDFragment> { ClientIDFragment.FromProperty(rawId) };
            var dataContextChanges = HasBinding(DataContextProperty) ? 1 : 0;
            DotvvmControl childContainer = null;
            bool searchingForIdElement = false;
            foreach (var ancestor in GetAllAncestors())
            {
                if (ancestor.HasBinding(DataContextProperty))
                {
                    dataContextChanges++;
                }

                if (IsNamingContainer(ancestor))
                {
                    if (searchingForIdElement)
                    {
                        fragments.Add(ClientIDFragment.FromProperty(childContainer.GetDotvvmUniqueId()));
                    }
                    searchingForIdElement = false;

                    var clientIdExpression = (string)ancestor.GetValue(Internal.ClientIDFragmentProperty);
                    if (clientIdExpression != null)
                    {
                        // generate the expression
                        var expression = new StringBuilder();
                        for (int i = 0; i < dataContextChanges; i++)
                        {
                            throw new NotImplementedException(); // TODO: 
                        }
                        expression.Append(clientIdExpression);
                        fragments.Add(new ClientIDFragment(expression.ToString(), isExpression: true));
                    }
                    else if (!string.IsNullOrEmpty(ancestor.ID))
                    {
                        // add the ID fragment
                        fragments.Add(new ClientIDFragment(ancestor.ID));
                    }
                    else
                    {
                        searchingForIdElement = true;
                        childContainer = ancestor;
                    }
                }

                if (searchingForIdElement && ancestor.IsPropertySet(ClientIDProperty))
                {
                    fragments.Add(ClientIDFragment.FromProperty(ancestor.GetValueRaw(ClientIDProperty)));
                    searchingForIdElement = false;
                }

                if (ancestor.ClientIDMode == ClientIDMode.Static)
                {
                    break;
                }
            }
            if (searchingForIdElement)
            {
                fragments.Add(ClientIDFragment.FromProperty(childContainer.GetDotvvmUniqueId()));
            }
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
    }
}