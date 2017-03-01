using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Compilation;
using System.Reflection;
using System.ComponentModel;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmMarkupConfiguration
    {
        /// <summary>
        /// Gets the registered control namespaces.
        /// </summary>
        [JsonProperty("controls", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<DotvvmControlConfiguration> Controls { get; private set; }

        /// <summary>
        /// Gets or sets the list of referenced assemblies.
        /// </summary>
        [JsonProperty("assemblies", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Assemblies { get; private set; }

        /// <summary>
        /// Gets a list of HTML attribute transforms.
        /// </summary>
        //[JsonProperty("htmlAttributeTransforms")]
        [JsonIgnore]
        public Dictionary<HtmlTagAttributePair, HtmlAttributeTransformConfiguration> HtmlAttributeTransforms { get; private set; }

        /// <summary>
        /// Gets a list of HTML attribute transforms.
        /// </summary>
        [JsonProperty("defaultDirectives")]
        public Dictionary<string, string> DefaultDirectives { get; private set; }

        /// <summary>
        /// Gets or sets list of namespaces imported in bindings
        /// </summary>
        [JsonProperty("importedNamespaces")]
        public List<NamespaceImport> ImportedNamespaces { get; set; } = new List<NamespaceImport>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMarkupConfiguration"/> class.
        /// </summary>
        public DotvvmMarkupConfiguration()
        {
            Controls = new List<DotvvmControlConfiguration>();
            Assemblies = new List<string>();
            DefaultDirectives = new Dictionary<string, string>();
            HtmlAttributeTransforms = new Dictionary<HtmlTagAttributePair, HtmlAttributeTransformConfiguration>()
            {
                {
                    new HtmlTagAttributePair { TagName = "a", AttributeName = "href" },
                    new HtmlAttributeTransformConfiguration() { Type = typeof(TranslateVirtualPathHtmlAttributeTransformer) }
                },
                {
                    new HtmlTagAttributePair { TagName = "link", AttributeName = "href" },
                    new HtmlAttributeTransformConfiguration() { Type = typeof(TranslateVirtualPathHtmlAttributeTransformer) }
                },
                {
                    new HtmlTagAttributePair { TagName = "img", AttributeName = "src" },
                    new HtmlAttributeTransformConfiguration() { Type = typeof(TranslateVirtualPathHtmlAttributeTransformer) }
                },
                {
                    new HtmlTagAttributePair { TagName = "iframe", AttributeName = "src" },
                    new HtmlAttributeTransformConfiguration() { Type = typeof(TranslateVirtualPathHtmlAttributeTransformer) }
                },
                {
                    new HtmlTagAttributePair { TagName = "script", AttributeName = "src" },
                    new HtmlAttributeTransformConfiguration() { Type = typeof(TranslateVirtualPathHtmlAttributeTransformer) }
                },
            };
        }

        /// <summary>
        /// Adds the assembly to the list of required assemblies.
        /// </summary>
        public void AddAssembly(string assemblyName)
        {
            if (!Assemblies.Contains(assemblyName))
            {
                Assemblies.Add(assemblyName);
            }
        }

        /// <summary>
        /// Registers markup control
        /// </summary>
        public void AddMarkupControl(string tagPrefix, string tagName, string src)
        {
            Controls.Add(new DotvvmControlConfiguration { TagPrefix = tagPrefix, TagName = tagName, Src = src });
        }

        /// <summary>
        /// Registers code controls in the specified namespace from the specified assembly
        /// </summary>
        public void AddCodeControls(string tagPrefix, string namespaceName, string assembly)
        {
            Controls.Add(new DotvvmControlConfiguration { TagPrefix = tagPrefix, Namespace = namespaceName, Assembly = assembly });
            AddAssembly(assembly);
        }

        /// <summary>
        /// Registers code controls from the same namespace and assembly as exampleControl
        /// </summary>
        public void AddCodeControls(string tagPrefix, Type exampleControl)
        {
            Controls.Add(new DotvvmControlConfiguration { TagPrefix = tagPrefix, Namespace = exampleControl.Namespace, Assembly = exampleControl.GetTypeInfo().Assembly.FullName });
            AddAssembly(exampleControl.GetTypeInfo().Assembly.FullName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use AddCodeControls instead.")]
        public void AddCodeControl(string tagPrefix, string namespaceName, string assembly) => AddCodeControls(tagPrefix, namespaceName, assembly);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use AddCodeControls instead.")]
        public void AddCodeControl(string tagPrefix, Type exampleControl) => AddCodeControls(tagPrefix, exampleControl);
    }
}