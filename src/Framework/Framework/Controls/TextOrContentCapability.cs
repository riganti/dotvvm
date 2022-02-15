using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    [DotvvmControlCapability]
    public sealed record TextOrContentCapability
    {
        public ValueOrBinding<string>? Text { get; init; }
        public List<DotvvmControl>? Content { get; init; }

        public TextOrContentCapability() { }
        public TextOrContentCapability(ValueOrBinding<string> text)
        {
            this.Text = text;
        }
        public TextOrContentCapability(IEnumerable<DotvvmControl> content)
        {
            this.Content = content.ToList();
        }
        public TextOrContentCapability(params DotvvmControl[] content)
        {
            this.Content = content.ToList();
        }

        public static TextOrContentCapability FromChildren(DotvvmControl control, DotvvmProperty textProperty)
        {
            var text = control.GetValueRaw(textProperty);
            if (text is object && !"".Equals(text))
                return new TextOrContentCapability { Text = ValueOrBinding<string>.FromBoxedValue(text) };
            else
                return new TextOrContentCapability { Content = control.Children.ToList() };
        }

        public void WriteToChildren(DotvvmControl control, DotvvmProperty textProperty)
        {
            if (!control.HasOnlyWhiteSpaceContent())
                throw new DotvvmControlException(control, "Cannot set TextOrContentCapability into the control since it already has some content.");
            control.SetProperty(textProperty, Text);
            control.Children.Clear();
            if (Content is not null)
                control.Children.Add(Content);
        }

        public IEnumerable<DotvvmControl> ToControls()
        {
            if(Text.HasValue && Content == null)
            {
                throw new DotvvmControlException(control, "Either Text property or Content must be set.");
            }
            
            if (Text.HasValue)
                return new DotvvmControl[] { new Literal(Text.Value) };
            else
                return Content.NotNull();
        }
    }
}
