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
        public const string SpaPostBackHeaderName = "X-DotVVM-PostBack";
        public const string SpaContentPlaceHolderID = "__dot_SpaContentPlaceHolder";
        public const string SpaContentPlaceHolderDefaultRouteDataAttributeName = "data-dotvvm-spacontentplaceholder-defaultroute";

        public const string DotvvmFileUploadAsyncHeaderName = "X-DotVVM-AsyncUpload";
    }
}