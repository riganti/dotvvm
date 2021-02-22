using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Binding
{
    public class DefaultExtensionsProvider : IExtensionsProvider
    {
        private readonly List<Type> typesLookup;
        private readonly List<MethodInfo> methodsLookup;

        public DefaultExtensionsProvider()
        {
            typesLookup = new List<Type>();
            methodsLookup = new List<MethodInfo>();
            AddTypeForExtensionsLookup(typeof(Enumerable));
        }

        protected void AddTypeForExtensionsLookup(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.GetCustomAttribute(typeof(ExtensionAttribute)) != null))
                methodsLookup.Add(method);

            typesLookup.Add(type);
        }

        public virtual IEnumerable<MethodInfo> GetExtensionMethods()
        {
            return methodsLookup;
        }
    }
}
