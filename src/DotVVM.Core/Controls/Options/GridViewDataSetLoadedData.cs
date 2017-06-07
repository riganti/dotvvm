using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    public class GridViewDataSetLoadedData<T> : GridViewDataSetLoadedData
    {

        public new IEnumerable<T> Items
        {
            get { return base.Items.OfType<T>(); }
            set { base.Items = value; }
        }

        public GridViewDataSetLoadedData(IEnumerable<T> items, int totalItemsCount) : base(items, totalItemsCount)
        {
        }
    }

    public abstract class GridViewDataSetLoadedData
    {
        public IEnumerable Items { get; protected set; }

        public int TotalItemsCount { get; protected set; }

        protected GridViewDataSetLoadedData(IEnumerable items, int totalItemsCount)
        {
            Items = items;
            TotalItemsCount = totalItemsCount;
        }
    }
}