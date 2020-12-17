using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet{T}" /> with refresh functionality.
    /// </summary>
    public interface IRefreshableGridViewDataSet<T> : IBaseGridViewDataSet<T>
    {
        /// <summary>
        /// Gets whether the data should be refreshed. This property is set to true automatically
        /// when paging, sorting or other options change.
        /// </summary>
        bool IsRefreshRequired { get; }

        /// <summary>
        /// Requests to reload data into the <see cref="IRefreshableGridViewDataSet{T}" />.
        /// </summary>
        void RequestRefresh();
    }
}
