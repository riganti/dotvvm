using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class ControlWizard : IWizard
    {
        private ControlWindowViewModel viewModel;
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
            var controlLocation = Path.GetDirectoryName(DTEHelper.GetProjectItemRelativePath(projectItem));
            var allProjectItems = DTEHelper.GetAllProjectItems().Select(DTEHelper.GetProjectItemRelativePath).ToList();

            viewModel = new ControlWindowViewModel()
            {
                ControlName = projectItem.Name,
                ControlLocation = controlLocation,
                CreateCodeBehind = false,
                CodeBehindClassName = WizardHelpers.GenerateCodeBehindClassName(Path.GetFileNameWithoutExtension(projectItem.Name)),
                CodeBehindClassLocation = controlLocation,
                Folders = allProjectItems
                    .Select(p => p.Substring(0, p.LastIndexOfAny(new[] { '/', '\\' }) + 1).TrimEnd('/', '\\'))
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList()
            };

            var window = new ControlWindow()
            {
                DataContext = viewModel
            };
            if (window.ShowDialog() != true)
            {
                throw new WizardCancelledException();
            }

            // generate items
            DTEHelper.ExecuteSafe(() => GenerateControl(viewModel, projectItem), "Couldn't create the control!");
            if (viewModel.CreateCodeBehind)
            {
                DTEHelper.ExecuteSafe(() => GenerateCodeBehindClass(viewModel, projectItem.ContainingProject), "Couldn't create the code behind file!");
            }
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }


        private void GenerateControl(ControlWindowViewModel controlWindowViewModel, ProjectItem projectItem)
        {
            // generate namespace for code behind
            var rootNamespace = projectItem.ContainingProject.Properties.Item("DefaultNamespace").Value as string;
            controlWindowViewModel.CodeBehindClassNamespace = WizardHelpers.GenerateViewModelNamespace(rootNamespace, controlWindowViewModel.CodeBehindClassLocation);
            controlWindowViewModel.CodeBehindClassRootNamespace = rootNamespace;

            // run template
            var template = new ControlTemplate() { ViewModel = controlWindowViewModel };
            File.WriteAllText(DTEHelper.GetProjectItemFullPath(projectItem), template.TransformText(), Encoding.UTF8);
        }

        private void GenerateCodeBehindClass(ControlWindowViewModel controlWindowViewModel, Project project)
        {
            // find path
            var folder = DTEHelper.GetOrCreateFolder(project.ProjectItems, controlWindowViewModel.CodeBehindClassLocation);
            var codeBehindClassTemplatePath = Path.Combine(templateDir, "DotvvmControlCodeBehind.zip\\DotvvmControlCodeBehind.vstemplate");

            // create a code behind
            folder.AddFromTemplate(codeBehindClassTemplatePath, controlWindowViewModel.CodeBehindClassName);
            var projectItem = folder.OfType<ProjectItem>().FirstOrDefault(p => p.Name == controlWindowViewModel.CodeBehindClassName + ".cs");
            if (projectItem != null)
            {
                // regenerate the codebehind
                var template = new ControlCodeBehindTemplate() { ViewModel = controlWindowViewModel };
                File.WriteAllText(DTEHelper.GetProjectItemFullPath(projectItem), template.TransformText(), Encoding.UTF8);
            }
        }

    }
}
