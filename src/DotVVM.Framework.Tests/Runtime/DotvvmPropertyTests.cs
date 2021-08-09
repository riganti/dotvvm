#nullable enable

using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmPropertyTests
    {
        class MoqComponent : DotvvmBindableObject
        {
            public object? Property
            {
                get { return (object?)GetValue(PropertyProperty); }
                set { SetValue(PropertyProperty, value); }
            }
            public static DotvvmProperty PropertyProperty
                = DotvvmProperty.Register<object, MoqComponent>(t => t.Property);
        }

        [TestMethod]
        public void DotvvmProperty_PropertyRegisteredTwiceThrowException()
        {
            Assert.ThrowsException<ArgumentException>(() => {
                _ = MoqComponent.PropertyProperty; // calls the static ctor
                DotvvmProperty.Register<bool, MoqComponent>(t => t.Property);
            });
        }

        public class TestObject : DotvvmBindableObject
        {
            public string? Aliased
            {
                get => (string?)GetValue(AliasedProperty);
                set => SetValue(AliasedProperty, value);
            }
            public static DotvvmProperty AliasedProperty
                = DotvvmProperty.Register<int, TestObject>(c => c.Aliased);

            [Obsolete("Use 'Aliased' instead.")]
            public string? Alias
            {
                get => (string?)GetValue(AliasedProperty);
                set => SetValue(AliasedProperty, value);
            }
            public static DotvvmProperty AliasProperty
                = DotvvmProperty.RegisterAlias<TestObject>(c => c.Alias, AliasedProperty);
        }

        [TestMethod]
        public void DotvvmProperty_AliasRegistered()
        {
            var alias = (DotvvmPropertyAlias)TestObject.AliasProperty; // calls the static ctor
            var resolvedAlias = DotvvmProperty.ResolveProperty(typeof(TestObject), nameof(TestObject.Alias));
            Assert.IsTrue(resolvedAlias == alias);
            Assert.IsTrue(alias.Aliased == TestObject.AliasedProperty);
        }
    }
}
