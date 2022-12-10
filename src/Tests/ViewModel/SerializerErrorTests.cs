using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Testing;
using System.Text;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class SerializerErrorTests
    {
        [TestMethod]
        public void Error_AbstractClassWithPublicConstructor()
        {
            AbstractClassWithPublicConstructor obj = new AbstractClassWithPublicConstructor.DerivedClass("test");
            var ex = Assert.ThrowsException<Exception>(() => SerializerTests.SerializeAndDeserialize(obj));

            Assert.AreEqual("Can not deserialize DotVVM.Framework.Tests.ViewModel.SerializerErrorTests.AbstractClassWithPublicConstructor because it's abstract. Please avoid using abstract types in view model. If you really mean it, you can add a static factory method and mark it with [JsonConstructor] attribute.", ex.Message);
        }

        public abstract class AbstractClassWithPublicConstructor
        {
            public string Property { get; }

            [JsonConstructor]
            public AbstractClassWithPublicConstructor(string property)
            {
                Property = property;
            }

            public class DerivedClass: AbstractClassWithPublicConstructor
            {
                public DerivedClass(string property) : base(property)
                {
                }
            }
        }

        [TestMethod]
        public void Error_InitOnlyProperty()
        {
            var obj = new ViewModelWithInitOnlyProperty { Property = "test" };
            var ex = Assert.ThrowsException<Exception>(() => SerializerTests.SerializeAndDeserialize(obj));

            Assert.AreEqual("Deserialization of DotVVM.Framework.Tests.ViewModel.SerializerErrorTests.ViewModelWithInitOnlyProperty is not allowed, because it implements IDotvvmViewModel and init-only property Property is transferred client → server. To allow cloning the object on deserialization, mark a constructor with [JsonConstructor].", ex.Message);
        }

        public class ViewModelWithInitOnlyProperty: DotvvmViewModelBase
        {
            public string Property { get; init; }
        }
    }
}
