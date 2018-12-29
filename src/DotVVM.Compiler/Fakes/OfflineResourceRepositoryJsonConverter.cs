using System;
using System.Collections.Generic;
using System.Reflection;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Compiler.Fakes
{
    internal class OfflineResourceRepositoryJsonConverter : ResourceRepositoryJsonConverter
    {
        protected override IEnumerable<Assembly> GetAllAssembliesLoadedAssemblies()
        {
            // ReflectionUtils.GetAllAssemblies() returns only referenced dependencies of entry assembly - compiler has a different dependences than the app itself
            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}
