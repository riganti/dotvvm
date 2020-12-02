#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.ResourceManagement;
using System.Collections.Immutable;

namespace DotVVM.Framework.Controls.Infrastructure
{
    /// <summary>
    /// Represents a top-level control in the control tree.
    /// </summary>
    public class DotvvmView : DotvvmControl
    {
        /// <summary>
        /// Gets or sets the collection of directives.
        /// </summary>
        public Dictionary<string, string>? Directives
        {
            get { return (Dictionary<string, string>?)GetValue(DirectivesProperty); }
            set { SetValue(DirectivesProperty, value); }
        }
        public static readonly DotvvmProperty DirectivesProperty
            = DotvvmProperty.Register<Dictionary<string, string>?, DotvvmView>(c => c.Directives, null, isValueInherited: true);



        public Type ViewModelType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmView"/> class.
        /// </summary>
        public DotvvmView()
        {
            Directives = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.ViewModelType = null!; // practically, ViewModelType should not be null. unfortunately, it is assigned late by the dotvvm compiler
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            context.ResourceManager.AddRequiredResource(ResourceConstants.DotvvmResourceName);
            if (context.Configuration.Debug)
            {
                context.ResourceManager.AddRequiredResource(ResourceConstants.DotvvmDebugResourceName);
            }
            foreach (var viewModule in this.GetValue<ImmutableList<ViewModuleReferenceInfo>>(Internal.ReferencedViewModuleInfoProperty)!)
            {
                context.ResourceManager.AddRequiredResource(viewModule.Resource);
            }
            base.OnPreRender(context);
        }
    }
}
