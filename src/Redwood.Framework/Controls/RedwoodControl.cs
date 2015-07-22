using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Represents a base class for all Redwood controls.
    /// </summary>
    [ContainsRedwoodProperties]
    [ControlMarkupOptions(AllowContent = true)]
    public abstract class RedwoodControl
    {
       
        protected internal Dictionary<RedwoodProperty, object> properties;

        /// <summary>
        /// Gets the collection of control property values.
        /// </summary>
        protected internal Dictionary<RedwoodProperty, object> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new Dictionary<RedwoodProperty, object>();
                }
                return properties;
            }
        }

        protected List<string> ResourceDependencies = new List<string>();


            /// <summary>
        /// Gets the parent control.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public RedwoodControl Parent { get; internal set; }


        /// <summary>
        /// Gets the child controls.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public RedwoodControlCollection Children { get; private set; }


        /// <summary>
        /// Gets or sets the unique control ID.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string ID
        {
            get { return (string)GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }
        public static readonly RedwoodProperty IDProperty =
            RedwoodProperty.Register<string, RedwoodControl>(c => c.ID, isValueInherited: false);



        private static ConcurrentDictionary<Type, IReadOnlyList<RedwoodProperty>> declaredProperties = new ConcurrentDictionary<Type, IReadOnlyList<RedwoodProperty>>();
        /// <summary>
        /// Gets all properties declared on this class or on any of its base classes.
        /// </summary>
        private IReadOnlyList<RedwoodProperty> GetDeclaredProperties()
        {
            return declaredProperties.GetOrAdd(GetType(), RedwoodProperty.ResolveProperties);
        }


        /// <summary>
        /// Gets the value of a specified property.
        /// </summary>
        public virtual object GetValue(RedwoodProperty property, bool inherit = true)
        {
            return property.GetValue(this, inherit);
        }

        /// <summary>
        /// Sets the value of a specified property.
        /// </summary>
        public virtual void SetValue(RedwoodProperty property, object value)
        {
            property.SetValue(this, value);
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodControl"/> class.
        /// </summary>
        public RedwoodControl()
        {
            Children = new RedwoodControlCollection(this);
        }

        /// <summary>
        /// Gets this control and all of its descendants.
        /// </summary>
        public IEnumerable<RedwoodControl> GetThisAndAllDescendants(Func<RedwoodControl, bool> enumerateChildrenCondition = null)
        {
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
        public IEnumerable<RedwoodControl> GetAllDescendants(Func<RedwoodControl, bool> enumerateChildrenCondition = null)
        {
            foreach (var child in Children)
            {
                yield return child;

                if (enumerateChildrenCondition == null || enumerateChildrenCondition(child))
                {
                    foreach (var grandChild in child.GetAllDescendants())
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all ancestors of this control starting with the parent.
        /// </summary>
        public IEnumerable<RedwoodControl> GetAllAncestors()
        {
            var ancestor = Parent;
            while (ancestor != null)
            {
                yield return ancestor;
                ancestor = ancestor.Parent;
            }
        }

        /// <summary>
        /// Determines whether the control has only white space content.
        /// </summary>
        public bool HasOnlyWhiteSpaceContent()
        {
            return Children.All(c => (c is Literal && ((Literal)c).HasWhiteSpaceContentOnly()));
        }


        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        public virtual void Render(IHtmlWriter writer, RenderContext context)
        {
            RenderControl(writer, context);
        }

        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        protected virtual void RenderControl(IHtmlWriter writer, RenderContext context)
        {
            AddAttributesToRender(writer, context);
            RenderBeginTag(writer, context);
            RenderContents(writer, context);
            RenderEndTag(writer, context);
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected virtual void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
        }

        /// <summary>
        /// Renders the control begin tag.
        /// </summary>
        protected virtual void RenderBeginTag(IHtmlWriter writer, RenderContext context)
        {
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected virtual void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            RenderChildren(writer, context);
        }

        /// <summary>
        /// Renders the control end tag.
        /// </summary>
        protected virtual void RenderEndTag(IHtmlWriter writer, RenderContext context)
        {
        }


        /// <summary>
        /// Renders the children.
        /// </summary>
        protected void RenderChildren(IHtmlWriter writer, RenderContext context)
        {
            foreach (var child in Children)
            {
                child.Render(writer, context);
            }
        }

        /// <summary>
        /// Ensures that the control has ID. The method can auto-generate it, if specified.
        /// </summary>
        public void EnsureControlHasId(bool autoGenerate = true)
        {
            if (autoGenerate && string.IsNullOrEmpty(ID))
            {
                ID = AutoGenerateControlId();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ID))
                {
                    throw new Exception(string.Format("The control of type '{0}' must have ID!", GetType().FullName)); // TODO: exception handling
                }
                if (!Regex.IsMatch(ID, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
                {
                    throw new Exception(string.Format("The control ID '{0}' is not valid! It can contain only characters, numbers and the underscore character, and it cannot start with a number!", ID)); // TODO: exception handling
                }
            }
        }

        /// <summary>
        /// Generates unique control ID automatically.
        /// </summary>
        private string AutoGenerateControlId()
        {
            var id = GetValue(Internal.UniqueIDProperty).ToString();
            var control = Parent;
            do
            {
                if ((bool)control.GetValue(Internal.IsNamingContainerProperty))
                {
                    id = control.GetValue(Internal.UniqueIDProperty) + "_" + id;
                }
                control = control.Parent;
            } 
            while (control != null);
            return id;
        }


        /// <summary>
        /// Finds the control by its ID.
        /// </summary>
        public RedwoodControl FindControl(string id, bool throwIfNotFound = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var control = GetAllDescendants(c => !IsNamingContainer(c)).SingleOrDefault(c => c.ID == id);
            if (control == null && throwIfNotFound)
            {
                throw new Exception(string.Format("The control with ID '{0}' was not found.", id));
            }
            return control;
        }

        /// <summary>
        /// Finds the control by its ID.
        /// </summary>
        public T FindControl<T>(string id, bool throwIfNotFound = false) where T : RedwoodControl
        {
            var control = FindControl(id, throwIfNotFound);
            if (!(control is T))
            {
                throw new Exception(string.Format("The control with ID '{0}' was found, however it is not an instance of the desired type '{1}'.", id, typeof(T)));
            }
            return (T)control;
        }

        /// <summary>
        /// Gets the naming container of the current control.
        /// </summary>
        public RedwoodControl GetNamingContainer()
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
        public static bool IsNamingContainer(RedwoodControl control)
        {
            return control is RedwoodBindableControl && (bool) ((RedwoodBindableControl) control).GetValue(Internal.IsNamingContainerProperty);
        }


        /// <summary>
        /// Gets the root of the control tree.
        /// </summary>
        public RedwoodControl GetRoot()
        {
            if (Parent == null) return this;
            return GetAllAncestors().Last();
        }


        /// <summary>
        /// Occurs after the viewmodel tree is complete.
        /// </summary>
        internal virtual void OnPreInit(RedwoodRequestContext context)
        {
            foreach (var property in GetDeclaredProperties())
            {
                property.OnControlInitialized(this);
            }
        }

        /// <summary>
        /// Called right before the rendering shall occur.
        /// </summary>
        internal virtual void OnPreRenderComplete(RedwoodRequestContext context)
        {
            // add resource dependencies to manager
            foreach (var resource in ResourceDependencies)
            {
                context.ResourceManager.AddRequiredResource(resource);
            }

            // events on properties
            foreach (var property in GetDeclaredProperties())
            {
                property.OnControlRendering(this);
            }
        }


        /// <summary>
        /// Occurs before the viewmodel is applied to the page.
        /// </summary>
        protected internal virtual void OnInit(RedwoodRequestContext context)
        {
        }

        /// <summary>
        /// Occurs after the viewmodel is applied to the page and before the commands are executed.
        /// </summary>
        protected internal virtual void OnLoad(RedwoodRequestContext context)
        {
        }

        /// <summary>
        /// Occurs after the page commands are executed.
        /// </summary>
        protected internal virtual void OnPreRender(RedwoodRequestContext context)
        {
        }
    }
}