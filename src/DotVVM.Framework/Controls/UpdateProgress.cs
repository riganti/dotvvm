#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Utils;
using System.Text.RegularExpressions;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Container for content that will be displayed for the time the page is doing a postback.
    /// </summary>
    public class UpdateProgress : HtmlGenericControl
    {
        /// <summary>
        /// Gets or sets the delay (in ms) after which the content inside UpdateProgress control is shown
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public int Delay
        {
            get { return (int)GetValue(DelayProperty)!; }
            set { SetValue(DelayProperty, value); }
        }
        public static readonly DotvvmProperty DelayProperty =
            DotvvmProperty.Register<int, UpdateProgress>(t => t.Delay, 0);

        /// <summary>
        /// Gets or sets the comma-separated names of PostBack.ConcurrencyQueue names for which this control should be enabled.
        /// If not set, all queues are included automatically.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string[]? IncludedQueues
        {
            get { return (string[]?)GetValue(IncludedQueuesProperty); }
            set { SetValue(IncludedQueuesProperty, value); }
        }
        public static readonly DotvvmProperty IncludedQueuesProperty
            = DotvvmProperty.Register<string[]?, UpdateProgress>(c => c.IncludedQueues, null);

        /// <summary>
        /// Gets or sets the comma-separated names of PostBack.ConcurrencyQueue names that should be ignored by this control.
        /// If you don't want to exclude any queue, use an empty string.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string[]? ExcludedQueues
        {
            get { return (string[]?)GetValue(ExcludedQueuesProperty); }
            set { SetValue(ExcludedQueuesProperty, value); }
        }
        public static readonly DotvvmProperty ExcludedQueuesProperty
            = DotvvmProperty.Register<string[]?, UpdateProgress>(c => c.ExcludedQueues, null);


        public UpdateProgress() : base("div")
        {
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddKnockoutDataBind("dotvvm-UpdateProgress-Visible", "true");

            if (Delay != 0)
            {
                writer.AddAttribute("data-delay", Delay.ToString());
            }
            if (IncludedQueues != null)
            {
                writer.AddAttribute("data-included-queues", string.Join(",", IncludedQueues));
            }
            if (ExcludedQueues != null)
            {
                writer.AddAttribute("data-excluded-queues", string.Join(",", ExcludedQueues));
            }

            base.AddAttributesToRender(writer, context);
        }



        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            var delayProperty = control.GetValue(DelayProperty) as ResolvedPropertyValue;
            if (delayProperty != null)
            {
                if ((int)delayProperty.Value < 0)
                {
                    yield return new ControlUsageError($"{nameof(Delay)} cannot be set to negative number.");
                }
            }

            // validate queue settings
            var includedQueues = (control.GetValue(IncludedQueuesProperty) as ResolvedPropertyValue)?.Value as string[];
            var excludedQueues = (control.GetValue(ExcludedQueuesProperty) as ResolvedPropertyValue)?.Value as string[];
            if (includedQueues != null && excludedQueues != null)
            {
                yield return new ControlUsageError($"The {nameof(IncludedQueues)} and {nameof(ExcludedQueues)} cannot be used together!");
            }
            if (includedQueues != null && !ValidateQueueNames(includedQueues))
            {
                yield return new ControlUsageError($"The {nameof(IncludedQueues)} must contain comma-separated list of queue names (which can contain alphanumeric characters, underscore or dash)!");
            }
            if (excludedQueues != null && !ValidateQueueNames(excludedQueues))
            {
                yield return new ControlUsageError($"The {nameof(ExcludedQueues)} must contain comma-separated list of queue names (which can contain alphanumeric characters, underscore or dash)!");
            }
        }

        private static bool ValidateQueueNames(string[] queues)
        {
            return queues.All(NamingUtils.IsValidConcurrencyQueueName);
        }
    }
}
