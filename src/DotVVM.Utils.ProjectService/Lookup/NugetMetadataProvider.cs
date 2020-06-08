using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public static class NugetMetadataProvider
    {
        public static List<string> GetPackagesDirectories(JObject assetsFile)
        {
            var pnd = GetProjectNugetDirectory(assetsFile);
            if ((pnd?.Count ?? 0) > 0) return pnd;

            return GetUserProfileNugetDirectory();
        }

        private static List<string> GetUserProfileNugetDirectory()
        {
            var userProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget");
            if (Directory.Exists(userProfilePath))
            {
                return new List<string>() { userProfilePath };
            }
            return new List<string>();
        }

        private static List<string> GetProjectNugetDirectory(JObject assetsFile)
        {
            var folders = assetsFile?["packageFolders"];
            return folders?.Children().Select(s => (s as JProperty)?.Name).Where(s => s != null).ToList();
        }
   
    }
}
