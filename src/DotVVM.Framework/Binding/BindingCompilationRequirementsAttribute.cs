using System;
using System.Collections.Immutable;
using System.Linq;

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


        public BindingCompilationRequirementsAttribute ClearRequirements()
        {
            if (Required.Length == 0) return this;
            return new BindingCompilationRequirementsAttribute(
                required: ImmutableArray<Type>.Empty,
                optional: Optional,
                excluded: Excluded
            );
        }
    }
}
