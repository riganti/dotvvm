using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation
{
	public struct NamespaceImport
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
	}
}
