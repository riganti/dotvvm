using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.Infrastructure;

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
        public string Dependencies
        {
            get { return (string)GetValue(DependenciesProperty); }
            set { SetValue(DependenciesProperty, value); }
        }
        public static readonly DotvvmProperty DependenciesProperty =
            DotvvmProperty.Register<string, InlineScript>(c => c.Dependencies, Constants.DotvvmResourceName);

        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public string Script
        {
            get { return (string)GetValue(ScriptProperty); }
            set { SetValue(ScriptProperty, value); }
        }
        public static readonly DotvvmProperty ScriptProperty =
            DotvvmProperty.Register<string, InlineScript>(t => t.Script);


        internal override void OnPreRenderComplete(IDotvvmRequestContext context)
        {
            EnsureControlHasId();

            var dep = Dependencies?.Split(',') ?? new string[] { Constants.DotvvmResourceName };
            context.ResourceManager.AddStartupScript("inlinescript_" + ID, Script, dep);

            base.OnPreRenderComplete(context);
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            // don't render anything
        }
    }
}
