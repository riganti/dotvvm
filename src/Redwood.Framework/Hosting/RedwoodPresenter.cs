using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser;
using Redwood.Framework.ViewModel;

namespace Redwood.Framework.Hosting
{
    public class RedwoodPresenter : IRedwoodPresenter
    {

        public IMarkupFileLoader MarkupFileLoader { get; private set; }

        public IControlBuilderFactory ControlBuilderFactory { get; private set; }

        public IViewModelLoader ViewModelLoader { get; private set; }

        public IViewModelSerializer ViewModelSerializer { get; private set; }

        public IOutputRenderer OutputRenderer { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodPresenter"/> class.
        /// </summary>
        public RedwoodPresenter(
            IMarkupFileLoader markupFileLoader,
            IControlBuilderFactory controlBuilderFactory,
            IViewModelLoader viewModelLoader,
            IViewModelSerializer viewModelSerializer,
            IOutputRenderer outputRenderer
        )
        {
            MarkupFileLoader = markupFileLoader;
            ControlBuilderFactory = controlBuilderFactory;
            ViewModelLoader = viewModelLoader;
            ViewModelSerializer = viewModelSerializer;
            OutputRenderer = outputRenderer;
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

            // get the page markup
            var markup = MarkupFileLoader.GetMarkup(context);

            // build the page
            var pageBuilder = ControlBuilderFactory.GetControlBuilder(markup);
            var page = pageBuilder() as RedwoodView;

            // locate and create the view model
            var viewModel = ViewModelLoader.InitializeViewModel(context, page);
            page.DataContext = viewModel;

            // init the view model lifecycle
            var isPostBack = context.OwinContext.Request.Method == "POST";
            context.IsPostBack = isPostBack;
            if (viewModel is IRedwoodViewModel)
            {
                ((IRedwoodViewModel)viewModel).Context = context;
                await ((IRedwoodViewModel)viewModel).Init();
            }

            if (!isPostBack)
            {
                // perform standard get
                if (viewModel is IRedwoodViewModel)
                {
                    await ((IRedwoodViewModel)viewModel).Load();
                }
            }
            else
            {
                // perform the postback
                Action invokedCommand;
                using (var sr = new StreamReader(context.OwinContext.Request.Body))
                {
                    ViewModelSerializer.PopulateViewModel(viewModel, page, await sr.ReadToEndAsync(), out invokedCommand);
                }
                if (viewModel is IRedwoodViewModel)
                {
                    await ((IRedwoodViewModel)viewModel).Load();
                }

                // invoke the postback command
                if (invokedCommand != null)
                {
                    invokedCommand();
                }
            }

            if (viewModel is IRedwoodViewModel)
            {
                await ((IRedwoodViewModel)viewModel).PreRender();
            }

            // render the output
            var serializedViewModel = ViewModelSerializer.SerializeViewModel(viewModel, page);
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
    }
}
