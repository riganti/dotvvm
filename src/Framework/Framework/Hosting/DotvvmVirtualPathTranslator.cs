using DotVVM.Framework.Hosting;

public interface IDotvvmVirtualPathTranslator
{
    string TranslateVirtualPath(string virtualUrl, IHttpContext httpContext);
}


public sealed class DotvvmVirtualPathTranslator : IDotvvmVirtualPathTranslator
{
    string IDotvvmVirtualPathTranslator.TranslateVirtualPath(string virtualUrl, IHttpContext httpContext) =>
        DotvvmVirtualPathTranslator.TranslateVirtualPath(virtualUrl, httpContext);

    public static string TranslateVirtualPath(string virtualUrl, IHttpContext httpContext)
    {
        if (virtualUrl.StartsWith("~/"))
        {
            var pathRelative = virtualUrl.Substring(1);
            var pathBase = httpContext.Request.PathBase.Value;
            if (string.IsNullOrEmpty(pathBase))
                return pathRelative;
            else if (pathBase[0] == '/')
                return pathBase + pathRelative;
            else
                return "/" + pathBase + pathRelative;
        }
        return virtualUrl;
    }
}
