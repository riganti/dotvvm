namespace DotVVM.Framework.Controls
{
    public static class GridViewDataSetExtensions
    {
        /// <summary>
        /// Navigates to the first page.
        /// </summary>
        public static void GoToPage<T>(this IPageableGridViewDataSet<T, IDataSetIndexPager<T>> dataSet, int index)
        {
            dataSet.Pager.GoToPage(index);
            (dataSet as IRefreshableGridViewDataSet<T>)?.RequestRefresh();
        }

        /// <summary>
        /// Navigates to the first page.
        /// </summary>
        public static void GoToFirstPage<T>(this IPageableGridViewDataSet<T, IDataSetIndexPager<T>> dataSet)
        {
            dataSet.GoToPage(0);
        }

        /// <summary>
        /// Navigates to the previous page if possible.
        /// </summary>
        public static void GoToPreviousPage<T>(this IPageableGridViewDataSet<T, IDataSetIndexPager<T>> dataSet)
        {
            if (dataSet.Pager.PageIndex != 0)
            {
                dataSet.GoToPage(dataSet.Pager.PageIndex - 1);
            }
        }

        /// <summary>
        /// Navigates to the next page if possible.
        /// </summary>
        public static void GoToNextPage<T>(this IPageableGridViewDataSet<T, IDataSetIndexPager<T>> dataSet)
        {
            if (dataSet.Pager.PageIndex + 1 < dataSet.Pager.PagesCount)
            {
                dataSet.GoToPage(dataSet.Pager.PageIndex + 1);
            }
        }

        /// <summary>
        /// Navigates to the last page.
        /// </summary>
        public static void GoToLastPage<T>(this IPageableGridViewDataSet<T, IDataSetIndexPager<T>> dataSet)
        {
            dataSet.GoToPage(dataSet.Pager.PagesCount - 1);
        }
    }
}
