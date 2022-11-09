using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Samples.Common.Views.ControlSamples.TemplateHost
{
    public class CompositeListControlWithTemplate : CompositeControl
    {
        private readonly BindingCompilationService bindingCompilationService;

        public CompositeListControlWithTemplate(BindingCompilationService bindingCompilationService)
        {
            this.bindingCompilationService = bindingCompilationService;
        }

        public IEnumerable<DotvvmControl> GetContents(
            IValueBinding<IEnumerable> dataSource,

            [ControlPropertyBindingDataContextChange("DataSource", order: 0)]
            [CollectionElementDataContextChange(order: 1)]
            ITemplate itemTemplate,

            ICommandBinding onCreateItem
        )
        {
            yield return new Framework.Controls.Repeater() {
                    ItemTemplate = new DelegateTemplate(_ => new HtmlGenericControl("div")
                        .AppendChildren(
                            new Framework.Controls.TemplateHost() { Template = itemTemplate },
                            new HtmlGenericControl("p")
                                .AppendChildren(
                                    new LinkButton() { Text = "Remove" }
                                        .SetProperty(
                                            ButtonBase.ClickProperty,
                                            new CommandBindingExpression(bindingCompilationService, contexts => {
                                                    ((dynamic)dataSource.GetBindingValue(this)).Remove((dynamic)contexts[0]);
                                                }, "564787DE-E882-4C2D-BA39-482D1AB8F0CD"))
                                )
                        )
                    ),
                    SeparatorTemplate = new DelegateTemplate(_ => new HtmlGenericControl("hr"))
                }
                .SetAttribute("class", "templated-list")
                .SetProperty(ItemsControl.DataSourceProperty, dataSource);

            yield return new HtmlGenericControl("p")
                .AppendChildren(new Button(
                    "Add item",
                    new CommandBindingExpression(bindingCompilationService, contexts => {
                                var item = onCreateItem.BindingDelegate(this);
                                ((dynamic)dataSource.GetBindingValue(this)).Add(((dynamic)item)());
                            }, "38921DE7-936D-4862-921A-5051DA0CAEB1")));
        }

    }
}
