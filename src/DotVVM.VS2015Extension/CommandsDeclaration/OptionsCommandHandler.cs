using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace DotVVM.VS2015Extension.CommandsDeclaration
{
    public class OptionsCommandHandler
    {
        public void SetupCommands(DTE2 dte, OleMenuCommandService mcs)
        {
            var optionsCommand = new CommandID(PackageGuids.ButtonsGuid, PackageIds.ShowOptions);
            var command = new OleMenuCommand(Execute, optionsCommand);
            mcs.AddCommand(command);
        }

        public void Execute(object sender, EventArgs eventArgs)
        {
            System.Windows.MessageBox.Show("Currently, there is nothing to set up in DotVVM yet.");
        }

    }
}
