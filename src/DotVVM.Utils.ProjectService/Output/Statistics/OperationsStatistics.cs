using System.Collections.Generic;

namespace DotVVM.Utils.ProjectService.Output.Statistics
{
    public class OperationsStatistics
    {
        public int OperationsTotal { get; set; }
        public List<OperationStatistics> Operations { get; set; }
    }
}