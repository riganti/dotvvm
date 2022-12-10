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

        [TestMethod]
        public void Error_ConstructorNotAllowed()
        {
            var obj = new ViewModelWithConstructor("test");
            var ex = Assert.ThrowsException<Exception>(() => SerializerTests.SerializeAndDeserialize(obj));

            Assert.AreEqual("Can not deserialize DotVVM.Framework.Tests.ViewModel.SerializerErrorTests.ViewModelWithConstructor, no parameterless constructor found. Use the [JsonConstructor] attribute to specify the constructor used for deserialization.", ex.Message);
        }

        public class ViewModelWithConstructor
        {
            public string Property { get; set; }

            public ViewModelWithConstructor(string property)
            {
                Property = property;
            }
        }

        [TestMethod]
        public void Error_ConstructorMismatch()
        {
            var obj = new ViewModelWithConstructorMismatch("test");
            var ex = Assert.ThrowsException<Exception>(() => SerializerTests.SerializeAndDeserialize(obj));

            Assert.AreEqual("Can not deserialize DotVVM.Framework.Tests.ViewModel.SerializerErrorTests.ViewModelWithConstructorMismatch, constructor parameter something is not mapped to any property.", ex.Message);
        }

        public class ViewModelWithConstructorMismatch
        {
            public string Property { get; set; }

            [JsonConstructor]
            public ViewModelWithConstructorMismatch(string something)
            {
                Property = something;
            }
        }
        [TestMethod]
        public void Error_ConstructorMismatch2()
        {
            // Error handling is different if the mismatched parameter could a service
            var obj = new ViewModelWithConstructorMismatch2(new ThisCouldBeAService { Property = "test" });
            var ex = Assert.ThrowsException<Exception>(() => SerializerTests.SerializeAndDeserialize(obj));

            Assert.AreEqual("Can not deserialize DotVVM.Framework.Tests.ViewModel.SerializerErrorTests.ViewModelWithConstructorMismatch2, constructor parameter s is not mapped to any property and service SerializerErrorTests.ThisCouldBeAService was not found in ServiceProvider.", ex.Message);
        }

        public class ViewModelWithConstructorMismatch2
        {
            public string Property { get; set; }

            [JsonConstructor]
            public ViewModelWithConstructorMismatch2(ThisCouldBeAService s)
            {
                Property = s.Property;
            }
        }

        public class ThisCouldBeAService
        {
            public string Property { get; set; }
        }
    }
}
