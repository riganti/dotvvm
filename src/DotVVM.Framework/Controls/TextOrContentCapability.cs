using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    [DotvvmControlCapability]
    public sealed class TextOrContentCapability
    {
        public ValueOrBinding<string>? Text { get; set; }
        public List<DotvvmControl>? Content { get; set; }

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
                throw new DotvvmControlException(control, "Can not set TextOrContentCapability into the control since it already has some content.");
            control.SetProperty(textProperty, Text);
            control.Children.Clear();
            if (Content is not null)
                control.Children.Add(Content);
        }

        public IEnumerable<DotvvmControl> ToControls()
        {
            if (Text.HasValue)
                return new DotvvmControl[] { new Literal(Text.Value) };
            else
                return Content.NotNull();
        }
    }
}
