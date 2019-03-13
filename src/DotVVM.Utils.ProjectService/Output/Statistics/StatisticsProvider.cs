using Newtonsoft.Json;
using NuGet.Versioning;
using DotVVM.Utils.ConfigurationHost.Extensions;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Operations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotVVM.Utils.ConfigurationHost.Output.Statistics
{
    public class StatisticsProvider : IStatisticsProvider
    {
        public string SaveDirectory { get; set; }
        public StatisticsProvider(string saveDirectory)
        {
            if (string.IsNullOrWhiteSpace(saveDirectory))
            {
                throw new ArgumentException("Directory cannot be empty.", nameof(saveDirectory));
            }

            SaveDirectory = Path.Combine(saveDirectory,
                $"{Constants.StatisticsDirectoryName}_{DateTime.Now:yyMMdd_HH-mm-ss}");
            Directory.CreateDirectory(SaveDirectory);
        }

        public void AddOperationResult(IResult searchResult, OperationResult operationResult)
        {
            if (searchResult is StatisticsResult statisticsResult)
            {
                statisticsResult.OperationResults.Add(operationResult);
            }
        }

        public IEnumerable<IResult> TransformResults(IEnumerable<IResult> results)
        {
            return TransformResultsToStatisticsResults(results);
        }

        public string GetDotvvmCompilerLogFileArgument(IResult result)
        {
            var outputFileName = Path.GetFileNameWithoutExtension(result.CsprojFullName) + ".json";
            var compilerDirectory = Path.Combine(SaveDirectory, Constants.DotvvmCompilerOutputDirectory);
            Directory.CreateDirectory(compilerDirectory);
            var path = Path.Combine(compilerDirectory, outputFileName);
            if (File.Exists(path))
            {
                path = RenameFile(path);
            }

            return $"--logfile {path}";
        }

        private string RenameFile(string fullPath)
        {
            int count = 2;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string directoryName = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = $"{fileNameOnly}({count++})";
                newFullPath = Path.Combine(directoryName, tempFileName + extension);
            }

            return newFullPath;
        }

        private IEnumerable<StatisticsResult> TransformResultsToStatisticsResults(IEnumerable<IResult> results)
        {
            return results.Select(r => r.ToStatisticsResult());
        }

        public void SaveStatistics(IEnumerable<IResult> results)
        {
            var resultList = TransformResultsToStatisticsResults(results).ToList();
            var statistics = new Statistics
            {
                ProjectsTotal = resultList.Count,
                CsprojVersionStatistics = GetCsprojVersionStatistics(resultList),
                DotvvmStatistics = GetDotvvmStatistics(resultList),
                OldestDotvvmVersion = GetOldestDotvvmVersion(resultList),
                OperationsStatistics = GetOperationsStatistics(resultList),
                StatisticsResults = resultList
            };

            SaveToFile(statistics);
        }

        private OperationsStatistics GetOperationsStatistics(IReadOnlyCollection<StatisticsResult> resultList)
        {
            return new OperationsStatistics()
            {
                OperationsTotal = resultList.SelectMany(r => r.OperationResults).Count(),
                Operations = resultList.SelectMany(r => r.OperationResults).GroupBy(r => r.OperationName).Select(r => new OperationStatistics()
                {
                    Name = r.Key,
                    Skipped = r.Count(o => !o.Executed),
                    Executed = r.Count(o => o.Executed),
                    Successful = r.Count(o => o.Successful)
                }).ToList()
            };
        }

        private CsprojVersionStatistics GetCsprojVersionStatistics(IReadOnlyCollection<IResult> resultList)
        {
            return new CsprojVersionStatistics()
            {
                OlderProjectSystem = resultList.Count(r => r.CsprojVersion == CsprojVersion.OlderProjectSystem),
                DotNetSdk = resultList.Count(r => r.CsprojVersion == CsprojVersion.DotNetSdk)
            };
        }

        private string GetOldestDotvvmVersion(IReadOnlyCollection<IResult> resultList)
        {
            return resultList.SelectMany(r =>
                    r.DotvvmPackagesVersions.Where(IsBasicDotvvmPackage))
                .OrderBy(v => new NuGetVersion(v.Version)).FirstOrDefault()?.Version;
        }

        private DotvvmStatistics GetDotvvmStatistics(IReadOnlyCollection<IResult> resultList)
        {
            return new DotvvmStatistics()
            {
                DotvvmProjects = GetProjectsCount(resultList, ""),
                BusinessPackProjects = GetProjectsCount(resultList, "BusinessPack"),
                BootstrapProjects = GetProjectsCount(resultList, "Bootstrap"),
                AspNetCoreProjects = GetProjectsCount(resultList, "AspNetCore"),
                OwinProjects = GetProjectsCount(resultList, "Owin")
            };
        }

        private int GetProjectsCount(IReadOnlyCollection<IResult> resultList, string filter)
        {
            return resultList.Count(r => r.DotvvmPackagesVersions.Any(p => p.Name.Contains(filter)));
        }

        private void SaveToFile(Statistics statistics)
        {
            using (var streamWriter = new StreamWriter(GetStatisticsFilePath()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(statistics, Formatting.Indented));
            }
        }

        private string GetStatisticsFilePath()
        {
            return Path.Combine(SaveDirectory, Constants.StatisticsFileName);
        }

        private bool IsBasicDotvvmPackage(PackageVersion packageVersion)
        {
            var packages = new List<string>()
            {
                "DotVVM",
                "DotVVM.AspNetCore",
                "DotVVM.Core",
                "DotVVM.Owin",
                "DotVVM.Templates",
                "DotVVM.Compiler.Light",
                "DotVVM.CommandLine"
            };
            return packages.Contains(packageVersion.Name);
        }
    }
}