using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Compiler.Resolving
{

#if NETSTANDARD
    using System.Runtime.Loader;
	public class AssemblyLoader 
	{
	    public static Assembly LoadRaw(byte[] data) => AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(data));

		public static Assembly LoadFile(string path) => AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        
	}
#else
    public class AssemblyLoader
    {
        public static Assembly LoadRaw(byte[] data) => Assembly.Load(data);

        public static Assembly LoadFile(string path) => Assembly.LoadFile(path);
    }

#endif
}

