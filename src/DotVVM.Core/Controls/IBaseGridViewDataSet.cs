using System;
using System.Collections;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Controls
{

    /// <summary>
    /// Represents a common core functionality for GridViewDataSets.
    /// </summary>
    public interface IBaseGridViewDataSet
    {
        
        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        IList Items { get;}
    }
}