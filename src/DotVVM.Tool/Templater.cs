using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using DotVVM.Cli;
using Microsoft.Extensions.Logging;

namespace DotVVM.Tool
{
    public static class Templater
    {
        public const string PageFileExtension = ".dothtml";

        public static void AddTemplater(Command command)
        {
            var addCmd = new Command("add", "Add a DotVVM-related thingy");
            addCmd.AddArgument(new Argument<FileSystemInfo>(
                name: "target",
                getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory),
                description: "Path to a DotVVM project"));
            addCmd.AddArgument(new Argument<string>(
                name: "name",
                description: "The name of the new page"));
            addCmd.AddOption(new Option<string>(
                aliases: new [] {"-m", "--master"},
                description: "The @master page of the new page"));
            addCmd.AddOption(new Option<string>(
                aliases: new [] {"-d", "--directory"},
                getDefaultValue: () => "Views/",
                description: "The directory where the new page is to be placed"));
            addCmd.Handler = CommandHandler.Create(typeof(Templater).GetMethod(nameof(HandleAddPage))!);
            command.AddCommand(addCmd);
        }

        public static async Task HandleAddPage(
            FileSystemInfo target,
            string name,
            string? master,
            string directory,
            ILogger logger)
        {
            var metadata = await ProjectFile.LoadProjectMetadata(target, logger);
            if (metadata is null)
            {
                return;
            }

            var file = new FileInfo(Path.Combine(directory, $"{name}{PageFileExtension}"));
            if (file.Exists)
            {
                logger.LogCritical($"Page '{name}' already exists at '{file.FullName}'.");
            }
            
            var viewModelPath = Names.GetViewModelPath(file.FullName);
            var viewModelName = Names.GetViewModel(viewModelPath);
            var viewModelNamespace = Names.GetNamespace(
                viewModelPath,
                metadata.ProjectDirectory,
                metadata.RootNamespace);

            var pageTemplate = new PageTemplate()
            {
                ViewModelRootNamespace = metadata.RootNamespace,
                ViewModelName = viewModelName,
                ViewModelNamespace = viewModelNamespace
            };
            if (!string.IsNullOrEmpty(master))
            {
                pageTemplate.EmbedInMasterPage = true;
                pageTemplate.MasterPageLocation = master;
                // TODO: extract placeholder ids
                // pageTemplate.ContentPlaceHolderIds = new MasterPageBuilder().ExtractPlaceHolderIds(masterPagePath);
            }
            await File.WriteAllTextAsync(file.FullName, pageTemplate.TransformText());
        }
    }
}
