namespace DotVVM.Utils.ConfigurationHost.Output.Statistics
{
    public class StatisticsProviderFactory
    {
        public IStatisticsProvider GetProvider(AppConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.StatisticsFolder))
            {
                return new DummyStatisticsProvider();
            }
            return new StatisticsProvider(configuration.StatisticsFolder);
        }
    }
}