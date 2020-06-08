using System;
using System.IO;
using System.Linq;
using DotVVM.CommandLine.Commands.Logic.BindingRedirects;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Providers;
#if NETCOREAPP
using Microsoft.Extensions.DependencyModel;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public abstract class DotvvmToolProvider
    {
        protected string CombineNugetPath(IResolvedProjectMetadata metadata, string mainModuleRelativePath)
        {
            if (metadata.DotvvmPackageNugetFolders == null || !metadata.DotvvmPackageNugetFolders.Any()) return null;

            var nugetPath =
                metadata.DotvvmPackageNugetFolders.FirstOrDefault(s =>
                    File.Exists(Path.Combine(s, mainModuleRelativePath)));

            return !string.IsNullOrWhiteSpace(nugetPath) ? Path.Combine(nugetPath, mainModuleRelativePath) : null;
        }

        protected string CombineDotvvmRepositoryRoot(IResolvedProjectMetadata metadata,
            ProjectDependency dotvvmDependency, string toolsDotvvmCompilerExe)
        {
            var dotvvmAbsPath = Path.IsPathRooted(dotvvmDependency.ProjectPath)
                ? dotvvmDependency.ProjectPath
                : Path.Combine(metadata.ProjectRootDirectory, dotvvmDependency.ProjectPath);
            var dotvvmAbsDir = new FileInfo(Path.GetFullPath(dotvvmAbsPath)).Directory;
            if (dotvvmAbsDir == null) return null;
            var executablePath = Path.GetFullPath(Path.Combine(dotvvmAbsDir.Parent.FullName, toolsDotvvmCompilerExe));
            return File.Exists(executablePath) ? executablePath : null;

        }

        protected DotvvmToolMetadata CreateMetadataOrDefault(string mainModule, DotvvmToolExecutableVersion version)
        {
            if (string.IsNullOrWhiteSpace(mainModule)) return null;
            return new DotvvmToolMetadata() { MainModulePath = mainModule, Version = version };
        }

        protected DotvvmToolMetadata PrepareTool(IResolvedProjectMetadata projectMeta, DotvvmToolMetadata toolMetadata)
        {
            if (projectMeta == null) throw new ArgumentNullException(nameof(projectMeta));
            if (toolMetadata == null) throw new ArgumentNullException(nameof(toolMetadata));

            var name = Path.GetFileName(toolMetadata.MainModulePath);
            var dir = new DirectoryInfo(Path.GetDirectoryName(toolMetadata.MainModulePath));
            var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Console.WriteLine($@"Copying files into temp directory: {tmp}");

            Directory.CreateDirectory(tmp);
            //Now Create all of the directories
            foreach (var dirPath in dir.GetDirectories("*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.FullName.Replace(dir.FullName, tmp));

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in dir.GetFiles("*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath.FullName, newPath.FullName.Replace(dir.FullName, tmp), true);

            var result = new DotvvmToolMetadata() {
                MainModulePath = Path.Combine(tmp, name),
                Version = toolMetadata.Version,
                TempDirectory = tmp
            };


            if (toolMetadata.Version == DotvvmToolExecutableVersion.DotNetCore)
            {
                CreateDepsFile(result, projectMeta);
            }
            else
            {
                CreateWebConfigFile(result, projectMeta);
            }

            return result;
        }

        protected virtual void CreateWebConfigFile(DotvvmToolMetadata result, IResolvedProjectMetadata loadedAssembly)
        {
            var path = WebConfig.Locate(result.MainModulePath);
            var webConfig = WebConfig.Load(path);
            WebConfig.UpdateRedirects(webConfig, loadedAssembly);
            // TODO: Save, but where?
            // WebConfig.Save(webConfig, ???);
        }

        protected virtual void CreateDepsFile(DotvvmToolMetadata toolMetadata, IResolvedProjectMetadata loadedAssembly)
        {

#if NETCOREAPP
            //TODO: this part is not needed right now. When the loading graph is not working this part is the first one to finish.
            var depsProjectFile = Path.Combine(Path.GetDirectoryName(loadedAssembly.AssemblyPath), Path.GetFileNameWithoutExtension(loadedAssembly.AssemblyPath) + ".runtimeconfig.json");
            var depsToolFile = Path.Combine(Path.GetDirectoryName(toolMetadata.MainModulePath), Path.GetFileNameWithoutExtension(toolMetadata.MainModulePath) + ".runtimeconfig.json");
            File.Copy(depsProjectFile, depsToolFile, true);
            //using (var stream = new StreamReader(depsProjectFile))
            //{
            //    string result = null;
            //    using (var depsToolStream = new StreamReader(depsToolFile))
            //    {
            //        var projsDeps = JObject.Load(new JsonTextReader(stream));
            //        var toolsDeps = JObject.Load(new JsonTextReader(depsToolStream));
            //        var jo = new JObject();
            //        jo.Add(projsDeps.Property("libraries"));
            //        toolsDeps.Merge(jo, new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Merge });
            //        result = JsonConvert.SerializeObject(toolsDeps);
            //    }
            //    File.WriteAllText(depsToolFile, result);
            //}
#endif
        }

        public static void Clean(DotvvmToolMetadata compilerMeta)
        {
            try
            {
                if (compilerMeta.TempDirectory == null) return;
                var d = new DirectoryInfo(compilerMeta.TempDirectory);
                d.Delete(true);
            }
            catch (Exception e)
            {
                // ignored
            }
        }


        public DotvvmToolMetadata GetPreparedTool(IResolvedProjectMetadata metadata)
        {
            var toolMeta = GetToolMetadata(metadata);
            if (toolMeta == null) return null;
            Console.WriteLine($@"Found tool: {toolMeta.MainModulePath}");

            return PrepareTool(metadata, toolMeta);
        }

        protected abstract DotvvmToolMetadata GetToolMetadata(IResolvedProjectMetadata metadata);
    }
}
