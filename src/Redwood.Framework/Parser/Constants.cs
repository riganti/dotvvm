using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Parser
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


        public const string RootSpecialBindingProperty = "_root";
        public const string ParentSpecialBindingProperty = "_parent";
        public const string ThisSpecialBindingProperty = "_this";
        public const string ControlStateSpecialBindingProperty = "_controlState_";

        public const string JQueryResourceName = "jquery";
        public const string KnockoutJSResourceName = "knockout";
        public const string KnockoutMapperResourceName = "knockout.mapper";
        public const string RedwoodResourceName = "redwood";
        public const string BootstrapResourceName = "bootstrap";
        public const string BootstrapCssResourceName = "bootstrap-css";
        public const string GlobalizeResourceName = "globalize";
        public const string GlobalizeCultureResourceName = "globalize.{0}";

        public const string ResourceHandlerUrl = "/redwoodEmbeddedResource?file={0}";
        public const string ResourceHandlerMatchUrl = "redwoodEmbeddedResource";
    }
}
