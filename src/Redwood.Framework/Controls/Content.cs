using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Contains markup that will be moved into the according <see cref="ContentPlaceHolder"/> in the master page.
    /// </summary>
    public class Content : RedwoodControl
    {

        /// <summary>
        /// Gets or sets the ID of the <see cref="ContentPlaceHolder"/> control in the master page where the content will be placed.
        /// </summary>
        public string ContentPlaceHolderID
        {
            get { return (string)GetValue(ContentPlaceHolderIDProperty); }
            set { SetValue(ContentPlaceHolderIDProperty, value); }
        }
        public static readonly RedwoodProperty ContentPlaceHolderIDProperty =
            RedwoodProperty.Register<string, Content>(c => c.ContentPlaceHolderID);

    }
}