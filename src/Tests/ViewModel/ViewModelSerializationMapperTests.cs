using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Testing;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using FastExpressionCompiler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class ViewModelSerializationMapperTests
    {

        [TestMethod]
        public void ViewModelSerializationMapper_Name_JsonPropertyVsBindAttribute()
        {
            var mapper = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();
            var map = mapper.GetMap(typeof(JsonPropertyVsBindAttribute));

            Assert.AreEqual("NoAttribute", map.Property("NoAttribute").Name);
            Assert.AreEqual("bind1", map.Property("BindWithName").Name);
            Assert.AreEqual("BindWithoutName", map.Property("BindWithoutName").Name);
            Assert.AreEqual("jsonProperty1", map.Property("JsonPropertyWithName").Name);
            Assert.AreEqual("JsonPropertyWithoutName", map.Property("JsonPropertyWithoutName").Name);
            Assert.AreEqual("bind2", map.Property("BothWithName").Name);
            Assert.AreEqual("jsonProperty3", map.Property("BindWithoutNameJsonPropertyWithName").Name);
        }

        [TestMethod]
        public void ViewModelSerializationMapper_Name_MemberShadowing()
        {
            var mapper = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();

            var exception = XAssert.ThrowsAny<Exception>(() => mapper.GetMap(typeof(MemberShadowingViewModelB)));
            XAssert.IsType<InvalidOperationException>(exception.GetBaseException());
            XAssert.Equal($"Detected member shadowing on property \"{nameof(MemberShadowingViewModelB.Property)}\" while building serialization map for \"{typeof(MemberShadowingViewModelB).ToCode()}\"", exception.GetBaseException().Message);
        }

        public class MemberShadowingViewModelA
        {
            public object Property { get; set; }
        }

        public class MemberShadowingViewModelB : MemberShadowingViewModelA
        {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
            public string Property { get; set; }
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        }

        public class JsonPropertyVsBindAttribute
        {

            public string NoAttribute { get; set; }

            [Bind(Name = "bind1")]
            public string BindWithName { get; set; }

            [Bind]
            public string BindWithoutName { get; set; }

            [JsonPropertyName("jsonProperty1")]
            public string JsonPropertyWithName { get; set; }

            public string JsonPropertyWithoutName { get; set; }

            [Bind(Name = "bind2")]
            [JsonPropertyName("jsonProperty2")]
            public string BothWithName { get; set; }

            [Bind()]
            [JsonPropertyName("jsonProperty3")]
            public string BindWithoutNameJsonPropertyWithName { get; set; }

        }

    }
}
