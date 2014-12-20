using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Represents a base class for all Redwood controls.
    /// </summary>
    public abstract class RedwoodControl
    {

        internal Dictionary<RedwoodProperty, object> Properties = new Dictionary<RedwoodProperty, object>();


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
        public void EnsureControlHasId(bool autoGenerate = false)
        {
            if (string.IsNullOrWhiteSpace(ID))
            {
                throw new Exception(string.Format("The control of type '{0}' must have ID!", GetType().FullName));      // TODO: exception handling
            }
            if (!Regex.IsMatch(ID, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                throw new Exception(string.Format("The control ID '{0}' is not valid! It can contain only characters, numbers and the underscore character, and it cannot start with a number!", ID));      // TODO: exception handling
            }

            if (autoGenerate)
            {
                // TODO: auto-generation of the control ID
            }
        }
    }
}