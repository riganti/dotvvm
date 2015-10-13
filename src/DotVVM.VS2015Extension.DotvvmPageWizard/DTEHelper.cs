using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell.Interop;
using Project = EnvDTE.Project;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    // ReSharper disable once InconsistentNaming
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

        public static UndoContext UndoContext => DTE.UndoContext;

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
            return item?.Properties.Item("FullPath").Value as string;
        }

        public static IEnumerable<ProjectItem> GetCurrentProjectItems()
        {
            return GetProjectItems(DTE.ActiveDocument.ProjectItem.ContainingProject);
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

        public static IEnumerable<ProjectItem> GetProjectItems(Project project)
        {
            if (project.ProjectItems == null)
            {
                return Enumerable.Empty<ProjectItem>();
            }
            return project.ProjectItems.OfType<ProjectItem>().SelectMany(GetSelfAndChildProjectItems);
        }

        /// <param name="viewKind" >EnvDTEConstants</param>
        public static void ChangeActiveWindowTo(string filePath, string viewKind = EnvDTEConstants.vsViewKindTextView)
        {
            var window = DTE.OpenFile(viewKind, filePath);
            window.Activate();
        }

        public static ProjectItems GetOrCreateFolder(ProjectItems projectItems, string viewModelLocation)
        {
            var path = viewModelLocation.Split('/', '\\').Where(p => !String.IsNullOrEmpty(p)).ToList();

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
                        throw new Exception($"Could not add a folder '{path[i]}' in the project!");
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

        public static ProjectItem GetCurrentProjectItem()
        {
            return DTE.ActiveDocument.ProjectItem;
        }

        public static Project GetCurrentProject()
        {
            return DTE.ActiveDocument.ProjectItem.ContainingProject;
        }

        public static string GetActiveDocumentFullName()
        {
            return DTE.ActiveDocument.FullName;
        }

        public static ProjectItem GetProjectItemByFullPath(string filePath)
        {
            return DTE.Solution.FindProjectItem(filePath);
        }

        public static void ChangeActiveWindowTo(ProjectItem item)
        {
            var window = item?.Open(EnvDTEConstants.vsViewKindTextView);
            window?.Activate();
        }

        public static Project GetProjectFromHierarchy(IVsHierarchy hierarchy)
        {
            object obj = null;
            if (hierarchy != null)
            {
                hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
            }
            return obj as Project;

        }

        public static bool IsDotvvmProject(Project project)
        {
            return DTEHelper.GetProjectItems(project).Any(i => String.Equals(i.Name, "dotvvm.json", StringComparison.CurrentCultureIgnoreCase));
        }

        public static string GetProjectTypeGuids(Project project, IVsSolution solution)
        {
            IVsHierarchy hierarchy;
            solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);
            var aggregatableProject = (IVsAggregatableProject) hierarchy;
            string projectGuids;
            aggregatableProject.GetAggregateProjectTypeGuids(out projectGuids);
            return projectGuids;
        }

        public static void SetProjectTypeGuids(Project project, string projectTypeGuids, IVsSolution solution)
        {
            IVsHierarchy hierarchy;
            solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);
            var aggregatableProject = (IVsAggregatableProject)hierarchy;
            aggregatableProject.SetAggregateProjectTypeGuids(projectTypeGuids);
        }

        public static void ReloadProject(Project project)
        {
            var solutionName = Path.GetFileNameWithoutExtension(dte.Solution.FullName);
            var projectName = project.Name;

            DTE.Windows.Item(EnvDTEConstants.vsWindowKindSolutionExplorer).Activate();
            ((DTE2)dte).ToolWindows.SolutionExplorer.GetItem(solutionName + @"\" + projectName).Select(vsUISelectionType.vsUISelectionTypeSelect);

            dte.ExecuteCommand("Project.UnloadProject");
            dte.ExecuteCommand("Project.ReloadProject");
        }
    }
}