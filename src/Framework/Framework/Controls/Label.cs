using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary> Render an HTML `&lt; label for=X &gt;` element. The for=X should be a control ID that will be looked up and adjusted to match the control even when it's in Repeater </summary>
    public sealed class Label: HtmlGenericControl
    {
        public Label() : base("label", false)
        {
            LifecycleRequirements = ControlLifecycleRequirements.None;
        }

        public Label(string forId) : this()
        {
            For = forId;
        }

        [MarkupOptions(Required = true, AllowBinding = false)]
        public string For
        {
            get { return (string)GetValue(ForProperty)!; }
            set { SetValue(ForProperty, value ?? throw new ArgumentNullException(nameof(value))); }
        }
        public static readonly DotvvmProperty ForProperty =
            DotvvmProperty.Register<string, Label>(nameof(For));


        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // I don't want to go reimplementing or refactoring the ID generation logic, so this is the simple option :]

            var dummyControl = new Label();
            dummyControl.SetValueRaw(DotvvmControl.IDProperty, this.GetValueRaw(ForProperty));
            var dummyIndex = this.Children.Count;
            this.Children.Add(dummyControl);
            var id = dummyControl.CreateClientId();
            this.Children.RemoveAt(dummyIndex);
            if (id is string idStr)
            {
                writer.AddAttribute("for", idStr);
            }
            else if (id is {})
            {
                // let the html generic control evaluate the binding
                this.Attributes.Add("for", id);
            }

            base.AddAttributesToRender(writer, context);
        }
    }
}
