using System;
using System.Collections.Immutable;
using System.Linq;

namespace DotVVM.Framework.Binding
{
    public class BindingCompilationRequirementsAttribute : Attribute
    {
        public readonly ImmutableArray<Type> Required;
        public readonly ImmutableArray<Type> Optional;
        public readonly ImmutableArray<Type> Excluded;

        public BindingCompilationRequirementsAttribute(Type[] required = null, Type[] optional = null, Type[] excluded = null)
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
            return new BindingCompilationRequirementsAttribute(
                required: Required.Concat(attr.Required).Except(attr.Excluded).Distinct().ToImmutableArray(),
                optional: Optional.Concat(attr.Optional).Distinct().ToImmutableArray(),
                excluded: Excluded.Concat(attr.Excluded).Distinct().ToImmutableArray());
        }
    }
}