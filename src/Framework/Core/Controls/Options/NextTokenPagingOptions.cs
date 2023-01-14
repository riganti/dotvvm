namespace DotVVM.Framework.Controls
{
    // TODO: comments, mention that null in NextPageToken means there is no next page
    public class NextTokenPagingOptions : IPagingFirstPageCapability, IPagingNextPageCapability
    {
        public string? NextPageToken { get; set; }

        public string? CurrentToken { get; set; }

        public bool IsFirstPage => CurrentToken == null;

        public void GoToFirstPage() => CurrentToken = null;

        public bool IsLastPage => NextPageToken == null;

        public void GoToNextPage()
        {
            if (NextPageToken != null)
            {
                CurrentToken = NextPageToken;
            }
        }
    }
}
