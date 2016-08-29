using System;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Http.Extensions;

namespace DotVVM.Framework.Runtime
{
    /// <summary>
    /// Builds the DotVVM view and resolves the master pages.
    /// </summary>
    public class DefaultDotvvmViewBuilder : DotvvmViewBuilderBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDotvvmViewBuilder"/> class.
        /// </summary>
        public DefaultDotvvmViewBuilder(DotvvmConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// If the request is SPA request, we need to verify that the page contains the same SpaContentPlaceHolder.
        /// Also we need to check that the placeholder is the same.
        /// </summary>
        protected override void VerifySpaRequest(DotvvmRequestContext context, DotvvmView page)
        {
            if (context.IsSpaRequest)
            {
                var spaContentPlaceHolders = page.GetAllDescendants().OfType<SpaContentPlaceHolder>().ToList();
                if (spaContentPlaceHolders.Count > 1)
                {
                    throw new Exception("Multiple controls of type <dot:SpaContentPlaceHolder /> found on the page! This control can be used only once!");   // TODO: exception handling
                }
                if (spaContentPlaceHolders.Count == 0 || spaContentPlaceHolders[0].GetSpaContentPlaceHolderUniqueId() != context.GetSpaContentPlaceHolderUniqueId())
                {
                    // the client has loaded different page which does not contain current SpaContentPlaceHolder - he needs to be redirected
                    context.RedirectToUrl(context.HttpContext.Request.Url.ToString());
                }
            }
        }
    }
}