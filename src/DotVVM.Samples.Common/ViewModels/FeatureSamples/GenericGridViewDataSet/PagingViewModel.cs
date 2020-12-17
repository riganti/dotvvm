using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Query;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.GenericGridViewDataSet
{
    public class PagingViewModel : DotvvmViewModelBase
    {

        public TwitterApiDataSet TwitterDataSet { get; set; } = new TwitterApiDataSet();

        public TwitterApiDataSet TwitterDataSet2 { get; set; } = new TwitterApiDataSet();

        public override async Task PreRender()
        {
            if (TwitterDataSet.IsRefreshRequired)
            {
                await LoadData(TwitterDataSet);
            }

            if (TwitterDataSet2.IsRefreshRequired)
            {
                await LoadData(TwitterDataSet2);
            }

            await base.PreRender();
        }

        public void GoToPreviousPage()
        {
            TwitterDataSet.GoToPreviousPage();
        }

        public void GoToNextPage()
        {
            TwitterDataSet.GoToNextPage();
        }



        [AllowStaticCommand]
        public static async Task<TwitterApiDataSet> GoToNextPage(DataSetPrevNextPager<Tweet, int?> pager)
        {
            // restore dataset
            var dataSet = new TwitterApiDataSet(pager);
            dataSet.GoToNextPage();

            // load
            await LoadData(dataSet);

            return dataSet;
        }

        [AllowStaticCommand]
        public static async Task<TwitterApiDataSet> GoToPreviousPage(DataSetPrevNextPager<Tweet, int?> pager)
        {
            // restore dataset
            var dataSet = new TwitterApiDataSet(pager);
            dataSet.GoToPreviousPage();

            // load
            await LoadData(dataSet);

            return dataSet;
        }

        private static async Task LoadData(TwitterApiDataSet dataSet)
        {
            var api = new ApiMocks();
            await dataSet.LoadDataAsync(
                getNextPage: token => api.TwitterApi_1_1_Statuses_UserTimeline(20, token, null),
                nextTokenProvider: response => response.Max(i => i.Id),
                getPreviousPage: token => api.TwitterApi_1_1_Statuses_UserTimeline(20, null, token),
                previousTokenProvider: response => response.Min(i => i.Id)
            );
        }
    }

    public class TwitterApiDataSet : GenericGridViewDataSet<Tweet, NopDataSetSorter<Tweet>, DataSetPrevNextPager<Tweet, int?>, NopDataSetFilter<Tweet>>
    {
        public TwitterApiDataSet() : base(new NopDataSetSorter<Tweet>(), new DataSetPrevNextPager<Tweet, int?>(), new NopDataSetFilter<Tweet>())
        {
        }

        public TwitterApiDataSet(DataSetPrevNextPager<Tweet, int?> pager) : base(new NopDataSetSorter<Tweet>(), pager, new NopDataSetFilter<Tweet>())
        {
        }

    }

    public interface IDataSetPrevNextPager<T, TToken> : IDataSetPager<T>
    {
        TToken CurrentToken { get; set; }

        PrevNextPagerDirection CurrentTokenDirection { get; set; }
        
        bool CanGoToPreviousPage { get; }

        bool CanGoToNextPage { get; }

        void GoToPreviousPage();

        void GoToNextPage();

    }

    public class DataSetPrevNextPager<T, TToken> : IDataSetPrevNextPager<T, TToken>
    {

        public TToken CurrentToken { get; set; }

        public PrevNextPagerDirection CurrentTokenDirection { get; set; }

        public TToken PreviousToken { get; set; }

        public TToken NextToken { get; set; }

        public bool CanGoToPreviousPage => !Equals(PreviousToken, default(TToken));

        public bool CanGoToNextPage => !Equals(NextToken, default(TToken));


        public IQuery<T> Apply(IQuery<T> query)
        {
            throw new NotSupportedException("The continuation token paging is not supported on IQueryable!");
        }

        public void GoToPreviousPage()
        {
            if (CanGoToPreviousPage)
            {
                CurrentToken = PreviousToken;
                CurrentTokenDirection = PrevNextPagerDirection.Previous;

                PreviousToken = default;
                NextToken = default;
            }
        }

        public void GoToNextPage()
        {
            if (CanGoToNextPage)
            {
                CurrentToken = NextToken;
                CurrentTokenDirection = PrevNextPagerDirection.Next;

                PreviousToken = default;
                NextToken = default;
            }
        }
        
    }

    public enum PrevNextPagerDirection
    {
        Next = 0,
        Previous = 1
    }

    public static class TwitterExtensions
    {
        /// <summary>
        /// Navigates to the previous page if possible.
        /// </summary>
        public static void GoToPreviousPage<T, TToken>(this IPageableGridViewDataSet<T, IDataSetPrevNextPager<T, TToken>> dataSet)
        {
            dataSet.Pager.GoToPreviousPage();
            (dataSet as IRefreshableGridViewDataSet<T>)?.RequestRefresh();
        }

        /// <summary>
        /// Navigates to the next page if possible.
        /// </summary>
        public static void GoToNextPage<T, TToken>(this IPageableGridViewDataSet<T, IDataSetPrevNextPager<T, TToken>> dataSet)
        {
            dataSet.Pager.GoToNextPage();
            (dataSet as IRefreshableGridViewDataSet<T>)?.RequestRefresh();
        }

        public static async Task LoadDataAsync<T, TToken, TResponse>(
            this IPageableGridViewDataSet<T, DataSetPrevNextPager<T, TToken>> dataSet,
            Func<TToken, Task<TResponse>> getNextPage,
            Func<TResponse, TToken> nextTokenProvider,
            Func<TToken, Task<TResponse>> getPreviousPage = null,
            Func<TResponse, TToken> previousTokenProvider = null,
            Func<TResponse, IEnumerable<T>> resultsProvider = null
        )
        {
            if (resultsProvider == null && !typeof(IEnumerable<T>).IsAssignableFrom(typeof(TResponse)))
            {
                throw new ArgumentException($"The response of getNextPage and getPreviousPage methods must be IEnumerable<{typeof(T)}>!");
            }

            TResponse response;
            if (dataSet.Pager.CurrentTokenDirection == PrevNextPagerDirection.Next)
            {
                response = await getNextPage(dataSet.Pager.CurrentToken);
            }
            else
            {
                if (getPreviousPage == null)
                {
                    throw new NotSupportedException("Loading of the previous page is not supported!");
                }
                response = await getPreviousPage(dataSet.Pager.CurrentToken);
            }

            dataSet.Pager.NextToken = nextTokenProvider(response);
            if (previousTokenProvider != null)
            {
                dataSet.Pager.PreviousToken = previousTokenProvider.Invoke(response);
            }

            if (resultsProvider != null)
            {
                // I need setter on Items
                ((dynamic)dataSet).Items = resultsProvider(response);
            }
            else
            {
                // I need setter on Items
                ((dynamic)dataSet).Items = ((IEnumerable<T>)response).ToList();
            }

            if (dataSet is IRefreshableGridViewDataSet<T> refreshableDataSet)
            {
                // I need setter on IsRefreshRequired
                ((dynamic) refreshableDataSet).IsRefreshRequired = false;
            }
        }
    }


    public class Tweet
    {
        public int Id { get; set; }

        public string Text { get; set; }
    }

    public class ApiMocks
    {


        public Task<IEnumerable<Tweet>> TwitterApi_1_1_Statuses_UserTimeline(int count, int? since_id = null, int? max_id = null)
        {
            // https://developer.twitter.com/en/docs/twitter-api/v1/tweets/timelines/api-reference/get-statuses-user_timeline
            // doesn't return total number of results, but there is since_id and max_id for going back and forth

            int first = 0;
            if (since_id == null && max_id == null)
            {
                first = 0;
            }
            else if (since_id != null)
            {
                first = since_id.Value + 1;
            }
            else
            {
                first = max_id.Value - count;
            }

            var result = Enumerable.Range(first, count).Select(i => new Tweet() { Id = i, Text = $"Tweet {i}" });
            return Task.FromResult(result);
        }

    }
}

