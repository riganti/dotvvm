using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class DTEHelper
    {





        private static DTE2 dte = null;
        private static object dteLocker = new object();
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



        public static IEnumerable<ProjectItem> GetSelfAndChildProjectItems(ProjectItem projectItem)
        {
            yield return projectItem;
            for (int i = 1; i <= projectItem.ProjectItems.Count; i++)
            {
                ProjectItem item = null;
                try
                {
                    item = projectItem.ProjectItems.Item(i);
                }
                catch (Exception ex)
                {
                    // sometimes we get System.ArgumentException: The parameter is incorrect. (Exception from HRESULT: 0x80070057 (E_INVALIDARG)) 
                    // when we open some file in the text editor
                    LogService.LogError(new Exception("Cannot evaluate items in the project!", ex));
                }

                if (item != null)
                {
                    foreach (var childItem in GetSelfAndChildProjectItems(item))
                    {
                        yield return childItem;
                    }
                }
            }
        }

        public static string GetProjectItemRelativePath(ProjectItem item)
        {
            var path = item.Properties.Item("FullPath").Value as string;
            var projectPath = GetProjectPath(item.ContainingProject);

            var result = path.StartsWith(projectPath, StringComparison.CurrentCultureIgnoreCase) ? path.Substring(projectPath.Length).TrimStart('\\', '/') : path;
            result = result.Replace('\\', '/');
            return result;
        }

        public static string GetProjectPath(Project project)
        {
            return project.Properties.Item("FullPath").Value as string;
        }
    }
}
