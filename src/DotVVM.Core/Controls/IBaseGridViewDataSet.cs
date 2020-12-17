using System.Collections;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Defines the basic contract for DataSets.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Items" /> elements.</typeparam>
    public interface IBaseGridViewDataSet<out T>
    {
        /// <summary>
        /// Gets the items for the current page.
        /// </summary>
        IReadOnlyList<T> Items { get; }
    }
}
