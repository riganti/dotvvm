using System;
using System.Linq;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Utils;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class CompositeControlSample: CompositeControl
    {
        public static DotvvmControl GetContents(
            IValueBinding<IEnumerable<object>> dataSource,

            [ControlPropertyBindingDataContextChange("dataSource")]
            [CollectionElementDataContextChange(1)]
            IValueBinding<string> titleBinding,

            [ControlPropertyBindingDataContextChange("dataSource")]
            [CollectionElementDataContextChange(1)]
            IValueBinding<int?> numberBinding,

            HtmlCapability html,

            [DotvvmControlCapability(prefix: "inner-li:")]
            HtmlCapability liHtml
        )
        {
            return new Repeater() {
                WrapperTagName = "ul",
                ItemTemplate = new DelegateTemplate(_ => new HtmlGenericControl("li") {
                    HtmlCapability = liHtml,
                    Children = {
                        new Literal(titleBinding),
                        new Literal(": "),
                        new TextBox()
                            .SetBinding(t => t.Text, numberBinding)
                            .SetValue(t => t.SelectAllOnFocus, true)
                    }
                }),
                HtmlCapability = html
            }
            .SetBinding(r => r.DataSource, dataSource);
        }
    }
}