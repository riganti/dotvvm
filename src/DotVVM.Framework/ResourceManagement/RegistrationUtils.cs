using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public static class RegistrationUtils
    {
        public static void SetEmbededResourceDebugFile(this DotvvmResourceRepository repo, string resourceName, string filePath)
        {
            var location = repo.FindResource(resourceName).As<ILinkResource>()?.GetLocations()?.OfType<EmbededResourceLocation>()?.FirstOrDefault();
            if (location != null) location.DebugFilePath = filePath;
        }
    }
}
