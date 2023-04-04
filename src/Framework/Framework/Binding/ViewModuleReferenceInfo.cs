using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    [HandleAsImmutableObjectInDotvvmProperty]
    public sealed class ViewModuleReferenceInfo
    {
        public string[] ReferencedModules { get; }

        /// <summary>The modules are referenced under an Id to the dotvvm client-side runtime. The same ID must be used in the invocation from the _js literal.</summary>
        public string ViewId
        {
            get => viewId ?? throw new ArgumentException($"{nameof(ViewId)} has not been set.");
            internal set => viewId = value;
        }
        private string? viewId;

        /// <summary> Whether control id should be used instead of ViewId to identify the modules. </summary>
        public bool IsMarkupControl { get; }

        public ViewModuleReferenceInfo(string? viewId, string[] referencedModules, bool isMarkupControl)
        {
            this.viewId = viewId;
            this.IsMarkupControl = isMarkupControl;

            // sort modules so the ID is deterministic
            this.ReferencedModules = referencedModules;
            Array.Sort(this.ReferencedModules, StringComparer.Ordinal);
            var moduleBatchUniqueId = GenerateModuleBatchUniqueId();

            ImportResourceName = ViewModuleImportResource.GetName(moduleBatchUniqueId);
            InitResourceName = ViewModuleInitResource.GetName(moduleBatchUniqueId);
        }

        public string InitResourceName { get; }

        public string ImportResourceName { get; }


        internal (ViewModuleImportResource importResource, ViewModuleInitResource initResource) BuildResources(IDotvvmResourceRepository allResources)
        {
            var dependencies = ReferencedModules.SelectMany((moduleResourceName, index) => {
                var moduleResource = allResources.FindResource(moduleResourceName);
                if (moduleResource is null)
                    throw new Exception($"Cannot find resource named '{moduleResourceName}' referenced by the @js directive!");
                if (!(moduleResource is ScriptModuleResource))
                    throw new Exception($"The resource named '{moduleResourceName}' referenced by the @js directive must be of the ScriptModuleResource type!");
                return moduleResource.Dependencies;
            }).Distinct().ToArray();

            return (
                new ViewModuleImportResource(ReferencedModules, ImportResourceName, dependencies),
                new ViewModuleInitResource(ReferencedModules, InitResourceName, ViewId, new[] { ImportResourceName })
            );
        }

        private string GenerateModuleBatchUniqueId()
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.Unicode.GetBytes(string.Join("\0", this.ReferencedModules))))
                .Replace("/", "_").Replace("+", "-").Replace("=", "");
        }
    }
}
