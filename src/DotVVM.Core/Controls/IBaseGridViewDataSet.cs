using System;
using System.Collections;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{
    public delegate GridViewDataSetLoadedData RequestRefresh(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);
    public interface IBaseGridViewDataSet
    {
        [Bind(Direction.None)]
        RequestRefresh RequestRefresh { get; }
        void ReloadData();
        IList Items { get;}
    }
}