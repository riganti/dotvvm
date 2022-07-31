using System;
using System.Collections.Generic;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    [ControlMarkupOptions(DefaultContentProperty = nameof(Property))]
    public class ClassWithInnerElementProperty : PostBackHandler
    {
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public string Property
        {
            get { return (string)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<string, ClassWithInnerElementProperty>(c => c.Property, null);

        protected internal override string ClientHandlerName => null;

        protected internal override Dictionary<string, object> GetHandlerOptions()
        {
            throw new NotImplementedException();
        }
    }
}
