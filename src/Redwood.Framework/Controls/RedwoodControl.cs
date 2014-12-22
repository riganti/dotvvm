using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Represents a base class for all Redwood controls.
    /// </summary>
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
        public IEnumerable<RedwoodControl> GetThisAndAllDescendants()
        {
            yield return this;
            foreach (var descendant in GetAllDescendants())
            {
                yield return descendant;
            }
        } 

        /// <summary>
        /// Gets all descendant controls of this control.
        /// </summary>
        public IEnumerable<RedwoodControl> GetAllDescendants()
        {
            foreach (var child in Children)
            {
                yield return child;
                foreach (var grandChild in child.GetAllDescendants())
                {
                    yield return grandChild;
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
        /// Renders the control into the specified writer.
        /// </summary>
        public virtual void Render(IHtmlWriter writer, RenderContext context)
        {
            RenderChildren(writer, context);
        }


        /// <summary>
        /// Renders the children.
        /// </summary>
        protected virtual void RenderChildren(IHtmlWriter writer, RenderContext context)
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
        public RedwoodControl FindControl(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            return GetAllDescendants().SingleOrDefault(c => c.ID == id);
        }
    }
}