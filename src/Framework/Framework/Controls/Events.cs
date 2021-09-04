using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class Events
    {
        [AttachedProperty(typeof(Command))]
        public static ActiveDotvvmProperty ClickProperty =
            ActiveDotvvmProperty.RegisterCommandToAttribute<Events>("Click", "onclick");

        [AttachedProperty(typeof(Command))]
        public static ActiveDotvvmProperty DoubleClickProperty =
            ActiveDotvvmProperty.RegisterCommandToAttribute<Events>("DoubleClick", "ondblclick");
    }
}
