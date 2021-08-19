#nullable enable
#pragma warning disable CS0618 // disable the warning about obsoletes

using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmPropertyTests
    {
        public class MoqComponent : DotvvmBindableObject
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
            [MarkupOptions(Required = true)]
            public string? Aliased
            {
                get => (string?)GetValue(AliasedProperty);
                set => SetValue(AliasedProperty, value);
            }
            public static DotvvmProperty AliasedProperty
                = DotvvmProperty.Register<int, TestObject>(c => c.Aliased);

            [Obsolete("Use 'Aliased' instead.")]
            [PropertyAlias(nameof(Aliased))]
            public string? Alias
            {
                get => (string?)GetValue(AliasedProperty);
                set => SetValue(AliasedProperty, value);
            }
            public static DotvvmProperty AliasProperty
                = DotvvmProperty.RegisterAlias<TestObject>(c => c.Alias);
        }

        [ContainsDotvvmProperties]
        public class AttachedOne
        {
            [AttachedProperty(typeof(string))]
            public static readonly DotvvmProperty OneProperty =
                DotvvmProperty.Register<string, AttachedOne>(() => OneProperty, string.Empty);
        }

        [ContainsDotvvmProperties]
        public class AttachedTwo
        {
            [AttachedProperty(typeof(string))]
            [PropertyAlias("One", typeof(AttachedOne))]
            public static readonly DotvvmProperty TwoProperty =
                DotvvmProperty.RegisterAlias<AttachedTwo>(() => TwoProperty);
        }

        [TestInitialize]
        public void Init()
        {
            DotvvmPropertyAlias.Resolve((DotvvmPropertyAlias)TestObject.AliasProperty);
            _ = AttachedOne.OneProperty;
            DotvvmPropertyAlias.Resolve((DotvvmPropertyAlias)AttachedTwo.TwoProperty);
        }

        [TestMethod]
        public void DotvvmProperty_AliasRegistered()
        {
            var resolvedAlias = DotvvmProperty.ResolveProperty(typeof(TestObject), nameof(TestObject.Alias));
            Assert.IsTrue(resolvedAlias == TestObject.AliasProperty);
            Assert.IsTrue(((DotvvmPropertyAlias)TestObject.AliasedProperty).Aliased == TestObject.AliasedProperty);
        }

        [TestMethod]
        public void DotvvmProperty_AliasMarkupOptionsPropagates()
        {
            Assert.IsTrue(TestObject.AliasProperty.MarkupOptions.Required);
        }

        [TestMethod]
        public void DotvvmProperty_AttachedAlias()
        {
            Assert.IsTrue(((DotvvmPropertyAlias)AttachedTwo.TwoProperty).Aliased == AttachedOne.OneProperty);
        }

        [TestMethod]
        public void DotvvmProperty_ObsoleteAttribute()
        {
            Assert.IsTrue(TestObject.AliasProperty.IsObsolete);
            var workaroundMessage = typeof(TestObject)
                .GetProperty(nameof(TestObject.Alias))
                ?.GetCustomAttribute<ObsoleteAttribute>()
                ?.Message;
            Assert.IsTrue(TestObject.AliasProperty.WorkaroundMessage == workaroundMessage);
        }
    }
}
