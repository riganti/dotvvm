using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    [HandleAsImmutableObjectInDotvvmPropertyAttribute]
    public sealed class ViewModuleReferenceInfo
    {
        public string[] ReferencedModules { get; }
        /// <summary>The modules are referenced under an Id to the dotvvm client-side runtime. The same ID must be used in the invocation from the _js literal.</summary>
        public string ViewId { get; }

        /// <summary> Whether control id should be used instead of ViewId to identify the modules. </summary>
        public bool IsMarkupControl { get; }

        public ViewModuleReferenceInfo(string viewId, string[] referencedModules, bool isMarkupControl)
        {
            this.ViewId = viewId;
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
            var dependencies = ReferencedModules.SelectMany(m => allResources.FindResource(m).Dependencies).Distinct().ToArray();

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
