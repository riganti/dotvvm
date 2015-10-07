using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.Common.Data
{
    public class VisualVersionInfo
    {
        public string Version { get; set; }

        public string VersionName
        {
            get
            {
                var productName = "";
                var versionName = "";
                switch (VisualVersion)
                {
                    //case VisualVersion.Express:
                    //    productName = "Visual C# Express";
                    //    break;

                    case VisualVersion.Studio:
                        productName = "Visual Studio";
                        break;
                }
                switch (Version)
                {
                    //case "12.0":
                    //    versionName = "2013";
                    //    break;

                    case "14.0":
                        versionName = "2015";
                        break;
                }
                return $"{productName} {versionName}";
            }
        }

        public VisualVersion VisualVersion { get; set; }

        public string InstallPath { get; set; }
    }
}