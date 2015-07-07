using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Configuration;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;
using Microsoft.CodeAnalysis.Text;

namespace DotVVM.Framework.Tests.VS2015Extension
{
    [TestClass]
    public class MetadataControlResolverTests
    {
        private AdhocWorkspace workspace;
        private ProjectInfo project;
        private DothtmlCompletionContext context;

        
        [TestInitialize]
        public void TestInit()
        {
            workspace = new AdhocWorkspace();
            project = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "TestProj", "TestProj", LanguageNames.CSharp)
                .WithMetadataReferences(new[] {
                    MetadataReference.CreateFromAssembly(typeof(DotvvmConfiguration).Assembly),
                    MetadataReference.CreateFromAssembly(typeof(object).Assembly)
                });
            workspace.AddProject(project);

            workspace.AddDocument(project.Id, "test", SourceText.From("class A {}"));

            context = new DothtmlCompletionContext()
            {
                Configuration = DotvvmConfiguration.CreateDefault(),
                RoslynWorkspace = workspace
            };
        }


        [TestMethod]
        public void MetadataControlResolver_ReloadAllControls()
        {
            TestInit();

            var resolver = new MetadataControlResolver();
            var allControls = resolver.ReloadAllControls(context);

            Assert.IsTrue(allControls.Any(c => c.DisplayText == "rw:TextBox"));
        }

        [TestMethod]
        public void MetadataControlResolver_ReloadAllControls_PropertyMetadata()
        {
            TestInit();

            var resolver = new MetadataControlResolver();
            var allControls = resolver.ReloadAllControls(context);

            var repeater = allControls.FirstOrDefault(c => c.DisplayText == "rw:Repeater");

            var itemTemplateProperty = resolver.GetMetadata("rw:Repeater").Properties.FirstOrDefault(p => p.Name == "ItemTemplate");
            Assert.IsTrue(itemTemplateProperty.IsTemplate);
            Assert.IsTrue(itemTemplateProperty.IsElement);
        }

        [TestMethod]
        public void MetadataControlResolver_ReloadAttachedProperties()
        {
            TestInit();

            var resolver = new MetadataControlResolver();
            var attachedProperties = resolver.GetAttachedPropertyNames(context);

            Assert.IsTrue(attachedProperties.Any(c => c.DisplayText == "RenderSettings.Mode"));
        }

        [TestMethod]
        public void MetadataControlResolver_AttachedPropertyEnumValues()
        {
            TestInit();

            var resolver = new MetadataControlResolver();
            var values = resolver.GetControlAttributeValues(context, new List<string>() { "html" }, "RenderSettings.Mode");

            Assert.IsTrue(values.Any(v => v.DisplayText == "Server"));
            Assert.IsTrue(values.Any(v => v.DisplayText == "Client"));
        }

        [TestMethod]
        public void MetadataControlResolver_ElementContext_Control()
        {
            TestInit();

            var resolver = new MetadataControlResolver();
            var allControls = resolver.ReloadAllControls(context);

            ControlMetadata control;
            ControlPropertyMetadata property;
            resolver.GetElementContext(new List<string>() { "html", "body", "rw:Button" }, out control, out property);

            Assert.IsNotNull(control);
            Assert.IsNull(property);
        }

        [TestMethod]
        public void MetadataControlResolver_ElementContext_HtmlGenericControl()
        {
            TestInit();

            var resolver = new MetadataControlResolver();
            var allControls = resolver.ReloadAllControls(context);

            ControlMetadata control;
            ControlPropertyMetadata property;
            resolver.GetElementContext(new List<string>() { "html" }, out control, out property);

            Assert.IsNotNull(control);
            Assert.IsNull(property);

            Assert.IsTrue(control.Properties.Any(p => p.Name == "Visible"));
            Assert.IsTrue(control.Properties.Any(p => p.Name == "DataContext"));
        }

        [TestMethod]
        public void MetadataControlResolver_ElementContext_ElementProperty()
        {
            TestInit();

            var resolver = new MetadataControlResolver();
            var allControls = resolver.ReloadAllControls(context);

            ControlMetadata control;
            ControlPropertyMetadata property;
            resolver.GetElementContext(new List<string>() { "html", "body", "rw:Repeater", "ItemTemplate" }, out control, out property);

            Assert.IsNotNull(control);
            Assert.IsNotNull(property);
        }

        [TestMethod]
        public void MetadataControlResolver_ElementContext_AttributeProperty()
        {
            TestInit();

            var resolver = new MetadataControlResolver();
            var allControls = resolver.ReloadAllControls(context);

            ControlMetadata control;
            ControlPropertyMetadata property;
            resolver.GetElementContext(new List<string>() { "html", "body", "rw:Repeater", "WrapperTagName" }, out control, out property);

            Assert.IsNotNull(control);
            Assert.IsNull(property);        // the property cannot be used as element
        }

        

    }
}
