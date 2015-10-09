using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.VS2015Extension.Bases.Commands
{
    static class CommandGuids
    {
        public const string GuidDotvvmMenuPackageCmdSetString = "1B39C66B-16E9-4B17-A90D-841357CC282C";
        public const string GuidShlMainMenuString = "3C6CAE40-7406-4629-A4A8-BD803D284CF0";
        public const string GuidDotvvmMainMenuCmdSetString = "1658ABCF-E4D1-423C-879C-AE9323010BFF";
        public const string GuidTopMenuString = "D03F77BC-80A2-4254-AA77-A215E49D5CC4";
        public const string GuidHelpCmdSetString = "978ABD05-C5C5-4884-AECE-80E41AD1B75C";
        
        public static readonly Guid GuidDotvvmMenuPackageCmdSet = new Guid(GuidDotvvmMenuPackageCmdSetString);
        public static readonly Guid GuidShlMainMenu = new Guid(GuidShlMainMenuString);
        public static readonly Guid GuidDotvvmMenuCmdSet = new Guid(GuidDotvvmMainMenuCmdSetString);
        public static readonly Guid GuidTopMenu = new Guid(GuidTopMenuString);
        public static readonly  Guid GuidHelpCmdSet = new Guid(GuidHelpCmdSetString);
    }
}
