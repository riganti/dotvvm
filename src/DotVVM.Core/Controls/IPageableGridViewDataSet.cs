namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Extends the <see cref="IBaseGridViewDataSet{T}" /> with paging functionality.
    /// </summary>
    public interface IPageableGridViewDataSet<T, out TPager> : IBaseGridViewDataSet<T>
        where TPager: IDataSetPager<T>
    {
        /// <summary>
        /// Gets the settings for paging.
        /// </summary>
        TPager Pager { get; }
    }

    // public interface IPageableGridViewDataSet<T> : IPageableGridViewDataSet<T, IDataSetPager<T>> { }
}
