using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree
{
    /// <summary>
    /// Represents compile-time DataContext info - Type of current DataContext, it's parent and other available parameters
    /// </summary>
    [HandleAsImmutableObjectInDotvvmPropertyAttribute]
    public sealed class DataContextStack : IDataContextStack
    {
        public DataContextStack? Parent { get; }
        public Type DataContextType { get; }
        public IReadOnlyList<NamespaceImport> NamespaceImports { get; }
        public IReadOnlyList<BindingExtensionParameter> ExtensionParameters { get; }
        public IReadOnlyList<Delegate> BindingPropertyResolvers { get; }

        private readonly int hashCode;

        private DataContextStack(Type type,
            DataContextStack? parent = null,
            IReadOnlyList<NamespaceImport>? imports = null,
            IReadOnlyList<BindingExtensionParameter>? extensionParameters = null,
            IReadOnlyList<Delegate>? bindingPropertyResolvers = null)
        {
            Parent = parent;
            DataContextType = type;
            NamespaceImports = imports ?? parent?.NamespaceImports ?? new NamespaceImport[0];
            ExtensionParameters = extensionParameters ?? new BindingExtensionParameter[0];
            BindingPropertyResolvers = bindingPropertyResolvers ?? new Delegate[0];

            hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Gets all extension parameter available in current context and their definition offset
        /// </summary>
        public IEnumerable<(int dataContextLevel, BindingExtensionParameter parameter)> GetCurrentExtensionParameters()
        {
            var blackList = new HashSet<string>();
            var current = this;
            int level = 0;
            while (current != null)
            {
                foreach (var p in current.ExtensionParameters.Where(p => blackList.Add(p.Identifier) && (current == this || p.Inherit)))
                    yield return (level, p);
                current = current.Parent;
                level++;
            }
        }

        public IEnumerable<DataContextStack> EnumerableItems()
        {
            var c = this;
            while (c != null)
            {
                yield return c;
                c = c.Parent;
            }
        }

        public IEnumerable<Type> Enumerable()
        {
            var c = this;
            while (c != null)
            {
                yield return c.DataContextType;
                c = c.Parent;
            }
        }

        public IEnumerable<Type> Parents()
        {
            var c = Parent;
            while (c != null)
            {
                yield return c.DataContextType;
                c = c.Parent;
            }
        }

        ITypeDescriptor IDataContextStack.DataContextType => new ResolvedTypeDescriptor(DataContextType);
        IDataContextStack? IDataContextStack.Parent => Parent;

        public override bool Equals(object? obj) =>
            obj is DataContextStack other && Equals(other);

        public bool Equals(DataContextStack stack)
        {
            return ReferenceEquals(this, stack) || hashCode == stack.hashCode
                && DataContextType == stack.DataContextType
                && NamespaceImports.SequenceEqual(stack.NamespaceImports)
                && ExtensionParameters.SequenceEqual(stack.ExtensionParameters)
                && Equals(Parent, stack.Parent);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = 0;
                foreach (var import in NamespaceImports)
                {
                    hashCode += (hashCode * 47) ^ import.GetHashCode();
                }

                foreach (var parameter in ExtensionParameters)
                {
                    hashCode *= 17;
                    hashCode += parameter.GetHashCode();
                }

                hashCode = (hashCode * 397) ^ (Parent?.GetHashCode() ?? 0);
                hashCode = (hashCode * 13) ^ (DataContextType?.FullName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            string?[] features = new [] {
                $"type={this.DataContextType.FullName}",
                this.NamespaceImports.Any() ? "imports=[" + string.Join(", ", this.NamespaceImports) + "]" : null,
                this.ExtensionParameters.Any() ? "ext=[" + string.Join(", ", this.ExtensionParameters.Select(e => e.Identifier + ": " + e.ParameterType.Name)) + "]" : null,
                this.BindingPropertyResolvers.Any() ? "resolvers=[" + string.Join(", ", this.BindingPropertyResolvers.Select(s => s.Method)) + "]" : null,
                this.Parent != null ? "par=[" + string.Join(", ", this.Parents().Select(p => p.Name)) + "]" : null
            };
            return "(" + features.Where(a => a != null).StringJoin(", ") + ")";
        }


        //private static ConditionalWeakTable<DataContextStack, DataContextStack> internCache = new ConditionalWeakTable<DataContextStack, DataContextStack>();
        public static DataContextStack Create(Type type,
            DataContextStack? parent = null,
            IReadOnlyList<NamespaceImport>? imports = null,
            IReadOnlyList<BindingExtensionParameter>? extensionParameters = null,
            IReadOnlyList<Delegate>? bindingPropertyResolvers = null)
        {
            var dcs = new DataContextStack(type, parent, imports, extensionParameters, bindingPropertyResolvers);
            return dcs;// internCache.GetValue(dcs, _ => dcs);
        }
    }
}
