using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
	public class AssemblyLoader : AssemblyLoadContext
	{
		protected override Assembly Load(AssemblyName assemblyName) => Assembly.Load(assemblyName);

		public static Assembly LoadStream(Stream stream) => new AssemblyLoader().LoadFromStream(stream);

		public static Assembly LoadFile(string path) => new AssemblyLoader().LoadFromAssemblyPath(path);
	}
}
