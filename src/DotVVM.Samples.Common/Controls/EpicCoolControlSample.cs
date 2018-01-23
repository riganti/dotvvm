using System;
using System.Linq;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.Infrastructure;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class EpicCoolControlSample: EpicCoolControl
    {
        public static DotvvmControl GetContents(
            IValueBinding<IEnumerable<object>> dataSource,

            [ControlPropertyBindingDataContextChange("dataSource")]
            [CollectionElementDataContextChange(1)]
            IValueBinding<string> titleBinding,

            [ControlPropertyBindingDataContextChange("dataSource")]
            [CollectionElementDataContextChange(1)]
            IValueBinding<int?> numberBinding
        )
        {
            return new Repeater() {
                WrapperTagName = "ul",
                ItemTemplate = new DelegateTemplate(_ => new HtmlGenericControl("li") {
                    Children = {
                        new Literal(titleBinding),
                        new Literal(": "),
                        new TextBox()
                            .SetBinding(t => t.Text, numberBinding)
                            .SetValue(t => t.SelectAllOnFocus, true)
                    }
                })
            }.SetBinding(r => r.DataSource, dataSource);
        }
    }
}