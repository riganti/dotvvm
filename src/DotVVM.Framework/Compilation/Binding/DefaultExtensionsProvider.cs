using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Binding
{
    public class DefaultExtensionsProvider : IExtensionsProvider
    {
        private readonly List<Type> typesLookup;

        public DefaultExtensionsProvider()
        {
            typesLookup = new List<Type>();
            AddTypeForExtensionsLookup(typeof(Enumerable));
        }

        protected void AddTypeForExtensionsLookup(Type type)
        {
            typesLookup.Add(type);
        }

        public virtual IEnumerable<MethodInfo> GetExtensionMethods()
        {
            foreach (var registeredType in typesLookup)
                foreach (var method in registeredType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    yield return method;
        }
    }
}
