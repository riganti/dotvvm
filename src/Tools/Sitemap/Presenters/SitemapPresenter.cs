using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Services;
using Microsoft.Extensions.Options;

namespace DotVVM.Sitemap.Presenters;

public class SitemapPresenter(IOptions<SitemapOptions> options, SitemapResolver sitemapResolver, SitemapXmlBuilder sitemapXmlBuilder) : IDotvvmPresenter
{
    public async Task ProcessRequest(IDotvvmRequestContext context)
    {
        var ct = context.GetCancellationToken();

        // determine public URL
        var publicUrl = GetPublicUrl(context);

        // get entries
        var entries = await sitemapResolver.GetSitemapEntriesAsync(context, publicUrl, ct);

        // write XML
        var xml = sitemapXmlBuilder.BuildXml(entries, publicUrl);
        context.HttpContext.Response.ContentType = "application/xml";

#if NET5_0_OR_GREATER
        await xml.SaveAsync(context.HttpContext.Response.Body, SaveOptions.None, ct);
#else
        xml.Save(context.HttpContext.Response.Body, SaveOptions.None);
#endif
    }

    private Uri GetPublicUrl(IDotvvmRequestContext context)
    {
        if (options.Value.SitePublicUrl != null)
        {
            return options.Value.SitePublicUrl;
        }

        // if not set, use the URL from the request
        var uri = new UriBuilder(context.HttpContext.Request.Url);
        uri.Path = context.HttpContext.Request.PathBase.Value ?? "";
        return uri.Uri;
    }
}
