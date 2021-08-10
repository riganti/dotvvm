#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Declares a command that can be exposed to JavaScript code under a specified name.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class NamedCommand : DotvvmControl 
    {

        /// <summary>
        /// Gets or sets the name of the command to be used in JavaScript code.
        /// </summary>
        [MarkupOptions(Required = true, AllowBinding = false)]
        public string? Name
        {
            get { return (string?)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DotvvmProperty NameProperty =
            DotvvmProperty.Register<string?, NamedCommand>(c => c.Name);

        /// <summary>
        /// Gets or sets a command that will be invoked.
        /// </summary>
        [MarkupOptions(Required = true, AllowHardCodedValue = false)]
        public ICommandBinding? Command
        {
            get { return (ICommandBinding?)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DotvvmProperty CommandProperty
            = DotvvmProperty.Register<ICommandBinding?, NamedCommand>(c => c.Command, null);
        
        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var viewModule = GetValue<ViewModuleReferenceInfo>(Internal.ReferencedViewModuleInfoProperty);
            if (viewModule == null)
            {
                throw new DotvvmControlException(this, "No module was registered. The NamedCommand control can not be used in pages without the @js directive.");
            }

            var options = new PostbackScriptOptions(
                returnValue: true,
                commandArgs: new CodeParameterAssignment("args", OperatorPrecedence.Max),
                elementAccessor: "$element"
            );
            var command = KnockoutHelper.GenerateClientPostBackExpression(nameof(Command), Command!, this, options);
            command = $"function(...args) {{ return ({command}); }}";

            var viewIdJs = ViewModuleHelpers.GetViewIdJsExpression(viewModule, this);
            writer.WriteKnockoutDataBindComment("dotvvm-named-command", $"{{ viewIdOrElement: {viewIdJs}, name: {KnockoutHelper.MakeStringLiteral(Name!)}, command: {command} }}");
            
            base.RenderBeginTag(writer, context);
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderEndTag(writer, context);

            writer.WriteKnockoutDataBindEndComment();
        }


        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if (!control.TreeRoot.TryGetProperty(Internal.ReferencedViewModuleInfoProperty, out var _))
            {
                yield return new ControlUsageError("The NamedCommand control can be used only in pages or controls that have the @js directive.");
            }
        }
    }
}
