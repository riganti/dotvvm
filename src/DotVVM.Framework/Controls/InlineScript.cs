using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ResourceManagement;

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
        public string Dependencies
        {
            get { return (string)GetValue(DependenciesProperty); }
            set { SetValue(DependenciesProperty, value); }
        }
        public static readonly DotvvmProperty DependenciesProperty =
            DotvvmProperty.Register<string, InlineScript>(c => c.Dependencies, ResourceConstants.DotvvmResourceName);

        [MarkupOptions(MappingMode = MappingMode.InnerElement, AllowBinding = false)]
        public string Script
        {
            get { return (string)GetValue(ScriptProperty); }
            set { SetValue(ScriptProperty, value); }
        }
        public static readonly DotvvmProperty ScriptProperty =
            DotvvmProperty.Register<string, InlineScript>(t => t.Script);


        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            var dep = Dependencies?.Split(',') ?? new string[] { ResourceConstants.DotvvmResourceName };
            context.ResourceManager.AddStartupScript("inlinescript_" + (ClientID ?? GetScriptUniqueId()), Script, dep);

            base.OnPreRender(context);
        }

        private string GetScriptUniqueId()
        {
            var uniqueId = GetDotvvmUniqueId() as string;
            if (uniqueId == null) throw new DotvvmControlException(this, $"Can not generate ID for InlineScript inside client template. Try to assign it ID manually.");
            return uniqueId;
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // don't render anything
        }
    }
}
