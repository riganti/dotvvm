using System;
using System.Collections.Generic;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    public class ClassWithoutInnerElementProperty : PostBackHandler
    {
        [MarkupOptions(MappingMode = MappingMode.Attribute)]
        public string Property
        {
            get { return (string)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<string, ClassWithoutInnerElementProperty>(c => c.Property, null);

        protected internal override string ClientHandlerName => null;

        protected internal override Dictionary<string, object> GetHandlerOptions()
        {
            throw new NotImplementedException();
        }
    }
}
