using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// A common base for button controls.
    /// </summary>
    public abstract class ButtonBase : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets the text on the button.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, ButtonBase>(t => t.Text, "");


        /// <summary>
        /// Gets or sets the command that will be triggered when the button is pressed.
        /// </summary>
        public Action Click
        {
            get { return (Action)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly RedwoodProperty ClickProperty =
            RedwoodProperty.Register<Action, ButtonBase>(t => t.Click, null);



        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonBase"/> class.
        /// </summary>
        public ButtonBase(string tagName) : base(tagName)
        {
        }
        
    }
}