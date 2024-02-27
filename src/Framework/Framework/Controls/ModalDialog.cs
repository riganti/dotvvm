using System;
using System.Net;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a HTML native dialog element, it is opened using the showModal function when the <see cref="Open" /> property is set to true
    /// </summary>
    /// <remarks>
    /// * Non-modal dialogs may be simply binding the attribute of the HTML dialog element
    /// * The dialog may be closed by button with formmethod="dialog", when ESC is pressed, or when the backdrop is clicked if <see cref="CloseOnBackdropClick" /> = true
    /// </remarks>
    [ControlMarkupOptions()]
    public class ModalDialog : HtmlGenericControl
    {
        public ModalDialog()
            : base("dialog", false)
        {
        }

        /// <summary> A value indicating whether the dialog is open. The value can either be a boolean or an object (not false or not null -> shown). On close, the value is written back into the Open binding. </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public object? Open
        {
            get { return GetValue(OpenProperty); }
            set { SetValue(OpenProperty, value); }
        }
        public static readonly DotvvmProperty OpenProperty =
            DotvvmProperty.Register<object, ModalDialog>(nameof(Open), null);

        /// <summary> Add an event handler which closes the dialog when the backdrop is clicked. </summary>
        public bool CloseOnBackdropClick
        {
            get { return (bool?)GetValue(CloseOnBackdropClickProperty) ?? false; }
            set { SetValue(CloseOnBackdropClickProperty, value); }
        }
        public static readonly DotvvmProperty CloseOnBackdropClickProperty =
            DotvvmProperty.Register<bool, ModalDialog>(nameof(CloseOnBackdropClick), false);

        /// <summary> Triggered when the dialog is closed. Called only if it was closed by user input, not by <see cref="Open"/> change. </summary>
        public Command? Close
        {
            get { return (Command?)GetValue(CloseProperty); }
            set { SetValue(CloseProperty, value); }
        }
        public static readonly DotvvmProperty CloseProperty =
            DotvvmProperty.Register<Command, ModalDialog>(nameof(Close));

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var valueBinding = GetValueBinding(OpenProperty);
            if (valueBinding is {})
            {
                writer.AddKnockoutDataBind("dotvvm-modal-open", this, valueBinding);
            }
            else if (!(Open is false or null))
            {
                // we have to use the binding handler instead of `open` attribute, because we need to call the showModal function
                writer.AddKnockoutDataBind("dotvvm-modal-open", "true");
            }

            if (GetValueOrBinding<bool>(CloseOnBackdropClickProperty) is {} x && !x.ValueEquals(false))
            {
                writer.AddKnockoutDataBind("dotvvm-modal-backdrop-close", x.GetJsExpression(this));
            }

            if (GetCommandBinding(CloseProperty) is {} close)
            {
                var postbackScript = KnockoutHelper.GenerateClientPostBackScript(nameof(Close), close, this, returnValue: null);
                writer.AddAttribute("onclose", "if (event.target.returnValue!=\"_dotvvm_modal_supress_onclose\") {" + postbackScript + "}");
            }

            base.AddAttributesToRender(writer, context);
        }
    }
}
