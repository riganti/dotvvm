using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class DTEHelper
    {





        private static DTE2 dte = null;
        private static object dteLocker = new object();
        public const string ProjectItemKindPhysicalFolder = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";

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
            var path = GetProjectItemFullPath(item);
            var projectPath = GetProjectPath(item.ContainingProject);

            var result = path.StartsWith(projectPath, StringComparison.CurrentCultureIgnoreCase) ? path.Substring(projectPath.Length).TrimStart('\\', '/') : path;
            result = result.Replace('\\', '/');
            return result;
        }

        public static string GetProjectItemFullPath(ProjectItem item)
        {
            return item.Properties.Item("FullPath").Value as string;
        }

        public static string GetProjectPath(Project project)
        {
            return project.Properties.Item("FullPath").Value as string;
        }

        public static IEnumerable<ProjectItem> GetAllProjectItems()
        {
            return DTE.Solution.Projects.OfType<Project>()
                    .SelectMany(p => p.ProjectItems.OfType<ProjectItem>())
                    .SelectMany(GetSelfAndChildProjectItems);
        }

        public static ProjectItems GetOrCreateFolder(ProjectItems projectItems, string viewModelLocation)
        {
            var path = viewModelLocation.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToList();

            for (var i = 0; i < path.Count; i++)
            {
                var projectItem = projectItems.OfType<ProjectItem>().FirstOrDefault(p => p.Name == path[i]);
                if (projectItem == null)
                {
                    try
                    {
                        projectItem = projectItems.AddFolder(path[i]);
                    }
                    catch
                    {
                        throw new Exception($"Couldn't add a folder '{path[i]}' in the project!");
                    }
                }
                else if (projectItem.Kind != ProjectItemKindPhysicalFolder)
                {
                    throw new Exception($"The location of the viewmodel is not valid! Path '{path[i]}'.");
                }
                projectItems = projectItem.ProjectItems;
            }
            return projectItems;
        }

        public static void ExecuteSafe(Action action, string errorMessage)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogService.LogError(new Exception(errorMessage, ex));
                MessageBox.Show(errorMessage + "\r\n" + ex.Message);
            }

        }
    }
}
