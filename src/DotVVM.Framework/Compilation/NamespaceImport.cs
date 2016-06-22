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
		public readonly string Namespace;
		public readonly string Alias;

		public bool HasAlias => Alias != null;

		public NamespaceImport(string @namespace, string alias = null)
		{
			if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
			this.Namespace = @namespace;
			this.Alias = alias;
		}
	}
}
