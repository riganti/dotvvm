using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DotVVM.Diagnostics.StatusPage
{
    internal sealed class DotHtmlFileInfo
    {
        public CompilationState Status { get; internal set; }
        public string Exception { get; internal set; }

        /// <summary>Gets or sets the virtual path to the view.</summary>
        public string VirtualPath { get; }

        public string TagName { get; }
        public string Namespace { get; }
        public string Assembly { get; }
        public string TagPrefix { get; }
        public string Url { get; }
        public string RouteName { get; }
        public ImmutableArray<string>? DefaultValues { get; }
        public bool? HasParameters { get; }

        public DotHtmlFileInfo(string virtualPath, string tagName = null, string nameSpace = null, string assembly = null, string tagPrefix = null, string url = null, string routeName = null, ImmutableArray<string>? defaultValues = null, bool? hasParameters = null)
        {
            VirtualPath = virtualPath;
            Status = IsDothtmlFile(virtualPath) ? CompilationState.None : CompilationState.NonCompilable;

            TagName = tagName;
            Namespace = nameSpace;
            Assembly = assembly;
            TagPrefix = tagPrefix;
            Url = url;
            RouteName = routeName;
            DefaultValues = defaultValues;
            HasParameters = hasParameters;
        }

        private static bool IsDothtmlFile(string virtualPath)
        {
            return !string.IsNullOrWhiteSpace(virtualPath) &&
                (
                virtualPath.IndexOf(".dothtml", StringComparison.OrdinalIgnoreCase) > -1 ||
                virtualPath.IndexOf(".dotmaster", StringComparison.OrdinalIgnoreCase) > -1 ||
                virtualPath.IndexOf(".dotcontrol", StringComparison.OrdinalIgnoreCase) > -1 ||
                virtualPath.IndexOf(".dotlayout", StringComparison.OrdinalIgnoreCase) > -1
                );
        }
    }
}
