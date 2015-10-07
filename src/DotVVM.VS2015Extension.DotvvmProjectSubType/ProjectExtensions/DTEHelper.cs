using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace DotVVM.VS2015Extension.DotvvmProjectSubType.ProjectExtensions
{
    public static class DTEHelper
    {

        private static DTE2 dte = null;
        private static readonly object dteLocker = new object();

        public static DTE2 DTE
        {
            get
            {
                if (dte == null)
                {
                    lock (dteLocker)
                    {
                        if (dte == null)
                        {
                            dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
                        }
                    }
                }
                return dte;
            }
        }

    }
}
