using System;
using DotVVM.Cli;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.CommandLine.Core.Templates;

namespace DotVVM.CommandLine.Commands.Handlers
{
    public class AddControlCommandHandler : CommandBase
    {
        public override string Name => "Add Control";

        public override string[] Usages => new []{ "dotvvm add control <NAME> [-c|--code|--codebehind]", "dotvvm ac <NAME> [-c|--code|--codebehind]"};

        public override bool TryConsumeArgs(Arguments args, ProjectMetadataJson dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "add", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "control", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "ac", StringComparison.CurrentCultureIgnoreCase))
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
            name = PathHelpers.EnsureFileExtension(name, "dotcontrol");

            var codeBehind = args.HasOption("-c", "--code", "--codebehind");

            CreateControl(name, codeBehind, dotvvmProjectMetadata);
        }

        private void CreateControl(string viewPath, bool createCodeBehind, ProjectMetadataJson dotvvmProjectMetadata)
        {
            var codeBehindPath = PathHelpers.ChangeExtension(viewPath, "cs");
            var codeBehindClassName = NamingHelpers.GetClassNameFromPath(viewPath);
            var codeBehindClassNamespace = NamingHelpers.GetNamespaceFromPath(viewPath, dotvvmProjectMetadata.ProjectDirectory, dotvvmProjectMetadata.RootNamespace);

            // create control
            var controlTemplate = new ControlTemplate()
            {
                CreateCodeBehind = createCodeBehind
            };
            if (createCodeBehind)
            {
                controlTemplate.CodeBehindClassName = codeBehindClassName;
                controlTemplate.CodeBehindClassNamespace = codeBehindClassNamespace;
                controlTemplate.CodeBehindClassRootNamespace = dotvvmProjectMetadata.RootNamespace;
            }
            FileSystemHelpers.WriteFile(viewPath, controlTemplate.TransformText());

            // create code behind
            if (createCodeBehind)
            {
                var codeBehindTemplate = new ControlCodeBehindTemplate()
                {
                    CodeBehindClassNamespace = codeBehindClassNamespace,
                    CodeBehindClassName = codeBehindClassName
                };
                FileSystemHelpers.WriteFile(codeBehindPath, codeBehindTemplate.TransformText());
            }
        }
    }
}
