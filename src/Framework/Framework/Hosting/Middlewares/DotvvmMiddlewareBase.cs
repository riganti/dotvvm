namespace DotVVM.Framework.Hosting.Middlewares
{
    public abstract class DotvvmMiddlewareBase
    {
        /// <summary>
        /// Determines the current virtual directory.
        /// </summary>
        public static string GetVirtualDirectory(IHttpContext context)
        {
            return context.Request.PathBase.Value?.Trim('/') ?? "";
        }

        /// <summary>
        /// Get clean request url without slashes.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetCleanRequestUrl(IHttpContext context)
        {
            return context.Request.Path.Value?.TrimStart('/').TrimEnd('/') ?? "";
        }
    }
}
