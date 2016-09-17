using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class UITests
    {

        public static readonly DotvvmProperty GenerateStubProperty =
            DotvvmProperty.Register<bool, UITests>(() => GenerateStubProperty, defaultValue: true, isValueInherited: true);

        public static readonly DotvvmProperty NameProperty =
            DotvvmProperty.Register<string, UITests>(() => NameProperty, isValueInherited: false);

    }
}
