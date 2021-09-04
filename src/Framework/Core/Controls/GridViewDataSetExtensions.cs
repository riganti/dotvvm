namespace DotVVM.Framework.Controls
{
    public static class GridViewDataSetExtensions
    {
        /// <summary>
        /// Navigates to the first page.
        /// </summary>
        public static void GoToFirstPage(this IPageableGridViewDataSet dataSet)
        {
            dataSet.GoToPage(0);
        }

        /// <summary>
        /// Navigates to the previous page if possible.
        /// </summary>
        public static void GoToPreviousPage(this IPageableGridViewDataSet dataSet)
        {
            if (!dataSet.PagingOptions.IsFirstPage)
            {
                dataSet.GoToPage(dataSet.PagingOptions.PageIndex - 1);
            }
        }

        /// <summary>
        /// Navigates to the next page if possible.
        /// </summary>
        public static void GoToNextPage(this IPageableGridViewDataSet dataSet)
        {
            if (!dataSet.PagingOptions.IsLastPage)
            {
                dataSet.GoToPage(dataSet.PagingOptions.PageIndex + 1);
            }
        }

        /// <summary>
        /// Navigates to the last page.
        /// </summary>
        public static void GoToLastPage(this IPageableGridViewDataSet dataSet)
        {
            dataSet.GoToPage(dataSet.PagingOptions.PagesCount - 1);
        }
    }
}
