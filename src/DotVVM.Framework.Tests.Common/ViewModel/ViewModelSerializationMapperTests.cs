using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DotVVM.Framework.Tests.Common.ViewModel
{
    [TestClass]
    public class ViewModelSerializationMapperTests
    {

        [TestMethod]
        public void ViewModelSerializationMapper_Name_JsonPropertyVsBindAttribute()
        {
            var mapper = new ViewModelSerializationMapper(new ViewModelValidationRuleTranslator(),
                new AttributeViewModelValidationMetadataProvider(),
                new DefaultPropertySerialization());
            var map = mapper.GetMap(typeof(JsonPropertyVsBindAttribute));

            Assert.AreEqual("NoAttribute", map.Property("NoAttribute").Name);
            Assert.AreEqual("bind1", map.Property("BindWithName").Name);
            Assert.AreEqual("BindWithoutName", map.Property("BindWithoutName").Name);
            Assert.AreEqual("jsonProperty1", map.Property("JsonPropertyWithName").Name);
            Assert.AreEqual("JsonPropertyWithoutName", map.Property("JsonPropertyWithoutName").Name);
            Assert.AreEqual("bind2", map.Property("BothWithName").Name);
            Assert.AreEqual("jsonProperty3", map.Property("BindWithoutNameJsonPropertyWithName").Name);
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
