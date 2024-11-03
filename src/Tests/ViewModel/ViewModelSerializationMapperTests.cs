using System;
using System.Collections.Generic;
using System.Linq;
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
using NJ=Newtonsoft.Json;

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
        public void ViewModelSerializationMapper_Name_NewtonsoftJsonAttributes()
        {
            // we still respect NJ attributes
            var mapper = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();
            var map = mapper.GetMap(typeof(NewtonsoftJsonAttributes));

            XAssert.DoesNotContain("Ignored", map.Properties.Select(p => p.Name));
            Assert.AreEqual("new_name", map.Property("RenamedProperty").Name);
        }

        [DataTestMethod]
        [DataRow(typeof(MemberShadowingViewModelB), "Property1", "List<string>", "List<List<string>>")]
        [DataRow(typeof(MemberShadowingViewModelC), "Property1", "List<string>", "object")]
        [DataRow(typeof(MemberShadowingViewModelD), "Property2", "ViewModelSerializationMapperTests.JsonPropertyVsBindAttribute", "object")]
        [DataRow(typeof(MemberShadowingViewModelE), "Property2", "ViewModelSerializationMapperTests.JsonPropertyVsBindAttribute", "TestViewModelWithBind")]
        public void ViewModelSerializationMapper_Name_MemberShadowing(Type type, string prop, string t1, string t2)
        {
            var mapper = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();

            var exception = XAssert.ThrowsAny<Exception>(() => mapper.GetMap(type));
            XAssert.IsType<InvalidOperationException>(exception.GetBaseException());
            XAssert.Equal<object>($"Detected forbidden member shadowing of 'ViewModelSerializationMapperTests.MemberShadowingViewModelA.{prop}: {t1}' by '{type.ToCode(stripNamespace: true)}.{prop}: {t2}' while building serialization map for '{type.ToCode(stripNamespace: true)}'", exception.GetBaseException().Message);
        }

        [TestMethod]
        public void ViewModelSerializationMapper_Name_NameConflictAttributes()
        {
            var mapper = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();
            var exception = XAssert.ThrowsAny<Exception>(() => mapper.GetMap(typeof(NameConflictAttributes)));
            XAssert.IsType<InvalidOperationException>(exception.GetBaseException());
            Assert.AreEqual("Serialization map for 'DotVVM.Framework.Tests.ViewModel.ViewModelSerializationMapperTests.NameConflictAttributes' has a name conflict between a property 'Name' and property 'MyProperty' — both are named 'Name' in JSON.", exception.GetBaseException().Message);
        }

        [TestMethod]
        public void ViewModelSerializationMapper_Name_NameConflictFieldProperty()
        {
            var mapper = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();
            var exception = XAssert.ThrowsAny<Exception>(() => mapper.GetMap(typeof(NameConflictFieldProperty)));
            XAssert.IsType<InvalidOperationException>(exception.GetBaseException());
            Assert.AreEqual("Serialization map for 'DotVVM.Framework.Tests.ViewModel.ViewModelSerializationMapperTests.NameConflictFieldProperty' has a name conflict between a property 'Name' and field 'MyField' — both are named 'Name' in JSON.", exception.GetBaseException().Message);
        }

        public class MemberShadowingViewModelA
        {
            public List<string> Property1 { get; set; }
            public JsonPropertyVsBindAttribute Property2 { get; set; }
        }

        public class MemberShadowingViewModelB : MemberShadowingViewModelA
        {
            // different type
            public new List<List<string>> Property1 { get; set; }
        }

        public class MemberShadowingViewModelC : MemberShadowingViewModelA
        {
            // more generic
            public new object Property1 { get; set; }
        }
        public class MemberShadowingViewModelD : MemberShadowingViewModelA
        {
            // more generic
            public new object Property2 { get; set; }
        }
        public class MemberShadowingViewModelE : MemberShadowingViewModelA
        {
            // different type
            public new TestViewModelWithBind Property2 { get; set; }
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

        public class NewtonsoftJsonAttributes
        {
            [NJ.JsonIgnore]
            public bool Ignored { get; set; }

            [NJ.JsonProperty("new_name")]
            public string RenamedProperty { get; set; }
        }

        public class NameConflictAttributes
        {
            [Bind(Name = "Name")]
            public string MyProperty { get; set; }

            public string Name { get; set; }
        }

        public class NameConflictFieldProperty
        {
            public string Name { get; set; }
            [Bind(Name = "Name")]
            public string MyField;
        }

    }
}
