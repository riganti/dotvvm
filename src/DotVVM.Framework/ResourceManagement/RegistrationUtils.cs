using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public static class RegistrationUtils
    {
        /// <summary>
        /// Registers debug file path for the specified embeded resource.
        /// In debug mode resource is loaded from this path, it is refreshed on every request, and debug maps are looked up in the same directory.
        /// </summary>
        public static void SetEmbeddedResourceDebugFile(this DotvvmResourceRepository repo, string resourceName, string filePath)
        {
            var location = repo.FindResource(resourceName).As<ILinkResource>()?.GetLocations()?.OfType<EmbeddedResourceLocation>()?.FirstOrDefault();
            if (location != null) location.DebugFilePath = filePath;
        }
    }
}
