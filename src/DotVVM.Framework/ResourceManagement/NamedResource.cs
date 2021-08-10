#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Resource with its name.
    /// </summary>
    public class NamedResource: IEquatable<NamedResource>
    {
        public string Name { get; }
        public IResource Resource { get; }

        public NamedResource(string name, IResource resource)
        {
            Name = name;
            Resource = resource;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(obj, this)
                || (Equals(obj as NamedResource));
        }

        public bool Equals(NamedResource? other)
        {
            return other != null && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
