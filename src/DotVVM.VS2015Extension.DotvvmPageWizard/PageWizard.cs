using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TemplateWizard;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class PageWizard : IWizard
    {
        private PageWindowViewModel viewModel;
        private dynamic automationObject;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            this.automationObject = automationObject;
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
            var viewLocation = DTEHelper.GetProjectItemRelativePath(projectItem);

            viewModel = new PageWindowViewModel()
            {
                ViewName = projectItem.Name,
                ViewLocation = viewLocation,
                CreateViewModel = true,
                ViewModelName = GenerateViewModelName(projectItem.Name),
                ViewModelLocation = GenerateViewModelLocation(viewLocation),
                // TODO
            };

            var window = new PageWindow()
            {
                DataContext = viewModel
            };
            if (window.ShowDialog() != true)
            {
                throw new WizardCancelledException();
            }
        }

        private string GenerateViewModelName(string name)
        {
            // TODO:
            throw new NotImplementedException();
        }

        private string GenerateViewModelLocation(string viewLocation)
        {
            // TODO:
            throw new NotImplementedException();
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }
    }
}
