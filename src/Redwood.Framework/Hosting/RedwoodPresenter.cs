using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser;
using Redwood.Framework.ViewModel;
using System.Diagnostics;
using Redwood.Framework.Runtime;
using Redwood.Framework.Security;

namespace Redwood.Framework.Hosting
{
    public class RedwoodPresenter : IRedwoodPresenter
    {
        public IRedwoodViewBuilder RedwoodViewBuilder { get; private set; }

        public IViewModelLoader ViewModelLoader { get; private set; }

        public IViewModelSerializer ViewModelSerializer { get; private set; }

        public IOutputRenderer OutputRenderer { get; private set; }

        public ICsrfProtector CsrfProtector { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodPresenter"/> class.
        /// </summary>
        public RedwoodPresenter(
            IRedwoodViewBuilder redwoodViewBuilder,
            IViewModelLoader viewModelLoader,
            IViewModelSerializer viewModelSerializer,
            IOutputRenderer outputRenderer,
            ICsrfProtector csrfProtector
        )
        {
            RedwoodViewBuilder = redwoodViewBuilder;
            ViewModelLoader = viewModelLoader;
            ViewModelSerializer = viewModelSerializer;
            OutputRenderer = outputRenderer;
            CsrfProtector = csrfProtector;
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        public async Task ProcessRequest(RedwoodRequestContext context)
        {
            Exception error = null;
            try
            {
                await ProcessRequestCore(context);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                await RenderErrorResponse(context, HttpStatusCode.InternalServerError, error);
            }
        }


        /// <summary>
        /// Renders the error response.
        /// </summary>
        public static async Task RenderErrorResponse(RedwoodRequestContext context, HttpStatusCode code, Exception error)
        {
            context.OwinContext.Response.StatusCode = (int)code;
            context.OwinContext.Response.ContentType = "text/html";

            var template = new ErrorPageTemplate()
            {
                Exception = error,
                ErrorCode = (int)code,
                ErrorDescription = code.ToString(),
                IpAddress = context.OwinContext.Request.RemoteIpAddress,
                CurrentUserName = context.OwinContext.Request.User.Identity.Name,
                Url = context.OwinContext.Request.Uri.ToString(),
                Verb = context.OwinContext.Request.Method
            };
            if (error is ParserException)
            {
                template.FileName = ((ParserException)error).FileName;
                template.LineNumber = ((ParserException)error).LineNumber;
                template.PositionOnLine = ((ParserException)error).PositionOnLine;
            }

            var text = template.TransformText();
            await context.OwinContext.Response.WriteAsync(text);
        }

        /// <summary>
        /// Processes the request and renders the output.
        /// </summary>
        private async Task ProcessRequestCore(RedwoodRequestContext context)
        {
            if (context.OwinContext.Request.Method != "GET" && context.OwinContext.Request.Method != "POST")
            {
                // unknown HTTP method
                await RenderErrorResponse(context, HttpStatusCode.MethodNotAllowed, new RedwoodHttpException("Only GET and POST methods are supported!"));
                return;
            }
            var isPostBack = context.OwinContext.Request.Method == "POST";
            context.IsPostBack = isPostBack;

            // build the page view
            var page = RedwoodViewBuilder.BuildView(context);

            // run the preinit phase in the page
            InvokePageLifeCycleEventRecursive(page, c => c.OnPreInit(context));

            // run the init phase in the page
            InvokePageLifeCycleEventRecursive(page, c => c.OnInit(context));

            // locate and create the view model
            context.ViewModel = ViewModelLoader.InitializeViewModel(context, page);
            page.DataContext = context.ViewModel;

            // init the view model lifecycle
            if (context.ViewModel is IRedwoodViewModel)
            {
                ((IRedwoodViewModel)context.ViewModel).Context = context;
                await ((IRedwoodViewModel)context.ViewModel).Init();
            }
            if (!isPostBack)
            {
                // perform standard get
                if (context.ViewModel is IRedwoodViewModel)
                {
                    await ((IRedwoodViewModel)context.ViewModel).Load();
                }

                // run the load phase in the page
                InvokePageLifeCycleEventRecursive(page, c => c.OnLoad(context));
            }
            else
            {
                // perform the postback
                Action invokedCommand;
                using (var sr = new StreamReader(context.OwinContext.Request.Body))
                {
                    ViewModelSerializer.PopulateViewModel(context, page, await sr.ReadToEndAsync(), out invokedCommand);
                }
                if (context.ViewModel is IRedwoodViewModel)
                {
                    await ((IRedwoodViewModel)context.ViewModel).Load();
                }

                // validate CSRF token 
                CsrfProtector.VerifyToken(context, context.CsrfToken);

                // run the load phase in the page
                InvokePageLifeCycleEventRecursive(page, c => c.OnLoad(context));

                // invoke the postback command
                if (invokedCommand != null)
                {
                    invokedCommand();
                }
            }

            if (context.ViewModel is IRedwoodViewModel)
            {
                await ((IRedwoodViewModel)context.ViewModel).PreRender();
            }

            // run the prerender phase in the page
            InvokePageLifeCycleEventRecursive(page, c => c.OnPreRender(context));

            // run the prerender complete phase in the page
            InvokePageLifeCycleEventRecursive(page, c => c.OnPreRenderComplete(context));

            // generate CSRF token if required
            if (string.IsNullOrEmpty(context.CsrfToken))
            {
                context.CsrfToken = CsrfProtector.GenerateToken(context);
            }

            // render the output
            var serializedViewModel = ViewModelSerializer.SerializeViewModel(context, page);
            if (!isPostBack)
            {
                // standard get
                await OutputRenderer.RenderPage(context, page, serializedViewModel);
            }
            else
            {
                // postback
                await OutputRenderer.RenderViewModel(context, page, serializedViewModel);
            }
        }

        /// <summary>
        /// Invokes the specified method on all controls in the page control tree.
        /// </summary>
        private void InvokePageLifeCycleEventRecursive(RedwoodControl control, Action<RedwoodControl> action)
        {
            foreach (var child in control.GetThisAndAllDescendants())
            {
                action(child);
            }
        }
    }
}
