using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class PageWizard : IWizard
    {
        private PageWindowViewModel viewModel;
        private string templateDir;


        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            templateDir = Path.GetDirectoryName(Path.GetDirectoryName(customParams[0] as string));
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void RunFinished()
        {
        }

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
            var currentFile = DTEHelper.GetProjectItemRelativePath(projectItem);
            var viewLocation = Path.GetDirectoryName(DTEHelper.GetProjectItemRelativePath(projectItem));
            var allProjectItems = DTEHelper.GetAllProjectItems().Select(DTEHelper.GetProjectItemRelativePath).ToList();

            viewModel = new PageWindowViewModel()
            {
                ViewName = projectItem.Name,
                ViewLocation = viewLocation,
                CreateViewModel = true,
                ViewModelName = WizardHelpers.GenerateViewModelName(Path.GetFileNameWithoutExtension(projectItem.Name)),
                ViewModelLocation = WizardHelpers.GenerateViewModelLocation(viewLocation),
                MasterPages = allProjectItems
                    .Where(p => p.EndsWith(".dotmaster", StringComparison.CurrentCultureIgnoreCase))
                    .OrderBy(p => p)
                    .Where(p => !string.Equals(p, currentFile, StringComparison.CurrentCultureIgnoreCase))
                    .ToList(),
                Folders = allProjectItems
                    .Select(p => p.Substring(0, p.LastIndexOf("/", StringComparison.CurrentCultureIgnoreCase) + 1).TrimEnd('/'))
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList()
            };

            var window = new PageWindow()
            {
                DataContext = viewModel
            };
            if (window.ShowDialog() != true)
            {
                throw new WizardCancelledException();
            }

            // generate items
            DTEHelper.ExecuteSafe(() => GenerateView(viewModel, projectItem), "Couldn't create the view!");
            if (viewModel.CreateViewModel)
            {
                DTEHelper.ExecuteSafe(() => GenerateViewModel(viewModel, projectItem.ContainingProject), "Couldn't create the viewmodel!");
            }
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }


        private void GenerateView(PageWindowViewModel pageWindowViewModel, ProjectItem projectItem)
        {
            if (pageWindowViewModel.EmbedInMasterPage)
            {
                // try to extract placeholders from the master page
                var masterPageFileName = Path.Combine(DTEHelper.GetProjectPath(projectItem.ContainingProject), pageWindowViewModel.MasterPageLocation);
                pageWindowViewModel.ContentPlaceHolderIds = new MasterPageBuilder().ExtractPlaceHolderIds(masterPageFileName);
            }

            // generate namespace for viewmodel
            var rootNamespace = projectItem.ContainingProject.Properties.Item("DefaultNamespace").Value as string;
            pageWindowViewModel.ViewModelNamespace = WizardHelpers.GenerateViewModelNamespace(rootNamespace, pageWindowViewModel.ViewModelLocation);
            pageWindowViewModel.ViewModelRootNamespace = rootNamespace;

            // run template
            var template = new PageTemplate() { ViewModel = pageWindowViewModel };
            File.WriteAllText(DTEHelper.GetProjectItemFullPath(projectItem), template.TransformText(), Encoding.UTF8);
        }

        private void GenerateViewModel(PageWindowViewModel pageWindowViewModel, Project project)
        {
            // find path
            var folder = DTEHelper.GetOrCreateFolder(project.ProjectItems, pageWindowViewModel.ViewModelLocation);
            var viewModelTemplatePath = Path.Combine(templateDir, "DotvvmViewModel.zip\\DotvvmViewModel.vstemplate");

            // create a viewmodel
            folder.AddFromTemplate(viewModelTemplatePath, pageWindowViewModel.ViewModelName);
            var projectItem = folder.OfType<ProjectItem>().FirstOrDefault(p => p.Name == pageWindowViewModel.ViewModelName + ".cs");
            if (projectItem != null)
            {
                // regenerate the viewmodel
                var template = new ViewModelTemplate() {ViewModel = pageWindowViewModel};
                File.WriteAllText(DTEHelper.GetProjectItemFullPath(projectItem), template.TransformText(), Encoding.UTF8);
            }
        }

    }
}
