﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation
{
	public readonly struct NamespaceImport: IEquatable<NamespaceImport>
	{
		[JsonPropertyName("namespace")]
		public string Namespace { get; }
		[JsonPropertyName("alias")]
		public string? Alias { get; }

		[JsonIgnore]
		public bool HasAlias => Alias is not null;

		[JsonConstructor]
		public NamespaceImport(string @namespace, string? alias = null)
		{
			if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
			if (alias == "") throw new ArgumentException("Alias cannot be empty string, use null instead.");
			this.Namespace = @namespace;
			this.Alias = alias;
		}

        public override bool Equals(object? obj) =>
            obj is NamespaceImport other && Equals(other);

        public bool Equals(NamespaceImport other) =>
            string.Equals(Namespace, other.Namespace) && string.Equals(Alias, other.Alias);

        public override int GetHashCode() =>
            unchecked(((Namespace?.GetHashCode() ?? 0) * 397) ^ (Alias?.GetHashCode() ?? 0));

		public override string ToString() => "import(" + (Alias == null ? Namespace : Alias + "=" + Namespace) + ")";
    }
}
