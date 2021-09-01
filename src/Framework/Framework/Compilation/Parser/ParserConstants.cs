namespace DotVVM.Framework.Compilation.Parser
{
    public class ParserConstants
    {
        public const string ViewModelDirectiveName = "viewModel";
        public const string MasterPageDirective = "masterPage";
        public const string BaseTypeDirective = "baseType";
        public const string ResourceTypeDirective = "resourceType";
        public const string ResourceNamespaceDirective = "resourceNamespace";
        public const string ImportNamespaceDirective = "import";
        public const string WrapperTagNameDirective = "wrapperTag";
        public const string NoWrapperTagNameDirective = "noWrapperTag";
        public const string ServiceInjectDirective = "service";
        public const string ViewModuleDirective = "js";

        public const string ValueBinding = "value";
        public const string CommandBinding = "command";
        public const string ControlPropertyBinding = "controlProperty";
        public const string ControlCommandBinding = "controlCommand";
        public const string ResourceBinding = "resource";
        public const string StaticCommandBinding = "staticCommand";
        
        public const string RootSpecialBindingProperty = "_root";
        public const string ParentSpecialBindingProperty = "_parent";
        public const string ThisSpecialBindingProperty = "_this";
    }
    
}
