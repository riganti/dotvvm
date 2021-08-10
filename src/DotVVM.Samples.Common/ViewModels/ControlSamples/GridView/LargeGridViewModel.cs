using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class LargeGridViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<GridRow> DataSet { get; set; } = new GridViewDataSet<GridRow>();

        public override Task Init()
        {
            if(!Context.IsPostBack)
            {
                DataSet.LoadFromQueryable(DataSource().Take(1000).AsQueryable());
            }
            return base.Init();
        }


        static IEnumerable<GridRow> DataSource()
        {
            for (int i = 0; ; i++)
            {
                yield return CreateRow(i);
            }
        }
        static PropertyInfo[] props = typeof(GridRow).GetProperties();
        public static GridRow CreateRow(int i)
        {
            var row = new GridRow();
            foreach (var p in props)
            {
                p.SetValue(row, p.Name + i);
            }
            return row;
        }

        public class GridRow
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string DataA { get; set; }
            public string DataB { get; set; }
            public string DataC { get; set; }
            public string DataD { get; set; }
            public string DataE { get; set; }
            public string DataF { get; set; }
            public string DataG { get; set; }
            public string DataH { get; set; }
            public string DataI { get; set; }
            public string DataJ { get; set; }
            public string DataK { get; set; }
            public string DataL { get; set; }
            public string DataM { get; set; }
            public string DataN { get; set; }
            public string DataO { get; set; }
            public string DataP { get; set; }
            public string DataQ { get; set; }
            public string DataR { get; set; }
            public string DataS { get; set; }
            public string DataT { get; set; }
            public string DataU { get; set; }
            public string DataV { get; set; }
            public string DataW { get; set; }
            public string DataX { get; set; }
            public string DataY { get; set; }
            public string DataZ { get; set; }
        }
    }
}