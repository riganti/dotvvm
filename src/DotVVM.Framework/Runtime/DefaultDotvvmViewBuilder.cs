#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime
{
    public class DefaultDotvvmViewBuilder : IDotvvmViewBuilder
    {
        protected readonly IMarkupFileLoader markupFileLoader;

        protected readonly IControlBuilderFactory controlBuilderFactory;
        protected readonly DotvvmMarkupConfiguration markupConfiguration;

        public DefaultDotvvmViewBuilder(IMarkupFileLoader markupFileLoader, IControlBuilderFactory builderFactory, DotvvmMarkupConfiguration markupConfiguration)
        {
            this.markupConfiguration = markupConfiguration;
            this.markupFileLoader = markupFileLoader;
            this.controlBuilderFactory = builderFactory;
        }

        /// <summary>
        /// Builds the <see cref="DotvvmView"/> for the specified HTTP request, resolves the master page hierarchy and performs the composition.
        /// </summary>
        public DotvvmView BuildView(IDotvvmRequestContext context)
        {
            // get the page markup
            var markup = markupFileLoader.GetMarkupFileVirtualPath(context);

            // build the page
            var (_, pageBuilder) = controlBuilderFactory.GetControlBuilder(markup);
            var contentPage = (DotvvmView)pageBuilder.Value.BuildControl(controlBuilderFactory, context.Services);

            FillsDefaultDirectives(contentPage);

            // check for master page and perform composition recursively
            while (IsNestedInMasterPage(contentPage))
            {
                // load master page
                var masterPageFile = contentPage.Directives![ParserConstants.MasterPageDirective];
                var masterPage = (DotvvmView)controlBuilderFactory.GetControlBuilder(masterPageFile).builder.Value.BuildControl(controlBuilderFactory, context.Services);

                FillsDefaultDirectives(masterPage);
                PerformMasterPageComposition(contentPage, masterPage);

                masterPage.ViewModelType = contentPage.ViewModelType;
                contentPage = masterPage;
            }

            // verifies the SPA request
            VerifySpaRequest(context, contentPage);

            return contentPage;
        }

        /// <summary>
        /// If the request is SPA request, we need to verify that the page contains the same SpaContentPlaceHolder.
        /// Also we need to check that the placeholder is the same.
        /// </summary>
        protected void VerifySpaRequest(IDotvvmRequestContext context, DotvvmView page)
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
                    context.RedirectToUrl(context.HttpContext.Request.Url.AbsoluteUri.Replace("/" + HostingConstants.SpaUrlIdentifier, ""));
                }
            }
        }

        /// <summary>
        /// Determines whether the page is nested in master page.
        /// </summary>
        private bool IsNestedInMasterPage(DotvvmView page)
        {
            return page.Directives!.ContainsKey(ParserConstants.MasterPageDirective);
        }

        /// <summary>
        /// Fills default directives if specific directives are not set
        /// </summary>
        private void FillsDefaultDirectives(DotvvmView page)
        {
            foreach (var key in markupConfiguration.DefaultDirectives.Keys)
            {
                if (!page.Directives!.Keys.Contains(key))
                {
                    page.Directives[key] = markupConfiguration.DefaultDirectives[key];
                }
            }
        }

        /// <summary>
        /// Performs the master page nesting.
        /// </summary>
        private void PerformMasterPageComposition(DotvvmView childPage, DotvvmView masterPage)
        {
            if (!masterPage.ViewModelType.IsAssignableFrom(childPage.ViewModelType))
                throw new DotvvmControlException(childPage, $"Master page requires viewModel of type '{masterPage.ViewModelType}' and it is not assignable from '{childPage.ViewModelType}'.");

            // find content place holders
            var placeHolders = GetMasterPageContentPlaceHolders(masterPage);

            // find contents
            var contents = GetChildPageContents(childPage, placeHolders);

            // perform the composition
            foreach (var content in contents)
            {
                // find the corresponding placeholder
                var placeHolder = placeHolders.SingleOrDefault(p => p.ID == content.ContentPlaceHolderID);
                if (placeHolder == null)
                {
                    throw new DotvvmControlException(content, $"The placeholder with ID '{content.ContentPlaceHolderID}' was not found in the master page '{masterPage.GetValue(Internal.MarkupFileNameProperty)}'!");
                }

                // replace the contents
                var contentPlaceHolder = new PlaceHolder();
                contentPlaceHolder.SetDataContextType(content.Parent!.GetDataContextType());
                (content.Parent as DotvvmControl)?.Children.Remove(content);

                placeHolder.Children.Clear();
                placeHolder.Children.Add(contentPlaceHolder);

                contentPlaceHolder.Children.Add(content);
                content.SetValue(Internal.IsMasterPageCompositionFinishedProperty, true);
                content.SetValue(DotvvmView.DirectivesProperty, childPage.Directives);
                content.SetValue(Internal.MarkupFileNameProperty, childPage.GetValue(Internal.MarkupFileNameProperty));
            }

            // copy the directives from content page to the master page (except the @masterpage)
            masterPage.ViewModelType = childPage.ViewModelType;
        }

        /// <summary>
        /// Gets the content place holders.
        /// </summary>
        private List<ContentPlaceHolder> GetMasterPageContentPlaceHolders(DotvvmControl masterPage)
        {
            var placeHolders = masterPage.GetAllDescendants().OfType<ContentPlaceHolder>().ToList();

            // check that no placeholder is nested in another one and that each one has valid ID
            foreach (var placeHolder in placeHolders)
            {
                if (placeHolder.ID == null) throw new DotvvmControlException(placeHolder, "PlaceHolder has to have a ID");
                if (placeHolder.GetAllAncestors().Intersect(placeHolders).Any())
                {
                    throw new Exception(string.Format("The ContentPlaceHolder with ID '{0}' cannot be nested in another ContentPlaceHolder!", placeHolder.ID)); // TODO: exception handling
                }
            }
            return placeHolders;
        }

        /// <summary>
        /// Checks that the content page does not contain invalid content.
        /// </summary>
        private List<Content> GetChildPageContents(DotvvmView childPage, List<ContentPlaceHolder> parentPlaceHolders)
        {
            // make sure that the body contains only whitespace and Content controls
            var nonContentElements =
                childPage.Children.Where(c => !((c is RawLiteral && ((RawLiteral)c).IsWhitespace) || (c is Content)));
            if (nonContentElements.Any())
            {
                // show all error lines
                var innerExceptions = nonContentElements.Select(s =>
                        new Exception($"Error occurred near line: {(s.GetValue(Internal.MarkupLineNumberProperty)?.ToString() ?? "")}.")).ToList(); // the message cannot be specifically to the line, because MarkupLineNumber shows the last character position which is a line under the error in some cases.

                var corruptedFile = childPage.GetValue(Internal.MarkupFileNameProperty)?.ToString();
                throw new AggregateException("If the page contains @masterpage directive, it can only contain white space and <dot:Content /> controls! \r\n"
                    + (string.IsNullOrWhiteSpace(corruptedFile) ? "" : $"Corrupted file name: {corruptedFile}"), innerExceptions);
            }

            // make sure that the Content controls are not nested in other elements
            var contents = childPage.GetAllDescendants().OfType<Content>()
                .Where(c => !(bool)c.GetValue(Internal.IsMasterPageCompositionFinishedProperty)!)
                .ToList();
            if (contents.Any(c => c.Parent != childPage))
            {
                throw new Exception("The control <dot:Content /> cannot be placed inside any control!");    // TODO: exception handling
            }

            return contents;
        }
    }
}
