using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using System.Security.Claims;
using System.Linq;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Binding;
using FastExpressionCompiler;
using DotVVM.Framework.Binding.Expressions;
using System.Reflection.Metadata;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class DataPagerTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task CommandDataPager()
        {
            var r = await cth.RunPage(typeof(GridViewModel), """
                <dot:DataPager DataSet={value: Customers} />
                """
            );

            var commandExpressions = r.Commands
                .Select(c => (c.control, c.command, str: c.command.GetProperty<ParsedExpressionBindingProperty>().Expression.ToCSharpString().Trim().TrimEnd(';')))
                .OrderBy(c => c.str)
                .ToArray();
            check.CheckLines(commandExpressions.GroupBy(c => c.command).Select(c => c.First().str), checkName: "command-bindings", fileExtension: "txt");

            check.CheckString(r.FormattedHtml, fileExtension: "html");

            var nextPage = commandExpressions.Single(c => c.str.Contains(".GoToNextPage()"));
            var prevPage = commandExpressions.Single(c => c.str.Contains(".GoToPreviousPage()"));
            var firstPage = commandExpressions.Single(c => c.str.Contains(".GoToFirstPage()"));
            var lastPage = commandExpressions.Single(c => c.str.Contains(".GoToLastPage()"));

            await r.RunCommand((CommandBindingExpression)nextPage.command, nextPage.control);
            Assert.AreEqual(1, (int)r.ViewModel.Customers.PagingOptions.PageIndex);
        }

        [TestMethod]
        public async Task StaticCommandPager()
        {
            var r = await cth.RunPage(typeof(GridViewModel), """
                <dot:DataPager DataSet={value: Customers} LoadData={staticCommand: RootViewModel.LoadCustomers} />
                """
            );
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task StaticCommandApendablePager()
        {
            var r = await cth.RunPage(typeof(GridViewModel), """
                <dot:AppendableDataPager DataSet={value: Customers} LoadData={staticCommand: RootViewModel.LoadCustomers}>
                    <LoadTemplate>
                        <div DataContext={value: 1}>
                            <dot:Button Text="Load more" Click="{staticCommand: _dataPager.Load()}" />
                        </div>
                    </LoadTemplate>
                    <EndTemplate> end </EndTemplate>
                </dot:AppendableDataPager>
                """
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");

            var commandExpressions = r.Commands
                .Select(c => (c.control, c.command, str: c.command.GetProperty<ParsedExpressionBindingProperty>().Expression.ToCSharpString().Trim().TrimEnd(';')))
                .OrderBy(c => c.str)
                .ToArray();
            check.CheckLines(commandExpressions.GroupBy(c => c.command).Select(c => c.First().str), checkName: "command-bindings", fileExtension: "txt");

            var nextPage = commandExpressions.Single(c => c.str.Contains(".GoToNextPage()"));
            var prevPage = commandExpressions.Single(c => c.str.Contains(".GoToPreviousPage()"));
            var firstPage = commandExpressions.Single(c => c.str.Contains(".GoToFirstPage()"));
            var lastPage = commandExpressions.Single(c => c.str.Contains(".GoToLastPage()"));

            await r.RunCommand((CommandBindingExpression)nextPage.command, nextPage.control);
            Assert.AreEqual(1, (int)r.ViewModel.Customers.PagingOptions.PageIndex);

        }

        public class GridViewModel: DotvvmViewModelBase
        {
            public GridViewDataSet<CustomerData> Customers { get; set; } = new GridViewDataSet<CustomerData>()
            {
                PagingOptions = new PagingOptions()
                {
                    PageSize = 5
                },
            };

            public override async Task PreRender()
            {
                if (Customers.IsRefreshRequired)
                {
                    Customers.LoadFromQueryable(
                        Enumerable.Range(0, 100).Select(i => new CustomerData() { Id = i, Name = "Name" + i }).AsQueryable()
                    );
                }
            }

            public class CustomerData
            {
                public int Id { get; set; }
                [Required]
                public string Name { get; set; }
            }

            [AllowStaticCommand]
            public static GridViewDataSetResult<CustomerData, NoFilteringOptions, SortingOptions, PagingOptions> LoadCustomers(GridViewDataSetOptions request)
            {
                throw new NotImplementedException();
            }
        }
    }
}
