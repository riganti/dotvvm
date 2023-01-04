using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Binding;

namespace DotVVM.Framework.Binding
{
    /// <summary> Allow to adjust how bindings are compiled. Can be placed on custom binding type (for example, see <see cref="ValueBindingExpression" />) or on a dotvvm property </summary>
    public abstract class BindingCompilationOptionsAttribute : Attribute
    {
        /// <summary> Returns a list of resolvers - functions which accept any set of existing binding properties and returns one new binding property.
        /// It will be automatically invoked when the returned property is needed.
        /// See <see cref="BindingPropertyResolvers" /> for a list of default property resolvers - to adjust how the binding is compiled, you'll want to redefine one of the default resolvers.
        /// See <see cref="StaticCommandBindingExpression.OptionsAttribute" /> for example how to use this method. </summary>
        public abstract IEnumerable<Delegate> GetResolvers();
    }
}
