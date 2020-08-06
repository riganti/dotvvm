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
            var targetArg = new Argument<FileSystemInfo>(
                name: "target",
                getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory),
                description: "Path to a DotVVM project");
            var nameArg = new Argument<string>(
                name: "name",
                description: "The name of the new thingy");
            var masterOpt = new Option<string>(
                aliases: new[] { "-m", "--master" },
                description: "The @master page of the new page");
            var directoryOpt = new Option<string>(
                aliases: new[] { "-d", "--directory" },
                getDefaultValue: () => "Views/",
                description: "The directory where the new page is to be placed");
            var codeBehindOpt = new Option<bool>(
                aliases: new [] {"-c", "--code-behind"},
                description: "Creates a C# code-behind class for the control");

            var pageCmd = new Command("page", "Add a page")
            {
                nameArg, masterOpt, directoryOpt
            };
            pageCmd.Handler = CommandHandler.Create(typeof(Templater).GetMethod(nameof(HandleAddPage))!);

            var masterCmd = new Command("master", "Add a master page")
            {
                nameArg, masterOpt, directoryOpt
            };

            var viewModelCmd = new Command("viewmodel", "Add a ViewModel")
            {
                nameArg
            };

            var controlCmd = new Command("control", "Add a control")
            {
                nameArg, codeBehindOpt
            };

            var addCmd = new Command("add", "Add a DotVVM-related thingy")
            {
                targetArg, pageCmd, masterCmd, viewModelCmd, controlCmd
            };
            command.AddCommand(addCmd);
        }

        public static async Task HandleAddPage(
            FileSystemInfo target,
            string name,
            string? master,
            string directory,
            ILogger logger)
        {
            var metadata = await ProjectFile.GetProjectMetadata(target, logger);
            if (metadata is null || metadata.ProjectDirectory is null || metadata.RootNamespace is null)
            {
                return;
            }

            var file = new FileInfo(Path.Combine(
                metadata.ProjectDirectory,
                Path.Combine(
                    directory,
                    $"{name}{PageFileExtension}")));
            if (file.Exists)
            {
                logger.LogCritical($"Page '{name}' already exists at '{file.FullName}'.");
            }

            var viewModelName = Names.GetViewModel(name);
            var viewModelNamespace = Names.GetNamespace(
                file.DirectoryName,
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

        public static async Task HandleAddMaster(
            FileSystemInfo target,
            string name,
            string? master,
            string directory,
            ILogger logger)
        {
            
        }

        public static async Task HandleAddViewModel(
            FileSystemInfo target,
            string name,
            string? master,
            string directory,
            ILogger logger)
        {
            
        }

        public static async Task HandleAddControl(
            FileSystemInfo target,
            string name,
            string? master,
            string directory,
            ILogger logger)
        {
            
        }
    }
}
