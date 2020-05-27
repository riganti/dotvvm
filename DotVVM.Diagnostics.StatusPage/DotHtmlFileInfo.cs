using System.Collections.Generic;

namespace DotVVM.Diagnostics.StatusPage
{
    public class DotHtmlFileInfo
    {
        public CompilationState Status { get; set; }
        public string Exception { get; set; }
        public string TagName { get; set; }
        public string Namespace { get; set; }
        public string Assembly { get; set; }
        public string TagPrefix { get; set; }
        public string Url { get; set; }

        /// <summary>Gets key of route.</summary>
        public string RouteName { get; set; }

        /// <summary>Gets the default values of the optional parameters.</summary>
        public List<string> DefaultValues { get; set; }

        /// <summary>Gets or sets the virtual path to the view.</summary>
        public string VirtualPath { get; set; }

        public bool HasParameters { get; set; }


        private sealed class VirtualPathEqualityComparer : IEqualityComparer<DotHtmlFileInfo>
        {
            public bool Equals(DotHtmlFileInfo x, DotHtmlFileInfo y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.VirtualPath == y.VirtualPath;
            }

            public int GetHashCode(DotHtmlFileInfo obj)
            {
                return (obj.VirtualPath != null ? obj.VirtualPath.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<DotHtmlFileInfo> VirtualPathComparer { get; } = new VirtualPathEqualityComparer();
    }
}