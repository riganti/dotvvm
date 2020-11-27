namespace DotVVM.Utils.ProjectService.Output.Statistics
{
    public class StatisticsProviderFactory
    {
        public IStatisticsProvider GetProvider(string folderDestination)
        {
            if (string.IsNullOrWhiteSpace(folderDestination))
            {
                return new DummyStatisticsProvider();
            }
            return new StatisticsProvider(folderDestination);
        }
    }
}
