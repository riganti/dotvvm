using System.Collections;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Defines the basic contract for DataSets.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Items" /> elements.</typeparam>
    public interface IBaseGridViewDataSet<T> : IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the items for the current page.
        /// </summary>
        new IList<T> Items { get; set; }
    }

    /// <summary>
    /// Defines the basic contract for DataSets.
    /// </summary>
    public interface IBaseGridViewDataSet
    {
        /// <summary>
        /// Gets the items for the current page.
        /// </summary>
        IList Items { get; }
    }
}
