using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Hosting;

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
        public Dictionary<string, string> Directives { get; set; }

        public Type ViewModelType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmView"/> class.
        /// </summary>
        public DotvvmView()
        {
            Directives = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            ResourceDependencies.Add(Constants.DotvvmResourceName);
            ResourceDependencies.Add(Constants.DotvvmValidationResourceName);
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            if (context.Configuration.Debug)
                ResourceDependencies.Add(Constants.DotvvmDebugResourceName);
            base.OnPreRender(context);
        }
    }
}
