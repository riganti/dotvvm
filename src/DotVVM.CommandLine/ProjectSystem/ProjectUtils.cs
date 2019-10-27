using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Locator;

namespace DotVVM.CommandLine.ProjectSystem
{
    public static class ProjectUtils
    {
        public const string AssemblyNameProperty = "AssemblyName";
        public const string CollectPackageReferencesTarget = "CollectPackageReferences";
        public const string ProjectReferenceItemType = "ProjectReference";
        public const string ReferenceItemType = "Reference";

        public static IResolvedProjectMetadata ResolveMetadata(string projectDirectory)
        {
            var csproj = new DirectoryInfo(projectDirectory).GetFiles("*.csproj").Single();
            var project = new Project(csproj.FullName);
            var projectInstance = BuildManager.DefaultBuildManager.GetProjectInstanceForBuild(project);

            var dependencies = new List<ProjectDependency>();
            dependencies.AddRange(projectInstance.GetItems(ProjectReferenceItemType)
                .Select(r => new ProjectDependency {
                    IsProjectReference = true,
                    ProjectPath = r.EvaluatedInclude,
                    Version = r.GetMetadata("Version")?.EvaluatedValue
                }));
            dependencies.AddRange(projectInstance.GetItems(ReferenceItemType)
                .Select(r => new ProjectDependency {
                    Name = r.EvaluatedInclude,
                    Version = r.GetMetadata("Version")?.EvaluatedValue
                }));
            var packagesRequest = new BuildRequestData(projectInstance, new[] { CollectPackageReferencesTarget });
            var packagesResult = BuildManager.DefaultBuildManager.Build(null, packagesRequest);
            if (packagesResult.OverallResult == BuildResultCode.Success)
            {
                dependencies.AddRange(packagesResult.ResultsByTarget[CollectPackageReferencesTarget].Items
                    .Select(i => new ProjectDependency {
                        Name = i.ItemSpec,
                        Version = i.GetMetadata("Version")
                    }));
            }
            var assemblyName = projectInstance.GetProperty(AssemblyNameProperty)?.EvaluatedValue;
            return new ResolvedProjectMetadata {
                AssemblyName = assemblyName,
                AssemblyPath = null, // TODO
                CsprojFullName = csproj.FullName,
                CsprojVersion = default, // TODO: Maybe use UsingMicrosoftNETSdk or NETCoreSdkVersion
                DotvvmPackageNugetFolders = null, // TODO
                DotvvmProjectDependencies = dependencies,
                PackageNugetFolders = null, // TODO
                ProjectRootDirectory = projectDirectory, // TODO
                RunDotvvmCompiler = false, // TODO
                TargetFramework = default // TODO
            };
        }
    }
}
