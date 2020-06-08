using System.Collections.Generic;

namespace DotVVM.Utils.ProjectService.Output.Statistics
{
    public class Statistics
    {
        public int ProjectsTotal { get; set; }
        public CsprojVersionStatistics CsprojVersionStatistics { get; set; }
        public DotvvmStatistics DotvvmStatistics { get; set; }
        public string OldestDotvvmVersion { get; set; }
        public OperationsStatistics OperationsStatistics { get; set; }
        public List<ResolvedProjectStatistics> StatisticsResults { get; set; }
    }
}