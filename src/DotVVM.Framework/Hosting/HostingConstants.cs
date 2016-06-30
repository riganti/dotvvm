namespace DotVVM.Framework.Hosting
{
    public class HostingConstants
    {
        public const string DotvvmRequestContextOwinKey = "dotvvm.requestContext";

        public const string GlobalizeCultureUrlPath = "dotvvmGlobalizeCulture";
        public const string GlobalizeCultureUrlIdParameter = "id";
        public const string ResourceHandlerUrl = "~/dotvvmEmbeddedResource/{0}/{1}/{2}";
        public const string ResourceHandlerMatchUrl = "dotvvmEmbeddedResource";

        public const string FileUploadHandlerMatchUrl = "dotvvmFileUpload";

        public const string SpaContentPlaceHolderHeaderName = "X-DotVVM-SpaContentPlaceHolder";
        public const string SpaPostBackHeaderName = "X-DotVVM-PostBack";
        public const string SpaContentPlaceHolderID = "__dot_SpaContentPlaceHolder";

        public const string SpaContentPlaceHolderDataAttributeName = "data-dotvvm-spacontentplaceholder";
        public const string SpaContentPlaceHolderDefaultRouteDataAttributeName = "data-dotvvm-spacontentplaceholder-defaultroute";
        public const string SpaUrlPrefixAttributeName = "data-dotvvm-spacontentplaceholder-urlprefix";

        public const string DotvvmFileUploadAsyncHeaderName = "X-DotVVM-AsyncUpload";
    }
}