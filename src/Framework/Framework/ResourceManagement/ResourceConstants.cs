using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public class ResourceConstants
    {
        public const string KnockoutJSResourceName = "knockout";
        public const string DotvvmResourceName = "dotvvm";
        public const string DotvvmDebugResourceName = "dotvvm.debug";
        public const string GlobalizeResourceName = "globalize";
        public const string GlobalizeCultureResourceName = "globalize:{0}";
        [Obsolete("IE is not supported anymore", error: true)]
        public const string PolyfillBundleResourceName = "dotvvm.polyfill.bundle";

        public const string DotvvmFileUploadCssResourceName = "dotvvm.fileUpload-css";
        public const string DotvvmInternalCssResourceName = "dotvvm.internal-css";
        public const string DotvvmDotnetWasmInteropResourceName = "dotvvm.interop.dotnet-wasm";
    }
}
