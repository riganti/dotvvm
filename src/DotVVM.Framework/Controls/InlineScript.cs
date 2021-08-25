#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a script that is executed when the DotVVM framework is loaded.
    /// </summary>
    [ControlMarkupOptions(DefaultContentProperty = nameof(Script))]
    public class InlineScript : DotvvmControl
    {

        /// <summary>
        /// Gets or sets the comma-separated list of resources that should be loaded before this script is executed.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string[] Dependencies
        {
            get { return (string[])GetValue(DependenciesProperty)! ?? new string[] { ResourceConstants.DotvvmResourceName }; }
            set { SetValue(DependenciesProperty, value); }
        }
        public static readonly DotvvmProperty DependenciesProperty =
            DotvvmProperty.Register<string[], InlineScript>(c => c.Dependencies, new string[] { ResourceConstants.DotvvmResourceName });

        [MarkupOptions(MappingMode = MappingMode.InnerElement, AllowBinding = false)]
        public string? Script
        {
            get { return (string?)GetValue(ScriptProperty); }
            set { SetValue(ScriptProperty, value); }
        }
        public static readonly DotvvmProperty ScriptProperty =
            DotvvmProperty.Register<string, InlineScript>(t => t.Script);


        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            var script = Script;
            if (script is object && !string.IsNullOrWhiteSpace(script))
            {
                context.ResourceManager.AddInlineScript(script, Dependencies);
            }

            base.OnPreRender(context);
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // don't render anything
        }
    }
}
