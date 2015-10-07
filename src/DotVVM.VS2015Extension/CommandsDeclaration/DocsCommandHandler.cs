using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Process = System.Diagnostics.Process;

namespace DotVVM.VS2015Extension.CommandsDeclaration
{
    public class DocsCommandHandler
    {
        public void SetupCommands(DTE2 dte, OleMenuCommandService mcs)
        {
            var docsCommand = new CommandID(PackageGuids.ButtonsGuid, PackageIds.ShowDocs);
            var command = new OleMenuCommand(Execute, docsCommand);
            mcs.AddCommand(command);
        }

        public void Execute(object sender, EventArgs eventArgs)
        {
            Process.Start("http://www.dotvvm.com/docs/");
        }

    }
}
