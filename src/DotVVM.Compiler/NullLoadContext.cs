#if NETCOREAPP2_1

using System.Reflection;
using System.Runtime.Loader;

namespace DotVVM.Compiler
{
    public class NullLoadContext : AssemblyLoadContext
    {
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
#endif
