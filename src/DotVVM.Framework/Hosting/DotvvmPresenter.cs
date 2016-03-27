using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmPresenter : IDotvvmPresenter
    {
        public IDotvvmViewBuilder DotvvmViewBuilder { get; private set; }

        public IViewModelLoader ViewModelLoader { get; private set; }

        public IViewModelSerializer ViewModelSerializer { get; private set; }

        public IOutputRenderer OutputRenderer { get; private set; }

        public ICsrfProtector CsrfProtector { get; private set; }
        public string ApplicationPath { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmPresenter"/> class.
        /// </summary>
        public DotvvmPresenter(DotvvmConfiguration configuration)
        {
            DotvvmViewBuilder = configuration.ServiceLocator.GetService<IDotvvmViewBuilder>();
            ViewModelLoader = configuration.ServiceLocator.GetService<IViewModelLoader>();
            ViewModelSerializer = configuration.ServiceLocator.GetService<IViewModelSerializer>();
            OutputRenderer = configuration.ServiceLocator.GetService<IOutputRenderer>();
            CsrfProtector = configuration.ServiceLocator.GetService<ICsrfProtector>();
            ApplicationPath = configuration.ApplicationPhysicalPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmPresenter"/> class.
        /// </summary>
        public DotvvmPresenter(
            IDotvvmViewBuilder dotvvmViewBuilder,
            IViewModelLoader viewModelLoader,
            IViewModelSerializer viewModelSerializer,
            IOutputRenderer outputRenderer,
            ICsrfProtector csrfProtector
        )
        {
            DotvvmViewBuilder = dotvvmViewBuilder;
            ViewModelLoader = viewModelLoader;
            ViewModelSerializer = viewModelSerializer;
            OutputRenderer = outputRenderer;
            CsrfProtector = csrfProtector;
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        public async Task ProcessRequest(DotvvmRequestContext context)
        {
            try
            {
                await ProcessRequestCore(context);
            }
            catch (DotvvmInterruptRequestExecutionException)
            {
                // the response has already been generated, do nothing
                return;
            }
            catch (UnauthorizedAccessException)
            {
                context.OwinContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            catch (DotvvmControlException ex)
            {
                ex.FileName = Path.Combine(ApplicationPath, ex.FileName);
                throw;
            }
        }

        public async Task ProcessRequestCore(DotvvmRequestContext context)
        {
            if (context.OwinContext.Request.Method != "GET" && context.OwinContext.Request.Method != "POST")
            {
                // unknown HTTP method
                context.OwinContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                throw new DotvvmHttpException("Only GET and POST methods are supported!");
            }
            if (context.OwinContext.Request.Headers["X-PostbackType"] == "StaticCommand")
            {
                await ProcessStaticCommandRequest(context);
                return;
            }
            var isPostBack = context.IsPostBack = DetermineIsPostBack(context.OwinContext);
            context.ChangeCurrentCulture(context.Configuration.DefaultCulture);

            // build the page view
            var page = DotvvmViewBuilder.BuildView(context);
            page.SetValue(Internal.RequestContextProperty, context);

            // run the preinit phase in the page
            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.PreInit);

            // locate and create the view model
            context.ViewModel = ViewModelLoader.InitializeViewModel(context, page);
            page.DataContext = context.ViewModel;

            // get action filters
            var globalFilters = context.Configuration.Runtime.GlobalFilters.ToList();
            var viewModelFilters = context.ViewModel.GetType().GetCustomAttributes<ActionFilterAttribute>(true).ToList();

            // run OnViewModelCreated on action filters
            foreach (var filter in globalFilters.Concat(viewModelFilters))
            {
                filter.OnViewModelCreated(context);
            }

            // init the view model lifecycle
            if (context.ViewModel is IDotvvmViewModel)
            {
                ((IDotvvmViewModel)context.ViewModel).Context = context;
                await ((IDotvvmViewModel)context.ViewModel).Init();
            }

            // run the init phase in the page
            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Init);

            if (!isPostBack)
            {
                // perform standard get
                if (context.ViewModel is IDotvvmViewModel)
                {
                    await ((IDotvvmViewModel)context.ViewModel).Load();
                }

                // run the load phase in the page
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Load);
            }
            else
            {
                // perform the postback
                string postData;
                using (var sr = new StreamReader(context.OwinContext.Request.Body))
                {
                    postData = await sr.ReadToEndAsync();
                }
                ViewModelSerializer.PopulateViewModel(context, postData);

                // validate CSRF token 
                CsrfProtector.VerifyToken(context, context.CsrfToken);

                if (context.ViewModel is IDotvvmViewModel)
                {
                    await ((IDotvvmViewModel)context.ViewModel).Load();
                }

                // validate CSRF token 
                CsrfProtector.VerifyToken(context, context.CsrfToken);

                // run the load phase in the page
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Load);

                // invoke the postback command
                ActionInfo actionInfo;
                ViewModelSerializer.ResolveCommand(context, page, postData, out actionInfo);

                if (actionInfo != null)
                {
                    // get filters
                    var methodFilters = actionInfo.Binding.ActionFilters == null ? globalFilters.Concat(viewModelFilters).ToArray() :
                       globalFilters.Concat(viewModelFilters).Concat(actionInfo.Binding.ActionFilters).ToArray();

                    await ExecuteCommand(actionInfo, context, methodFilters);
                }
            }

            if (context.ViewModel is IDotvvmViewModel)
            {
                await ((IDotvvmViewModel)context.ViewModel).PreRender();
            }

            // run the prerender phase in the page
            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.PreRender);

            // run the prerender complete phase in the page
            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.PreRenderComplete);

            // generate CSRF token if required
            if (string.IsNullOrEmpty(context.CsrfToken))
            {
                context.CsrfToken = CsrfProtector.GenerateToken(context);
            }

            // run OnResponseRendering on action filters
            foreach (var filter in globalFilters.Concat(viewModelFilters))
            {
                filter.OnResponseRendering(context);
            }

            // render the output
            ViewModelSerializer.BuildViewModel(context);
            if (!context.IsInPartialRenderingMode)
            {
                // standard get
                await OutputRenderer.WriteHtmlResponse(context, page);
            }
            else
            {
                // postback or SPA content
                OutputRenderer.RenderPostbackUpdatedControls(context, page);
                ViewModelSerializer.AddPostBackUpdatedControls(context);
                await OutputRenderer.WriteViewModelResponse(context, page);
            }

            if (context.ViewModel != null)
            {
                ViewModelLoader.DisposeViewModel(context.ViewModel);
            }
        }

        public async Task ProcessStaticCommandRequest(DotvvmRequestContext context)
        {
            JObject postData;
            using (var jsonReader = new JsonTextReader(new StreamReader(context.OwinContext.Request.Body)))
            {
                postData = JObject.Load(jsonReader);
            }
            // validate csrf token
            context.CsrfToken = postData["$csrfToken"].Value<string>();
            CsrfProtector.VerifyToken(context, context.CsrfToken);

            var command = postData["command"].Value<string>();
            var arguments = postData["args"] as JArray;
            var lastDot = command.LastIndexOf('.');
            var typeName = command.Remove(lastDot);
            var methodName = command.Substring(lastDot + 1);
            var methodInfo = Type.GetType(typeName).GetMethod(methodName);

            if (!Attribute.IsDefined(methodInfo, typeof(AllowStaticCommandAttribute)))
            {
                throw new DotvvmHttpException($"This method cannot be called from the static command. If you need to call this method, add the '{nameof(AllowStaticCommandAttribute)}' to the method.");
            }
            var target = methodInfo.IsStatic ? null : arguments[0].ToObject(methodInfo.DeclaringType);
            var methodArguments =
                arguments.Skip(methodInfo.IsStatic ? 0 : 1)
                .Zip(methodInfo.GetParameters(), (arg, parameter) => arg.ToObject(parameter.ParameterType))
                .ToArray();
            var actionInfo = new ActionInfo()
            {
                IsControlCommand = false,
                Action = () => methodInfo.Invoke(target, methodArguments)
            };
            var filters = context.Configuration.Runtime.GlobalFilters
                .Concat(methodInfo.DeclaringType.GetCustomAttributes<ActionFilterAttribute>())
                .Concat(methodInfo.GetCustomAttributes<ActionFilterAttribute>())
                .ToArray();

            var task = ExecuteCommand(actionInfo, context, filters);
            await task;
            object result = TaskUtils.GetResult(task);

            using (var writer = new StreamWriter(context.OwinContext.Response.Body))
            {
                writer.WriteLine(JsonConvert.SerializeObject(result));
            }
        }

        protected Task ExecuteCommand(ActionInfo action, DotvvmRequestContext context, IEnumerable<ActionFilterAttribute> methodFilters)
        {
            // run OnCommandExecuting on action filters
            foreach (var filter in methodFilters)
            {
                filter.OnCommandExecuting(context, action);
            }
            object result = null;
            try
            {
                result = action.Action();
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }
                if (ex is DotvvmInterruptRequestExecutionException)
                {
                    throw new DotvvmInterruptRequestExecutionException("The request execution was interrupted in the command!", ex);
                }
                context.CommandException = ex;
            }

            // run OnCommandExecuted on action filters
            foreach (var filter in methodFilters.Reverse())
            {
                filter.OnCommandExecuted(context, action, context.CommandException);
            }

            if (context.CommandException != null && !context.IsCommandExceptionHandled)
            {
                throw new Exception("Unhandled exception occured in the command!", context.CommandException);
            }

            return result as Task ?? (result == null ? TaskUtils.GetCompletedTask() : Task.FromResult(result));
        }

        public static bool DetermineIsPostBack(IOwinContext context)
        {
            return context.Request.Method == "POST";
        }

        public static bool DetermineSpaRequest(IOwinContext context)
        {
            return !string.IsNullOrEmpty(context.Request.Headers[HostingConstants.SpaContentPlaceHolderHeaderName]);
        }

        public static bool DeterminePartialRendering(IOwinContext context)
        {
            return DetermineIsPostBack(context) || DetermineSpaRequest(context);
        }

        public static string DetermineSpaContentPlaceHolderUniqueId(IOwinContext context)
        {
            return context.Request.Headers[HostingConstants.SpaContentPlaceHolderHeaderName];
        }
    }
}
