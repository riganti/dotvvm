namespace DotVVM.Framework.Hosting
{
    public class HostingConstants
    {
        public const string GlobalizeCultureUrlPath = "dotvvmGlobalizeCulture";
        public const string GlobalizeCultureUrlIdParameter = "id";
        public const string ResourceHandlerUrl = "~/dotvvmEmbeddedResource?name={0}&assembly={1}";
        public const string ResourceHandlerMatchUrl = "dotvvmEmbeddedResource";

        public const string FileUploadHandlerMatchUrl = "dotvvmFileUpload";

        public const string SpaContentPlaceHolderDataAttributeName = "data-dot-spacontentplaceholder";
        public const string SpaContentPlaceHolderHeaderName = "X-DotVVM-SpaContentPlaceHolder";
        public const string SpaContentPlaceHolderID = "__dot_SpaContentPlaceHolder";
        public const string SpaContentPlaceHolderDefaultRouteDataAttributeName = "data-dot-spacontentplaceholder-defaultroute";

        public const string DotvvmFileUploadAsyncHeaderName = "X-DotvvmAsyncUpload";
    }
}