using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class GridViewDataSetTests
    {
        private readonly GridViewDataSetBindingProvider commandProvider;
        private readonly GridViewDataSet<TestDto> vm;
        private readonly DataContextStack dataContextStack;
        private readonly DotvvmControl control;

        public GridViewDataSetTests()
        {
            var bindingService = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetRequiredService<BindingCompilationService>();
            commandProvider = new GridViewDataSetBindingProvider(bindingService);

            // build viewmodel
            vm = new GridViewDataSet<TestDto>()
            {
                PagingOptions =
                {
                    PageSize = 10,
                    TotalItemsCount = 65
                },
                SortingOptions = { SortExpression = nameof(TestDto.Id) }
            };

            // create page
            dataContextStack = DataContextStack.Create(vm.GetType());
            control = new DotvvmView() { DataContext = vm };
        }

        [TestMethod]
        public void GridViewDataSet_DataPagerCommands_Command()
        {
            // create control with page index data context
            var pageIndexControl = new PlaceHolder();
            var pageIndexDataContextStack = DataContextStack.Create(typeof(int), dataContextStack);
            pageIndexControl.SetDataContextType(pageIndexDataContextStack);
            pageIndexControl.SetProperty(p => p.DataContext, ValueOrBinding<int>.FromBoxedValue(1));
            control.Children.Add(pageIndexControl);

            // get pager commands
            var commands = commandProvider.GetDataPagerCommands(dataContextStack, GridViewDataSetCommandType.Command);

            // test evaluation of commands
            Assert.IsNotNull(commands.GoToLastPage);
            vm.IsRefreshRequired = false;
            commands.GoToLastPage.Evaluate(control);
            Assert.AreEqual(6, vm.PagingOptions.PageIndex);
            Assert.IsTrue(vm.IsRefreshRequired);

            Assert.IsNotNull(commands.GoToPreviousPage);
            vm.IsRefreshRequired = false;
            commands.GoToPreviousPage.Evaluate(control);
            Assert.AreEqual(5, vm.PagingOptions.PageIndex);
            Assert.IsTrue(vm.IsRefreshRequired);

            Assert.IsNotNull(commands.GoToNextPage);
            vm.IsRefreshRequired = false;
            commands.GoToNextPage.Evaluate(control);
            Assert.AreEqual(6, vm.PagingOptions.PageIndex);
            Assert.IsTrue(vm.IsRefreshRequired);

            Assert.IsNotNull(commands.GoToFirstPage);
            vm.IsRefreshRequired = false;
            commands.GoToFirstPage.Evaluate(control);
            Assert.AreEqual(0, vm.PagingOptions.PageIndex);
            Assert.IsTrue(vm.IsRefreshRequired);

            Assert.IsNotNull(commands.GoToPage);
            vm.IsRefreshRequired = false;
            commands.GoToPage.Evaluate(pageIndexControl);
            Assert.AreEqual(1, vm.PagingOptions.PageIndex);
            Assert.IsTrue(vm.IsRefreshRequired);
        }

        [TestMethod]
        public void GridViewDataSet_GridViewCommands_Command()
        {
            // get gridview commands
            var commands = commandProvider.GetGridViewCommands(dataContextStack, GridViewDataSetCommandType.Command);

            // test evaluation of commands
            Assert.IsNotNull(commands.SetSortExpression);
            vm.IsRefreshRequired = false;
            commands.SetSortExpression.Evaluate(control, _ => "Name");
            Assert.AreEqual("Name", vm.SortingOptions.SortExpression);
            Assert.IsFalse(vm.SortingOptions.SortDescending);
            Assert.IsTrue(vm.IsRefreshRequired);

            vm.IsRefreshRequired = false;
            commands.SetSortExpression.Evaluate(control, _ => "Name");
            Assert.AreEqual("Name", vm.SortingOptions.SortExpression);
            Assert.IsTrue(vm.SortingOptions.SortDescending);
            Assert.IsTrue(vm.IsRefreshRequired);

            vm.IsRefreshRequired = false;
            commands.SetSortExpression.Evaluate(control, _ => "Id");
            Assert.AreEqual("Id", vm.SortingOptions.SortExpression);
            Assert.IsFalse(vm.SortingOptions.SortDescending);
            Assert.IsTrue(vm.IsRefreshRequired);
        }


        [TestMethod]
        public void GridViewDataSet_DataPagerCommands_StaticCommand()
        {
            // get pager commands
            var commands = commandProvider.GetDataPagerCommands(dataContextStack, GridViewDataSetCommandType.StaticCommand);

            var goToFirstPage = CompileBinding(commands.GoToFirstPage);
        }

        private string CompileBinding(ICommandBinding staticCommand)
        {
            return KnockoutHelper.GenerateClientPostBackExpression(
                "",
                staticCommand,
                new Literal(),
                new PostbackScriptOptions(
                    allowPostbackHandlers: false,
                    returnValue: null,
                    commandArgs: CodeParameterAssignment.FromLiteral("commandArguments")
                ));
        }

        class TestDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
