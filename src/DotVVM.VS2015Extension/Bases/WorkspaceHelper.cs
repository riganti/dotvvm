using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml;
using DotVVM.VS2015Extension.DotvvmPageWizard;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using Project = EnvDTE.Project;
using Task = System.Threading.Tasks.Task;

namespace DotVVM.VS2015Extension.Bases
{
    public class WorkspaceHelper
    {
        public const string ProjectItemKindPhysicalFolder = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";
        private static readonly object InstanceLocker = new object();
        private static VisualStudioWorkspace staticWorkspace;
        private static object workspaceLocker = new object();
        private static WorkspaceHelper instance;
        private VisualStudioWorkspace workspace;

        public static VisualStudioWorkspace ServiceProvidedWorkspace
        {
            get
            {
                if (staticWorkspace == null)
                {
                    lock (workspaceLocker)
                    {
                        if (staticWorkspace == null)
                        {
                            staticWorkspace = ServiceProvider.GlobalProvider.GetService(typeof(VisualStudioWorkspace)) as VisualStudioWorkspace;
                        }
                    }
                }
                return staticWorkspace;
            }
        }

        public static WorkspaceHelper Current
        {
            get
            {
                if (instance == null)
                    lock (InstanceLocker)
                    {
                        if (instance == null)
                        {
                            instance = new WorkspaceHelper() { Workspace = ServiceProvidedWorkspace };
                        }
                    }
                return instance;
            }
        }

        public VisualStudioWorkspace Workspace
        {
            get
            {
                if (workspace == null)
                {
                    lock (workspaceLocker)
                    {
                        if (workspace == null)
                        {
                            workspace = ServiceProvider.GlobalProvider.GetService(typeof(VisualStudioWorkspace)) as VisualStudioWorkspace;
                        }
                    }
                    return workspace;
                }
                return workspace;
            }
            set
            {
                workspace = value;
            }
        }

        public void GetTypeDestination(string typeName)
        {
            throw new NotImplementedException();
            var compilations = GetCompilations();

            //var directives = compilations.SelectMany(s => s.SyntaxTrees.Where(g=> g.nam).Select(f => f.GetRoot().GetDirectives()));
        }

        public List<SyntaxTreeInfo> GetSyntaxTreeInfos()
        {
            var compilations = GetCompilations();

            var trees = compilations
                .SelectMany(c => c.SyntaxTrees.Select(t => new SyntaxTreeInfo() { Tree = t, SemanticModel = c.GetSemanticModel(t), Compilation = c }))
                .Where(t => t.Tree != null)
                .ToList();
            return trees;
        }

        public List<SyntaxTree> GetSyntaxTree()
        {
            var compilations = GetCompilations();
            var trees = compilations
                .SelectMany(c => c.SyntaxTrees)
                .Where(t => t != null)
                .ToList();
            return trees;
        }

        public void GetTypeDestination(string assambly, string typeName)
        {
            throw new NotImplementedException();
        }

        public void GetTypeDestination(Type type)
        {
            throw new NotImplementedException();
        }

        private List<Compilation> GetCompilations()
        {
            var compilations = new List<Compilation>();

            foreach (var p in Workspace.CurrentSolution.Projects)
            {
                try
                {
                    var compilation = Task.Run(() => p.GetCompilationAsync()).Result;
                    if (compilation != null)
                    {
                        compilations.Add(compilation);
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(new Exception("Cannot get the compilation!", ex));
                }
            }

            return compilations;
        }
    }
}