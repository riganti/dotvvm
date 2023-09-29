using System;

namespace DotVVM.Framework.Hosting
{
    public class HostingConstants
    {
        public const string DotvvmRequestContextKey = "dotvvm.requestContext";
        public const string DotvvmIsErrorHandlingKey = "dotvvm.isErrorHandling";

        public const string GlobalizeCultureUrlPath = "dotvvmGlobalizeCulture";
        public const string GlobalizeCultureUrlIdParameter = "id";
        public const string ResourceRouteName = "dotvvmResource";
        public const string ResourceHandlerUrl = "~/dotvvmEmbeddedResource?name={0}&assembly={1}";
        public const string ResourceHandlerMatchUrl = "dotvvmEmbeddedResource";

        public const string FileUploadHandlerMatchUrl = "dotvvmFileUpload";
        public const string CsrfTokenMatchUrl = "___dotvvm-create-csrf-token___";

        public const string SpaContentPlaceHolderHeaderName = "X-DotVVM-SpaContentPlaceHolder";
        public const string PostBackHeaderName = "X-DotVVM-PostBack";
        [Obsolete("Use PostBackHeaderName instead")]
        public const string SpaPostBackHeaderName = PostBackHeaderName;
        public const string SpaContentPlaceHolderID = "__dot_SpaContentPlaceHolder";
        public const string SpaUrlIdentifier = "___dotvvm-spa___";

        public const string SpaContentPlaceHolderDataAttributeName = "data-dotvvm-spacontentplaceholder";

        public const string DotvvmFileUploadAsyncHeaderName = "X-DotVVM-AsyncUpload";

        public const string HostAppModeKey = "host.AppMode";
    }
}
