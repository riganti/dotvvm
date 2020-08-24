using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using DotVVM.CommandLine;
using DotVVM.CommandLine.Templates;
using Microsoft.Extensions.Logging;

namespace DotVVM.CommandLine
{
    public static class TemplateCommands
    {
        public const string PageFileExtension = ".dothtml";
        public const string MasterPageFileExtension = ".dotmaster";
        public const string ControlFileExtension = ".dotcontrol";
        public const string ViewModelFileExtensions = ".cs";
        public const string CodeBehindExtension = ".cs";

        public static void AddTemplateCommands(this Command command)
        {
            var targetArg = new Argument<FileSystemInfo>(
                name: CommandLineExtensions.TargetArg,
                getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory),
                description: "Path to a DotVVM project");
            var nameArg = new Argument<string>(
                name: "name",
                description: "The name of the new thingy");
            var masterOpt = new Option<string>(
                aliases: new[] { "-m", "--master" },
                description: "The @master page of the new page");
            var viewsDirectoryOpt = new Option<string>(
                aliases: new[] { "-d", "--directory" },
                getDefaultValue: () => "Views/",
                description: "The directory where the new page is to be placed");
            var viewModelsDirectoryOpt = new Option<string>(
                aliases: new[] { "-d", "--directory" },
                getDefaultValue: () => "ViewModels/",
                description: "The directory where the new viewModel is to be placed");
            var controlsDirectoryOpt = new Option<string>(
                aliases: new[] { "-d", "--directory" },
                getDefaultValue: () => "Controls/",
                description: "The directory where the new control is to be placed");
            var codeBehindOpt = new Option<bool>(
                aliases: new [] {"-c", "--code-behind"},
                description: "Creates a C# code-behind class for the control");
            var baseOpt = new Option<string>(
                aliases: new [] {"-b", "--base"},
                description: "The base class of the ViewModel");

            var pageCmd = new Command("page", "Add a page")
            {
                nameArg, masterOpt, viewsDirectoryOpt
            };
            pageCmd.Handler = CommandHandler.Create(typeof(TemplateCommands).GetMethod(nameof(HandleAddPage))!);

            var masterCmd = new Command("master", "Add a master page")
            {
                nameArg, masterOpt, viewsDirectoryOpt
            };
            masterCmd.Handler = CommandHandler.Create(typeof(TemplateCommands).GetMethod(nameof(HandleAddMaster))!);

            var viewModelCmd = new Command("viewmodel", "Add a ViewModel")
            {
                nameArg, viewModelsDirectoryOpt, baseOpt
            };
            viewModelCmd.Handler = CommandHandler.Create(typeof(TemplateCommands).GetMethod(nameof(HandleAddViewModel))!);

            var controlCmd = new Command("control", "Add a control")
            {
                nameArg, controlsDirectoryOpt, codeBehindOpt
            };
            controlCmd.Handler = CommandHandler.Create(typeof(TemplateCommands).GetMethod(nameof(HandleAddControl))!);

            var addCmd = new Command("add", "Add a DotVVM-related thingy")
            {
                targetArg, pageCmd, masterCmd, viewModelCmd, controlCmd
            };
            command.AddCommand(addCmd);
        }

        public static void HandleAddPage(
            ProjectMetadataOld metadata,
            string name,
            string? master,
            string directory,
            ILogger logger,
            bool isMaster = false)
        {
            var extension = isMaster ? MasterPageFileExtension : PageFileExtension;
            var file = GetFile(metadata.ProjectDirectory, directory, name, extension, logger);
            if (file is null)
            {
                return;
            }

            var viewModelName = Names.GetViewModel(name);
            var viewModelNamespace = Names.GetNamespace(
                file.DirectoryName,
                metadata.ProjectDirectory,
                metadata.RootNamespace);

            var placeholderIds = master is object
                ? Dothtml.ExtractPlaceholderIds(master)
                : null;
            var pageText = PageTemplate.TransformText(
                viewModel: $"{viewModelNamespace}.{viewModelName}",
                master: master,
                isMaster: isMaster,
                contentPlaceholderIds: placeholderIds);
            File.WriteAllText(file.FullName, pageText);
        }

        public static void HandleAddMaster(
            ProjectMetadataOld metadata,
            string name,
            string? master,
            string directory,
            ILogger logger)
        {
            HandleAddPage(metadata, name, master, directory, logger, true);
        }

        public static void HandleAddViewModel(
            ProjectMetadataOld metadata,
            string name,
            string directory,
            string? @base,
            ILogger logger)
        {
            var file = GetFile(metadata.ProjectDirectory, directory, name, ViewModelFileExtensions, logger);
            if (file is null)
            {
                return;
            }

            var viewModelName = Names.GetViewModel(name);
            var viewModelNamespace = Names.GetNamespace(
                file.DirectoryName,
                metadata.ProjectDirectory,
                metadata.RootNamespace);

            var viewModelText = ViewModelTemplate.TransformText(
                @namespace: viewModelNamespace,
                name: viewModelName,
                @base: @base);
            File.WriteAllText(file.FullName, viewModelText);
        }

        public static void HandleAddControl(
            ProjectMetadataOld metadata,
            string name,
            string directory,
            bool codeBehind,
            ILogger logger)
        {
            var file = GetFile(metadata.ProjectDirectory, directory, name, ControlFileExtension, logger);
            if (file is null)
            {
                return;
            }

            var @namespace = Names.GetNamespace(
                file.DirectoryName,
                metadata.ProjectDirectory,
                metadata.RootNamespace);

            var codeBehindName = codeBehind ? $"{@namespace}.{name}" : null;

            var controlText = ControlTemplate.TransformText(codeBehindName);
            File.WriteAllText(file.FullName, controlText);

            if (codeBehind)
            {
                var codeBehindFile = GetFile(
                    metadata.ProjectDirectory,
                    directory,
                    name,
                    CodeBehindExtension,
                    logger);
                if (codeBehindFile is null)
                {
                    return;
                }

                var codeBehindText = ControlCodeBehindTemplate.TransformText(@namespace, name);
                File.WriteAllText(codeBehindFile.FullName, codeBehindText);
            }
        }

        private static FileInfo? GetFile(
            string projectDirectory,
            string directory,
            string name,
            string extension,
            ILogger logger)
        {
            var file = new FileInfo(Path.Combine(
                projectDirectory,
                Path.Combine(
                    directory,
                    $"{name}{extension}")));
            if (file.Exists)
            {
                logger.LogCritical($"File '{file}' already exists.");
                return null;
            }

            Directory.CreateDirectory(file.DirectoryName);
            return file;
        }
    }
}
