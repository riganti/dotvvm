using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
#if DotNetCore
    using System.Runtime.Loader;
	public class AssemblyLoader : AssemblyLoadContext
	{
		protected override Assembly Load(AssemblyName assemblyName) => Assembly.Load(assemblyName);

		public static Assembly LoadRaw(byte[] data) => new AssemblyLoader().LoadFromStream(new MemoryStream(data));

		public static Assembly LoadFile(string path) => new AssemblyLoader().LoadFromAssemblyPath(path);
	}
#else
	public class AssemblyLoader
	{
		public static Assembly LoadRaw(byte[] data) => Assembly.Load(data);

		public static Assembly LoadFile(string path) => Assembly.LoadFile(path);
	}
#endif
}
