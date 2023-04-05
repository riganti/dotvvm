using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using FastExpressionCompiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class ViewModelSerializationMapperTests
    {

        [TestMethod]
        public void ViewModelSerializationMapper_Name_JsonPropertyVsBindAttribute()
        {
            var mapper = new ViewModelSerializationMapper(new ViewModelValidationRuleTranslator(),
                new AttributeViewModelValidationMetadataProvider(),
                new DefaultPropertySerialization(),
                DotvvmConfiguration.CreateDefault());
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
            var mapper = new ViewModelSerializationMapper(new ViewModelValidationRuleTranslator(),
                new AttributeViewModelValidationMetadataProvider(),
                new DefaultPropertySerialization(),
                DotvvmConfiguration.CreateDefault());

            Assert.ThrowsException<InvalidOperationException>(() => mapper.GetMap(typeof(MemberShadowingViewModelB)),
                $"Detected member shadowing on property \"{nameof(MemberShadowingViewModelB.Property)}\" " +
                $"while building serialization map for \"{typeof(MemberShadowingViewModelB).ToCode()}\"");
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

            [JsonProperty("jsonProperty1")]
            public string JsonPropertyWithName { get; set; }

            [JsonProperty]
            public string JsonPropertyWithoutName { get; set; }

            [Bind(Name = "bind2")]
            [JsonProperty("jsonProperty2")]
            public string BothWithName { get; set; }

            [Bind()]
            [JsonProperty("jsonProperty3")]
            public string BindWithoutNameJsonPropertyWithName { get; set; }

        }

    }
}
