using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
    public class DeviceList : DotvvmMarkupControl
    {
        public Func<Guid, Task> Edit
        {
            get => (Func<Guid, Task>)GetValue(EditProperty);
            set => SetValue(EditProperty, value);
        }
        public static readonly DotvvmProperty EditProperty
            = DotvvmProperty.Register<Func<Guid, Task>, DeviceList>(c => c.Edit, null);

        public Func<Guid, Task> Remove
        {
            get => (Func<Guid, Task>)GetValue(RemoveProperty);
            set => SetValue(RemoveProperty, value);
        }
        public static readonly DotvvmProperty RemoveProperty
            = DotvvmProperty.Register<Func<Guid, Task>, DeviceList>(c => c.Remove, null);

    }
}

