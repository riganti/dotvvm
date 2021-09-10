using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class UITests
    {

        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty GenerateStubProperty =
            DotvvmProperty.Register<bool, UITests>(() => GenerateStubProperty, defaultValue: true, isValueInherited: true);

        /// <summary>
        /// Gets or sets a name rendered as data-uitest-name attribute which is used by Selenium to identify the control in the page.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [AttachedProperty(typeof(string))]
        public static readonly ActiveDotvvmProperty NameProperty =
            DelegateActionProperty<string>.Register<UITests>("Name", AddNameProperty);

        private static void AddNameProperty(IHtmlWriter writer, IDotvvmRequestContext context, DotvvmProperty prop, DotvvmControl control)
        {
            if (context.Configuration.Debug && control is HtmlGenericControl htmlControl)
            {
                ((HtmlGenericControl) control).Attributes.Add("data-uitest-name", prop.GetValue(control));
            }
        }
    }
}
