using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class DataContextStack : IDataContextStack
    {
        public DataContextStack Parent { get; }
        public Type DataContextType { get; }
        public IReadOnlyList<NamespaceImport> NamespaceImports { get; }
        public IReadOnlyList<BindingExtensionParameter> ExtensionParameters { get; }
        public int DataContextSpaceId { get; }


        public DataContextStack(Type type,
            DataContextStack parent = null,
            IReadOnlyList<NamespaceImport> imports = null,
            IReadOnlyList<BindingExtensionParameter> extenstionParameters = null,
            int contextId = -1)
        {
            Parent = parent;
            DataContextType = type;
            NamespaceImports = imports ?? parent?.NamespaceImports;
            ExtensionParameters = extenstionParameters ?? new BindingExtensionParameter[0];
            DataContextSpaceId = contextId > 0 ? contextId : AssignId();
        }

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
        IDataContextStack IDataContextStack.Parent => Parent;

        private static int _idCounter;
        public static int AssignId() => System.Threading.Interlocked.Add(ref _idCounter, 1);

        private static readonly ConcurrentDictionary<int, DataContextStack> _store = new ConcurrentDictionary<int, DataContextStack>();
        public void Save()
        {
            _store.TryAdd(this.DataContextSpaceId, this);
        }

        public static DataContextStack GetById(int id) => _store[id];
    }
}
