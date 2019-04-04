using System;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Commands.Logic;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.CommandLine.Core.Metadata;
using DotVVM.CommandLine.Core.Templates;

namespace DotVVM.CommandLine.Commands.Handlers
{
    public class AddMasterPageCommandHandler : CommandBase
    {
        public override string Name => "Add Master Page";

        public override string[] Usages => new []{"dotvvm add master <NAME> [-m|--master|--masterpage <MASTERPAGE>]","dotvvm am <NAME> [-m|--master|--masterpage <MASTERPAGE>]"};

        public override bool TryConsumeArgs(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "add", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "master", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "am", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(1);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            var name = args[0];
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidCommandUsageException("You have to specify the NAME.");
            }

            if (PathHelpers.IsCurrentDirectory(dotvvmProjectMetadata.ProjectDirectory) && !name.Contains("/") && !name.Contains("\\"))
            {
                name = "Views/" + name;
            }
            name = PathHelpers.EnsureFileExtension(name, "dotmaster");

            var masterPage = args.GetOptionValue("-m", "--master", "--masterpage");
            if (!string.IsNullOrEmpty(masterPage))
            {
                masterPage = PathHelpers.EnsureFileExtension(masterPage, "dotmaster");

                if (PathHelpers.IsCurrentDirectory(dotvvmProjectMetadata.ProjectDirectory) && !masterPage.Contains("/") && !masterPage.Contains("\\"))
                {
                    masterPage = "Views/" + masterPage;
                }
            }

            CreatePage(name, masterPage, dotvvmProjectMetadata);
        }

        private void CreatePage(string viewPath, string masterPagePath, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            var viewModelPath = NamingHelpers.GenerateViewModelPath(viewPath);
            var viewModelName = NamingHelpers.GetClassNameFromPath(viewModelPath);
            var viewModelNamespace = NamingHelpers.GetNamespaceFromPath(viewModelPath, dotvvmProjectMetadata.ProjectDirectory, dotvvmProjectMetadata.RootNamespace);

            // create page
            var pageTemplate = new PageTemplate()
            {
                ViewModelRootNamespace = dotvvmProjectMetadata.RootNamespace,
                ViewModelName = viewModelName,
                ViewModelNamespace = viewModelNamespace,
                IsMasterPage = true
            };
            if (!string.IsNullOrEmpty(masterPagePath))
            {
                pageTemplate.EmbedInMasterPage = true;
                pageTemplate.MasterPageLocation = masterPagePath;
                pageTemplate.ContentPlaceHolderIds = new MasterPageBuilder().ExtractPlaceHolderIds(masterPagePath);
            }
            FileSystemHelpers.WriteFile(viewPath, pageTemplate.TransformText());

            // create viewmodel
            var viewModelTemplate = new ViewModelTemplate()
            {
                ViewModelName = viewModelName,
                ViewModelNamespace = viewModelNamespace
                // TODO: BaseViewModel
            };
            FileSystemHelpers.WriteFile(viewModelPath, viewModelTemplate.TransformText());
        }
    }
}
