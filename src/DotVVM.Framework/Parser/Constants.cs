using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser
{
    public class Constants
    {
        public const string ViewModelDirectiveName = "viewModel";
        public const string MasterPageDirective = "masterPage";
        public const string BaseTypeDirective = "baseType";
        
        public const string ValueBinding = "value";
        public const string CommandBinding = "command";
        public const string ControlStateBinding = "controlState";
        public const string ControlPropertyBinding = "controlProperty";
        public const string ControlCommandBinding = "controlCommand";
        public const string ResourceBinding = "resource";
        public const string StaticCommandBinding = "staticCommand";


        public const string RootSpecialBindingProperty = "_root";
        public const string ParentSpecialBindingProperty = "_parent";
        public const string ThisSpecialBindingProperty = "_this";
        public const string ControlStateSpecialBindingProperty = "_controlState_";

        public const string JQueryResourceName = "jquery";
        public const string KnockoutJSResourceName = "knockout";
        public const string DotvvmResourceName = "dotvvm";
        public const string DotvvmValidationResourceName = "dotvvm.validation";
        public const string DotvvmDebugResourceName = "dotvvm.debug";
        public const string GlobalizeResourceName = "globalize";
        public const string GlobalizeCultureResourceName = "globalize:{0}";

        public const string GlobalizeCultureUrlPath = "dotvvmGlobalizeCulture";
        public const string GlobalizeCultureUrlIdParameter = "id";
        public const string ResourceHandlerUrl = "~/dotvvmEmbeddedResource?name={0}&assembly={1}";
        public const string ResourceHandlerMatchUrl = "dotvvmEmbeddedResource";

        public const string FileUploadHandlerMatchUrl = "dotvvmFileUpload";

        public const string SpaContentPlaceHolderDataAttributeName = "data-dot-spacontentplaceholder";
        public const string SpaContentPlaceHolderHeaderName = "X-DotVVM-SpaContentPlaceHolder";
        public const string SpaContentPlaceHolderID = "__dot_SpaContentPlaceHolder";
        public const string SpaContentPlaceHolderDefaultRouteDataAttributeName = "data-dot-spacontentplaceholder-defaultroute";
        
        public const string DotvvmFileUploadResourceName = "dotvvm.fileUpload";
        public const string DotvvmFileUploadCssResourceName = "dotvvm.fileUpload-css";
        public const string DotvvmFileUploadAsyncHeaderName = "X-DotvvmAsyncUpload";
    }
}
