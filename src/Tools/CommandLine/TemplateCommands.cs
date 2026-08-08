using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using DotVVM.CommandLine;
using DotVVM.CommandLine.Templates;
using DotVVM.Framework.Utils;
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
            var nameArg = new Argument<string>("name")
            {
                Description = "The name of the new thingy"
            };
            var masterOpt = new Option<string>("--master", "-m")
            {
                Description = "The @master page of the new page"
            };
            var viewsDirectoryOpt = new Option<string>("--directory", "-d")
            {
                DefaultValueFactory = _ => "Views/",
                Description = "The directory where the new page is to be placed"
            };
            var viewModelsDirectoryOpt = new Option<string>("--directory", "-d")
            {
                DefaultValueFactory = _ => "ViewModels/",
                Description = "The directory where the new ViewModel is to be placed"
            };
            var controlsDirectoryOpt = new Option<string>("--directory", "-d")
            {
                DefaultValueFactory = _ => "Controls/",
                Description = "The directory where the new control is to be placed"
            };
            var codeBehindOpt = new Option<bool>("--code-behind", "-c")
            {
                Description = "Create a C# code-behind class for the control"
            };
            var baseOpt = new Option<string>("--base", "-b")
            {
                Description = "The base class of the ViewModel"
            };

            var pageCmd = new Command("page", "Add a page")
            {
                nameArg, masterOpt, viewsDirectoryOpt
            };
            pageCmd.SetAction(parseResult =>
            {
                if (!CommandLineExtensions.TryGetProject(parseResult, out var project, out var logger))
                    return 1;

                HandleAddPage(project, parseResult.GetRequiredValue(nameArg), parseResult.GetValue(masterOpt),
                    parseResult.GetValue(viewsDirectoryOpt) ?? "Views/", logger);
                return 0;
            });

            var masterCmd = new Command("master", "Add a master page")
            {
                nameArg, masterOpt, viewsDirectoryOpt
            };
            masterCmd.SetAction(parseResult =>
            {
                if (!CommandLineExtensions.TryGetProject(parseResult, out var project, out var logger))
                    return 1;

                HandleAddMaster(project, parseResult.GetRequiredValue(nameArg), parseResult.GetValue(masterOpt),
                    parseResult.GetValue(viewsDirectoryOpt) ?? "Views/", logger);
                return 0;
            });

            var viewModelCmd = new Command("viewmodel", "Add a ViewModel")
            {
                nameArg, viewModelsDirectoryOpt, baseOpt
            };
            viewModelCmd.SetAction(parseResult =>
            {
                if (!CommandLineExtensions.TryGetProject(parseResult, out var project, out var logger))
                    return 1;

                HandleAddViewModel(project, parseResult.GetRequiredValue(nameArg), parseResult.GetValue(viewModelsDirectoryOpt) ?? "ViewModels/",
                    parseResult.GetValue(baseOpt), logger);
                return 0;
            });

            var controlCmd = new Command("control", "Add a control")
            {
                nameArg, controlsDirectoryOpt, codeBehindOpt
            };
            controlCmd.SetAction(parseResult =>
            {
                if (!CommandLineExtensions.TryGetProject(parseResult, out var project, out var logger))
                    return 1;

                HandleAddControl(project, parseResult.GetRequiredValue(nameArg), parseResult.GetValue(controlsDirectoryOpt) ?? "Controls/",
                    parseResult.GetValue(codeBehindOpt), logger);
                return 0;
            });

            var addCmd = new Command("add", "Add a DotVVM-related thingy");
            addCmd.AddTargetArgument();
            addCmd.AddRange(pageCmd, masterCmd, viewModelCmd, controlCmd);
            command.Subcommands.Add(addCmd);
        }

        public static void HandleAddPage(
            DotvvmProject metadata,
            string name,
            string? master,
            string directory,
            ILogger logger,
            bool isMaster = false)
        {
            var projectDir = Path.GetDirectoryName(metadata.ProjectFilePath)!;
            var extension = isMaster ? MasterPageFileExtension : PageFileExtension;
            var file = GetFile(projectDir, directory, name, extension, logger);
            if (file is null)
            {
                return;
            }

            var viewModelName = Names.GetViewModel(name);
            var viewModelNamespace = Names.GetNamespace(
                file.DirectoryName.NotNull(),
                projectDir,
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
            DotvvmProject metadata,
            string name,
            string? master,
            string directory,
            ILogger logger)
        {
            HandleAddPage(metadata, name, master, directory, logger, true);
        }

        public static void HandleAddViewModel(
            DotvvmProject metadata,
            string name,
            string directory,
            string? @base,
            ILogger logger)
        {
            var projectDir = Path.GetDirectoryName(metadata.ProjectFilePath)!;
            var file = GetFile(projectDir, directory, name, ViewModelFileExtensions, logger);
            if (file is null)
            {
                return;
            }

            var viewModelName = Names.GetViewModel(name);
            var viewModelNamespace = Names.GetNamespace(
                file.DirectoryName.NotNull(),
                projectDir,
                metadata.RootNamespace);

            var viewModelText = ViewModelTemplate.TransformText(
                @namespace: viewModelNamespace,
                name: viewModelName,
                @base: @base);
            File.WriteAllText(file.FullName, viewModelText);
        }

        public static void HandleAddControl(
            DotvvmProject metadata,
            string name,
            string directory,
            bool codeBehind,
            ILogger logger)
        {
            var projectDir = Path.GetDirectoryName(metadata.ProjectFilePath)!;
            var file = GetFile(projectDir, directory, name, ControlFileExtension, logger);
            if (file is null)
            {
                return;
            }

            var @namespace = Names.GetNamespace(
                file.DirectoryName.NotNull(),
                projectDir,
                metadata.RootNamespace);

            var codeBehindName = codeBehind ? $"{@namespace}.{name}" : null;

            var controlText = ControlTemplate.TransformText(codeBehindName);
            File.WriteAllText(file.FullName, controlText);

            if (codeBehind)
            {
                var codeBehindFile = GetFile(
                    projectDir,
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

            Directory.CreateDirectory(file.DirectoryName.NotNull());
            return file;
        }
    }
}
