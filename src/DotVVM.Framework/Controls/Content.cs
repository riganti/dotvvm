#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Contains markup that will be placed inside the according ContentPlaceHolder in the master page.
    /// </summary>
    public class Content : DotvvmControl
    {
        /// <summary>
        /// Gets or sets the ID of the ContentPlaceHolder control in the master page in which the content will be placed.
        /// </summary>
        public string? ContentPlaceHolderID
        {
            get { return (string?)GetValue(ContentPlaceHolderIDProperty); }
            set { SetValue(ContentPlaceHolderIDProperty, value); }
        }
        public static readonly DotvvmProperty ContentPlaceHolderIDProperty =
            DotvvmProperty.Register<string?, Content>(c => c.ContentPlaceHolderID);
    }
}
