namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Simple token-based paging options that can navigate to the next page.
    /// Paging back is not supported by this class, you can use <see cref="NextTokenHistoryPagingOptions" />
    /// </summary>
    public class NextTokenPagingOptions : IPagingFirstPageCapability, IPagingNextPageCapability
    {
        /// <summary> The paging token of the next page. If this property is <c>null</c>, then this is the last known page. </summary>
        public string? NextPageToken { get; set; }

        /// <summary> The paging token of the currently displayed page. Equal to <c>null</c>, if this is the first page. </summary>
        public string? CurrentToken { get; set; }

        /// <summary> Gets if we are on the first page — if the <see cref="CurrentToken" /> is null. </summary>
        public bool IsFirstPage => CurrentToken == null;

        /// <summary> Navigates to the first page, resets both the current and next token to <c>null</c> </summary>
        public void GoToFirstPage()
        {
            CurrentToken = null;
            NextPageToken = null;
        }

        /// <summary> Gets if it is on the last known page — if the <see cref="NextPageToken" /> is null. </summary>
        public bool IsLastPage => NextPageToken == null;

        /// <summary> Navigates to the next page, sets the <see cref="CurrentToken" /> to the <see cref="NextPageToken" /> and resets the next token. </summary>
        public void GoToNextPage()
        {
            if (NextPageToken != null)
            {
                CurrentToken = NextPageToken;
                NextPageToken = null;
            }
        }
    }
}
