using System;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.Cli;
using DotVVM.CommandLine.Core.Templates;

namespace DotVVM.CommandLine.Commands.Handlers
{
    public class AddViewModelCommandHandler : CommandBase
    {
        public override string Name => "Add ViewModel";

        public override string[] Usages => new[] { "dotvvm add viewmodel <NAME>", "dotvvm avm <NAME>" };

        public override bool TryConsumeArgs(Arguments args, ProjectMetadataJson dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "add", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "viewmodel", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "avm", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(1);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args, ProjectMetadataJson dotvvmProjectMetadata)
        {
            var name = args[0];
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidCommandUsageException("You have to specify the NAME.");
            }
            name = PathHelpers.EnsureFileExtension(name, "cs");

            if (PathHelpers.IsCurrentDirectory(dotvvmProjectMetadata.ProjectDirectory) && !name.Contains("/") && !name.Contains("\\"))
            {
                name = "ViewModels/" + name;
            }

            CreateViewModel(name, dotvvmProjectMetadata);
        }

        private void CreateViewModel(string viewModelPath, ProjectMetadataJson dotvvmProjectMetadata)
        {
            var viewModelName = NamingHelpers.GetClassNameFromPath(viewModelPath);
            var viewModelNamespace = NamingHelpers.GetNamespaceFromPath(viewModelPath, dotvvmProjectMetadata.ProjectDirectory, dotvvmProjectMetadata.RootNamespace);

            // create viewmodel
            var viewModelTemplate = new ViewModelTemplate() {
                ViewModelName = viewModelName,
                ViewModelNamespace = viewModelNamespace
                // TODO: BaseViewModel
            };
            FileSystemHelpers.WriteFile(viewModelPath, viewModelTemplate.TransformText());
        }
    }
}
