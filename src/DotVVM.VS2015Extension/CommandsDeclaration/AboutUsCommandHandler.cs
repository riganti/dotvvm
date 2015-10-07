using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Process = System.Diagnostics.Process;

namespace DotVVM.VS2015Extension.CommandsDeclaration
{
    public class AboutUsCommandHandler
    {
        public void SetupCommands(DTE2 dte, OleMenuCommandService mcs)
        {
            var aboutUsCommand = new CommandID(PackageGuids.ButtonsGuid, PackageIds.ShowAbout);
            var command = new OleMenuCommand(Execute, aboutUsCommand);
            mcs.AddCommand(command);
        }

        public void Execute(object sender, EventArgs eventArgs)
        {
            Process.Start("http://www.dotvvm.com/");
        }

    }
}
