#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    public class GridViewDataSetCommandHandler : IGridViewDataSetHandler
    {
        private readonly BindingCompilationService bindingCompilationService;

        private readonly Dictionary<string, CommandBindingExpression> pagerCommands;

        public GridViewDataSetCommandHandler(BindingCompilationService bindingCompilationService)
        {
            this.bindingCompilationService = bindingCompilationService;

            pagerCommands = new Dictionary<string, CommandBindingExpression>() {
                {
                    GridViewDataSetHelper.PagerCommands.GoToThisPage,
                    new CommandBindingExpression(bindingCompilationService, h => { GridViewDataSetExtensions.GoToPage((dynamic)h[1], (int)h[0]); }, "__$DataPager_GoToThisPage")
                },
                {
                    GridViewDataSetHelper.PagerCommands.GoToFirstPage,
                    new CommandBindingExpression(bindingCompilationService, h => { GridViewDataSetExtensions.GoToFirstPage((dynamic)h[0]); }, "__$DataPager_GoToFirstPage")
                },
                {
                    GridViewDataSetHelper.PagerCommands.GoToPrevPage,
                    new CommandBindingExpression(bindingCompilationService, h => { GridViewDataSetExtensions.GoToPreviousPage((dynamic)h[0]); }, "__$DataPager_GoToPrevPage")
                },
                {
                    GridViewDataSetHelper.PagerCommands.GoToNextPage,
                    new CommandBindingExpression(bindingCompilationService, h => { GridViewDataSetExtensions.GoToNextPage((dynamic)h[0]); }, "__$DataPager_GoToNextPage")
                },
                {
                    GridViewDataSetHelper.PagerCommands.GoToLastPage,
                    new CommandBindingExpression(bindingCompilationService, h => { GridViewDataSetExtensions.GoToLastPage((dynamic)h[0]); }, "__$DataPager_GoToLastPage")
                }
            };
        }

        public virtual bool IsCommandSupported(string commandName)
        {
            return pagerCommands.ContainsKey(commandName)
                || commandName == GridViewDataSetHelper.SorterCommands.SortByColumn;
        }

        public virtual void SetCommand(string commandName, DotvvmControl control, DotvvmProperty property, Action<object[]>? parameter = null)
        {
            if (pagerCommands.TryGetValue(commandName, out var binding))
            {
                control.SetBinding(property, binding);
            }
            else if (commandName == GridViewDataSetHelper.SorterCommands.SortByColumn)
            {
                SetSortByColumnCommand(control, property, parameter!);
            }
            else
            {
                throw new NotSupportedException($"The handler {GetType()} doesn't support the '{commandName}' command!");
            }
        }

        private void SetSortByColumnCommand(DotvvmControl control, DotvvmProperty property, Action<object[]> parameter)
        {
            var bindingId = control.GetDotvvmUniqueId() + "_sortBinding";
            var binding = new CommandBindingExpression(bindingCompilationService, parameter, bindingId);
            control.SetBinding(property, binding);
        }
    }
}
