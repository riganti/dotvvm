using System;
using System.Collections.Generic;

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
        [MarkupOptions(Name = "id", AllowBinding = false)]
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
        public virtual object GetValue(RedwoodProperty property)
        {
            return property.GetValue(this);
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
    }
}