using DotVVM.VS2015Extension.Bases.Commands;
using DotVVM.VS2015Extension.CommandsDeclaration;
using DotVVM.VS2015Extension.DotvvmPageWizard;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using DotVVM.VS2015Extension.ProjectExtensions;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;

namespace DotVVM.VS2015Extension.VsPackages
{
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(CommandGuids.GuidDotvvmMenuPackageCmdSetString)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [InstalledProductRegistration("#1", "#1", VsPackagesConfiguration.Version, IconResourceID = 400)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    public sealed class DotvvmPackage : Package, IVsSolutionEvents3, IVsSolutionLoadEvents
    {
        private Microsoft.VisualStudio.OLE.Interop.IServiceProvider oleServiceProvider;
        public Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
        {
            get
            {
                return oleServiceProvider ?? (oleServiceProvider = GetService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider)) as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            }
        }

        private uint solutionEventsCookie;
        private IVsSolution solution;


        public static DotvvmPackage Instance { get; private set; }

        public OleMenuCommand TopMenu { get; set; }

        public static void ExecuteCommand(string commandName, string commandArgs = "")
        {
            var command = DTEHelper.DTE.Commands.Item(commandName);

            if (!command.IsAvailable)
                return;

            try
            {
                DTEHelper.DTE.ExecuteCommand(commandName, commandArgs);
            }
            catch
            {
                // ignored
            }
        }


        protected override void Initialize()
        {
            Instance = this;

            base.Initialize();

            RegisterMenuCommands();
            AdviseSolutionEvents();
        }


        private void RegisterMenuCommands()
        {
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs != null)
            {
                var aboutUsHandler = new AboutUsCommandHandler();
                aboutUsHandler.SetupCommands(DTEHelper.DTE, mcs);
                
                var docsHandler = new DocsCommandHandler();
                docsHandler.SetupCommands(DTEHelper.DTE, mcs);

                var optionsHandler = new OptionsCommandHandler();
                optionsHandler.SetupCommands(DTEHelper.DTE, mcs);
            }
        }


        private void AdviseSolutionEvents()
        {
            UnadviseSolutionEvents();

            solution = this.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solution != null)
            {
                solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            }
        }

        private void UnadviseSolutionEvents()
        {
            if (solution != null)
            {
                if (solutionEventsCookie != uint.MaxValue)
                {
                    solution.UnadviseSolutionEvents(solutionEventsCookie);
                    solutionEventsCookie = uint.MaxValue;
                }

                solution = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            UnadviseSolutionEvents();

            base.Dispose(disposing);
        }



        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK; 
        }

        public int OnAfterClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK; 
        }

        public int OnAfterMergeSolution(object pUnkReserved)
        {
            return VSConstants.S_OK; 
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK; 
        }

        public int OnAfterOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK; 
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK; 
        }

        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK; 
        }

        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK; 
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK; 
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK; 
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK; 
        }


        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeBackgroundSolutionLoadBegins()
        {
            return VSConstants.S_OK;
        }

        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return VSConstants.S_OK;
        }

        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterBackgroundSolutionLoadComplete()
        {
            UpgradeDotvvmProjects();

            return VSConstants.S_OK;
        }


        /// <summary>
        /// Upgrades all DotVVM projects in the solution.
        /// </summary>
        private void UpgradeDotvvmProjects()
        {
            foreach (var project in DTEHelper.DTE.Solution.Projects.OfType<Project>())
            {
                if (DTEHelper.IsDotvvmProject(project))
                {
                    // it is a DotVVM project, make sure the project has the flavor
                    var guids = DTEHelper.GetProjectTypeGuids(project, solution);
                    if (guids.IndexOf(GuidList.DotvvmPropertyPageProjectFactory, StringComparison.CurrentCultureIgnoreCase) < 0)
                    {
                        DTEHelper.SetProjectTypeGuids(project, "{" + GuidList.DotvvmPropertyPageProjectFactory + "};" + guids,
                            solution);
                        project.Save();

                        DTEHelper.ReloadProject(project);
                    }
                }
            }
        }
    }
}