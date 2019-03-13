using System.Collections.Generic;

namespace DotVVM.Utils.ConfigurationHost.Output.Statistics
{
    public class OperationsStatistics
    {
        public int OperationsTotal { get; set; }
        public List<OperationStatistics> Operations { get; set; }
    }
}