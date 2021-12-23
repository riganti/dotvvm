using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// Specifies requirements that binding has to satisfy in order to successfully Initialize (and compile).
    /// Can be applied on binding class, bound property or instance (as property).
    /// Attribute can be applied multiple times - all values are combined before compilation.
    /// </summary>
    public class BindingCompilationRequirementsAttribute : Attribute
    {
        /// <summary>
        /// Properties that have to created in order to Initialize.
        /// </summary>
        public readonly ImmutableArray<Type> Required;
        /// <summary>
        /// Properties that will be computed, if possible.
        /// </summary>
        public readonly ImmutableArray<Type> Optional;
        /// <summary>
        /// Properties that does not have to be there - overwrites previous specification of Required or Optional.
        /// For example may be useful for property that may contain value binding, but does not need a Javascript translation
        /// </summary>
        public readonly ImmutableArray<Type> Excluded;

        public static readonly BindingCompilationRequirementsAttribute Empty = new();

        public bool IsEmpty => Required.IsEmpty && Optional.IsEmpty && Excluded.IsEmpty;

        public BindingCompilationRequirementsAttribute(Type[]? required = null, Type[]? optional = null, Type[]? excluded = null)
        {
            this.Required = required?.ToImmutableArray() ?? ImmutableArray<Type>.Empty;
            this.Optional = optional?.ToImmutableArray() ?? ImmutableArray<Type>.Empty;
            this.Excluded = excluded?.ToImmutableArray() ?? ImmutableArray<Type>.Empty;
        }

        public BindingCompilationRequirementsAttribute(ImmutableArray<Type> required, ImmutableArray<Type> optional, ImmutableArray<Type> excluded)
        {
            this.Required = required;
            this.Optional = optional;
            this.Excluded = excluded;
        }

        public BindingCompilationRequirementsAttribute ApplySecond(BindingCompilationRequirementsAttribute attr)
        {
            if (attr.IsEmpty) return this;
            if (this.IsEmpty) return attr;
            var result = new BindingCompilationRequirementsAttribute(
                required: Required.Concat(attr.Required).Except(attr.Excluded).Distinct().ToImmutableArray(),
                optional: Optional.Concat(attr.Optional).Distinct().ToImmutableArray(),
                excluded: Excluded.Concat(attr.Excluded).Distinct().ToImmutableArray());
            if (result.IsEmpty)
                return Empty;
            return result;
        }


        public BindingCompilationRequirementsAttribute ClearRequirements()
        {
            if (Required.Length + Optional.Length == 0) return this;
            return new BindingCompilationRequirementsAttribute(
                required: ImmutableArray<Type>.Empty,
                optional: ImmutableArray<Type>.Empty,
                excluded: Excluded
            );
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[BindingCompilationRequirements(");

            if (!Required.IsEmpty)
            {
                sb.Append("required: [")
                    .Append(string.Join(", ", Required.Select(r => r.Name)))
                    .Append("], ");
            }
            if (!Optional.IsEmpty)
            {
                sb.Append("optional: [")
                    .Append(string.Join(", ", Optional.Select(r => r.Name)))
                    .Append("], ");
            }
            if (!Excluded.IsEmpty)
            {
                sb.Append("excluded: [")
                    .Append(string.Join(", ", Excluded.Select(r => r.Name)))
                    .Append("], ");
            }

            sb.Append(")]");
            return sb.ToString();
        }
    }
}
