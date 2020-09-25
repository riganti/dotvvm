using System;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Tests.Common.ControlTests
{
    [TestClass]
    public class GridViewTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.GetSerializationMapper()
                .Map(typeof(RowModel), map => {
                    map.Property(nameof(RowModel.StringProperty)).Name = "strprop";
                });
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task GridView_Simple()
        {
            var r = await cth.RunPage(typeof(GridTestViewModel), @"
                <!-- Server rendering -->
                <dot:GridView DataSource={value: DataSet} RenderSettings.Mode=Server>
                    <dot:GridViewTextColumn HeaderText='Id' AllowSorting ValueBinding={value: Id} />
                    <dot:GridViewTextColumn ValueBinding={value: StringProperty}>
                        <HeaderTemplate>String [{{value: Integer}}]</HeaderTemplate>
                    </dot:GridViewTextColumn>
                    <dot:GridViewCheckBoxColumn HeaderText=Bool ValueBinding={value: BoolProperty} />
                </dot:GridView>
                <!-- Client rendering -->
                <dot:GridView DataSource={value: DataSet} RenderSettings.Mode=Client>
                    <dot:GridViewTextColumn HeaderText='Id' AllowSorting ValueBinding={value: Id} />
                    <dot:GridViewTextColumn ValueBinding={value: StringProperty}>
                        <HeaderTemplate>String [{{value: Integer}}]</HeaderTemplate>
                    </dot:GridViewTextColumn>
                    <dot:GridViewCheckBoxColumn HeaderText=Bool ValueBinding={value: BoolProperty} />
                </dot:GridView>
               "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task GridView_InlineEditing()
        {
            var r = await cth.RunPage(typeof(GridTestViewModel), @"
                <dot:GridView DataSource={value: DataSet} InlineEditing>
                    <dot:GridViewTextColumn HeaderText='Id' IsEditable=false ValueBinding={value: Id} />
                    <dot:GridViewTextColumn ValueBinding={value: StringProperty}/>
                    <dot:GridViewCheckBoxColumn HeaderText=Bool ValueBinding={value: BoolProperty} />
                </dot:GridView>
               "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }
        [TestMethod]
        public async Task GridView_SortCommand()
        {
            var r = await cth.RunPage(typeof(GridTestViewModel), @"
                <dot:GridView DataSource={value: DataSet}>
                    <dot:GridViewTextColumn HeaderText='Id' AllowSorting ValueBinding={value: Id} />
                    <dot:GridViewTextColumn HeaderText='StringProperty' AllowSorting ValueBinding={value: StringProperty} />
                </dot:GridView>
            ");

            // uncomment to find the command ids
            // Console.WriteLine(r.FormattedHtml);

            await r.RunCommand("c7_c7a0a0a0a0_sortBinding"); // first column
            Assert.AreEqual("Id", (string)r.ViewModel.DataSet.SortingOptions.SortExpression);

            await r.RunCommand("c7_c7a0a0a1a0_sortBinding"); // second column
            Assert.AreEqual("StringProperty", (string)r.ViewModel.DataSet.SortingOptions.SortExpression);
            Assert.IsFalse((bool)r.ViewModel.DataSet.SortingOptions.SortDescending);

            Assert.IsTrue(((string)r.ViewModel.DataSet.Items[0].strprop).CompareTo((string)r.ViewModel.DataSet.Items[1].strprop) < 0);
            await r.RunCommand("c7_c7a0a0a1a0_sortBinding"); // second column
            Assert.AreEqual("StringProperty", (string)r.ViewModel.DataSet.SortingOptions.SortExpression);
            Assert.IsTrue((bool)r.ViewModel.DataSet.SortingOptions.SortDescending);
            Assert.IsTrue(((string)r.ViewModel.DataSet.Items[0].strprop).CompareTo((string)r.ViewModel.DataSet.Items[1].strprop) > 0);
        }

        [TestMethod]
        public async Task GridView_SortChangedHasPrecendenceOverGridDataSet()
        {
            var r = await cth.RunPage(typeof(GridTestViewModel), @"
                <dot:GridView DataSource={value: DataSet} SortChanged={command: NopSortChanged}>
                    <dot:GridViewTextColumn HeaderText='Id' AllowSorting ValueBinding={value: Id} />
                    <dot:GridViewTextColumn HeaderText='StringProperty' AllowSorting ValueBinding={value: StringProperty} />
                </dot:GridView>
            ");

            // uncomment to find the command ids
            // Console.WriteLine(r.FormattedHtml);

            await r.RunCommand("c7_c7a0a0a0a0_sortBinding"); // first column
            Assert.AreEqual(null, (string)r.ViewModel.DataSet.SortingOptions.SortExpression);
        }

        public class GridTestViewModel: DotvvmViewModelBase
        {
            public int Integer { get; set; } = 10000000;

            public GridViewDataSet<RowModel> EmptyDataSet { get; set; } = new GridViewDataSet<RowModel>();
            public GridViewDataSet<RowModel> DataSet { get; set; } = new GridViewDataSet<RowModel> {
                RowEditOptions = {
                    PrimaryKeyPropertyName = "Id"
                }
            };

            public void NopSortChanged(string column)
            {

            }

            public override Task PreRender()
            {
                DataSet.LoadFromQueryable(new [] {
                    new RowModel {
                        BoolProperty = true,
                        StringProperty = "row 1",
                        Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
                    },
                    new RowModel {
                        BoolProperty = false,
                        StringProperty = "row 2",
                        Id = Guid.Parse("00000000-0000-0000-0000-000000000002")
                    }
                }.AsQueryable());
                return base.PreRender();
            }
        }

        public class RowModel
        {
            public bool BoolProperty { get; set; }
            public string StringProperty { get; set; }
            public Guid Id { get; set; }
        }
    }
}
