using System.Collections.Generic;
using DotVVM.Utils.ConfigurationHost.Lookup;

namespace DotVVM.Utils.ConfigurationHost.Output.Statistics
{
    public class Statistics
    {
        public int ProjectsTotal { get; set; }
        public CsprojVersionStatistics CsprojVersionStatistics { get; set; }
        public DotvvmStatistics DotvvmStatistics { get; set; }
        public string OldestDotvvmVersion { get; set; }
        public OperationsStatistics OperationsStatistics { get; set; }
        public List<StatisticsResult> StatisticsResults { get; set; }
    }
}