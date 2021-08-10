using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation
{
	public struct NamespaceImport: IEquatable<NamespaceImport>
	{
		[JsonProperty("namespace")]
		public readonly string Namespace;
		[JsonProperty("alias")]
		public readonly string Alias;

		[JsonIgnore]
		public bool HasAlias => Alias != null;

		[JsonConstructor]
		public NamespaceImport(string @namespace, string alias = null)
		{
			if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
			this.Namespace = @namespace;
			this.Alias = alias;
		}

        public override bool Equals(object obj) =>
            obj is NamespaceImport other && Equals(other);

        public bool Equals(NamespaceImport other) =>
            string.Equals(Namespace, other.Namespace) && string.Equals(Alias, other.Alias);

        public override int GetHashCode() =>
            unchecked(((Namespace?.GetHashCode() ?? 0) * 397) ^ (Alias?.GetHashCode() ?? 0));

		public override string ToString() => "import(" + (Alias == null ? Namespace : Alias + "=" + Namespace) + ")";
    }
}
