using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Runtime.Tracing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Hosting
{
    [NotAuthorized] // DotvvmPresenter handles authorization itself, allowing authorization on it would make [NotAuthorized] attribute useless on ViewModel, since request would be interrupted earlier that VM is found
    public class DotvvmPresenter : IDotvvmPresenter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmPresenter" /> class.
        /// </summary>
        public DotvvmPresenter(DotvvmConfiguration configuration, IDotvvmViewBuilder viewBuilder, IViewModelLoader viewModelLoader, IViewModelSerializer viewModelSerializer,
            IOutputRenderer outputRender, ICsrfProtector csrfProtector)
        {
            DotvvmViewBuilder = viewBuilder;
            ViewModelLoader = viewModelLoader;
            ViewModelSerializer = viewModelSerializer;
            OutputRenderer = outputRender;
            CsrfProtector = csrfProtector;
            ApplicationPath = configuration.ApplicationPhysicalPath;
        }

        public IDotvvmViewBuilder DotvvmViewBuilder { get; }

        public IViewModelLoader ViewModelLoader { get; }

        public IViewModelSerializer ViewModelSerializer { get; }

        public IOutputRenderer OutputRenderer { get; }

        public ICsrfProtector CsrfProtector { get; }

        public string ApplicationPath { get; }

        /// <summary>
        /// Processes the request.
        /// </summary>
        public async Task ProcessRequest(IDotvvmRequestContext context)
        {
            try
            {
                await ProcessRequestCore(context);
            }
            catch (UnauthorizedAccessException)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            catch (DotvvmControlException ex)
            {
                if (ex.FileName != null)
                {
                    ex.FileName = Path.Combine(ApplicationPath, ex.FileName);
                }
                throw;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ProcessRequestCore(IDotvvmRequestContext context)
        {
            if (context.HttpContext.Request.Method != "GET" && context.HttpContext.Request.Method != "POST")
            {
                // unknown HTTP method
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                throw new DotvvmHttpException("Only GET and POST methods are supported!");
            }
            if (context.HttpContext.Request.Headers["X-PostbackType"] == "StaticCommand")
            {
                await ProcessStaticCommandRequest(context);
                await context.RequestTracers.TracingEvent(RequestTracingConstants.StaticCommandExecuted, context);
                return;
            }
            var isPostBack = context.IsPostBack = DetermineIsPostBack(context.HttpContext);

            // build the page view
            var page = DotvvmViewBuilder.BuildView(context);
            page.SetValue(Internal.RequestContextProperty, context);
            context.View = page;
            await context.RequestTracers.TracingEvent(RequestTracingConstants.ViewInitialized, context);

            // locate and create the view model
            context.ViewModel = ViewModelLoader.InitializeViewModel(context, page);

            // get action filters
            var viewModelFilters = ActionFilterHelper.GetActionFilters<IViewModelActionFilter>(context.ViewModel.GetType().GetTypeInfo());
            viewModelFilters.AddRange(context.Configuration.Runtime.GlobalFilters.OfType<IViewModelActionFilter>());
            var requestFilters = ActionFilterHelper.GetActionFilters<IPageActionFilter>(context.ViewModel.GetType().GetTypeInfo());
            foreach (var f in requestFilters)
            {
                await f.OnPageLoadingAsync(context);
            }
            try
            {
                // run the preinit phase in the page
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.PreInit);
                page.DataContext = context.ViewModel;

                // run OnViewModelCreated on action filters
                foreach (var filter in viewModelFilters)
                {
                    await filter.OnViewModelCreatedAsync(context);
                }
                await context.RequestTracers.TracingEvent(RequestTracingConstants.ViewModelCreated, context);

                // init the view model lifecycle
                if (context.ViewModel is IDotvvmViewModel viewModel)
                {
                    viewModel.Context = context;
                    ChildViewModelsCache.SetViewModelClientPath(viewModel, ChildViewModelsCache.RootViewModelPath);
                    await viewModel.Init();
                }

                // run the init phase in the page
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Init);
                await context.RequestTracers.TracingEvent(RequestTracingConstants.InitCompleted, context);

                if (!isPostBack)
                {
                    // perform standard get
                    if (context.ViewModel is IDotvvmViewModel)
                    {
                        await ((IDotvvmViewModel)context.ViewModel).Load();
                    }

                    // run the load phase in the page
                    DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Load);
                    await context.RequestTracers.TracingEvent(RequestTracingConstants.LoadCompleted, context);
                }
                else
                {
                    // perform the postback
                    string postData;
                    using (var sr = new StreamReader(context.HttpContext.Request.Body))
                    {
                        postData = await sr.ReadToEndAsync();
                    }
                    ViewModelSerializer.PopulateViewModel(context, postData);

                    // run OnViewModelDeserialized on action filters
                    foreach (var filter in viewModelFilters)
                    {
                        await filter.OnViewModelDeserializedAsync(context);
                    }
                    await context.RequestTracers.TracingEvent(RequestTracingConstants.ViewModelDeserialized, context);

                    // validate CSRF token 
                    CsrfProtector.VerifyToken(context, context.CsrfToken);

                    if (context.ViewModel is IDotvvmViewModel)
                    {
                        await ((IDotvvmViewModel)context.ViewModel).Load();
                    }

                    // run the load phase in the page
                    DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Load);
                    await context.RequestTracers.TracingEvent(RequestTracingConstants.LoadCompleted, context);

                    // invoke the postback command
                    var actionInfo = ViewModelSerializer.ResolveCommand(context, page);

                    if (actionInfo != null)
                    {
                        // get filters
                        var methodFilters = context.Configuration.Runtime.GlobalFilters.OfType<ICommandActionFilter>()
                            .Concat(ActionFilterHelper.GetActionFilters<ICommandActionFilter>(context.ViewModel.GetType().GetTypeInfo()));
                        if (actionInfo.Binding.GetProperty<ActionFiltersBindingProperty>(ErrorHandlingMode.ReturnNull) is ActionFiltersBindingProperty filters)
                            methodFilters = methodFilters.Concat(filters.Filters.OfType<ICommandActionFilter>());

                        await ExecuteCommand(actionInfo, context, methodFilters);
                        await context.RequestTracers.TracingEvent(RequestTracingConstants.CommandExecuted, context);
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
                await context.RequestTracers.TracingEvent(RequestTracingConstants.PreRenderCompleted, context);

                // generate CSRF token if required
                if (string.IsNullOrEmpty(context.CsrfToken))
                {
                    context.CsrfToken = CsrfProtector.GenerateToken(context);
                }

                // run OnViewModelSerializing on action filters
                foreach (var filter in viewModelFilters)
                {
                    await filter.OnViewModelSerializingAsync(context);
                }
                await context.RequestTracers.TracingEvent(RequestTracingConstants.ViewModelSerialized, context);

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
                await context.RequestTracers.TracingEvent(RequestTracingConstants.OutputRendered, context);

                if (context.ViewModel != null)
                {
                    ViewModelLoader.DisposeViewModel(context.ViewModel);
                }
                foreach (var f in requestFilters) await f.OnPageLoadedAsync(context);
            }
            catch (DotvvmInterruptRequestExecutionException) { throw; }
            catch (DotvvmHttpException) { throw; }
            catch (Exception ex)
            {
                // run OnPageException on action filters
                foreach (var filter in requestFilters)
                {
                    await filter.OnPageExceptionAsync(context, ex);

                    if (context.IsPageExceptionHandled)
                    {
                        context.InterruptRequest();
                    }
                }

                throw;
            }
        }

        private object ExecuteStaticCommandPlan(StaticCommandInvocationPlan plan, Queue<JToken> arguments, IDotvvmRequestContext context)
        {
            var methodArgs = plan.Arguments.Select((a, index) => 
                a.Type == StaticCommandParameterType.Argument ? arguments.Dequeue().ToObject((Type)a.Arg) :
                a.Type == StaticCommandParameterType.Constant || a.Type == StaticCommandParameterType.DefaultValue ? a.Arg :
                a.Type == StaticCommandParameterType.Inject ? context.Services.GetRequiredService((Type)a.Arg) :
                a.Type == StaticCommandParameterType.Invocation ? ExecuteStaticCommandPlan((StaticCommandInvocationPlan)a.Arg, arguments, context) :
                throw new NotSupportedException("" + a.Type)
            ).ToArray();
            return plan.Method.Invoke(plan.Method.IsStatic ? null : methodArgs.First(), plan.Method.IsStatic ? methodArgs : methodArgs.Skip(1).ToArray());
        }

        public async Task ProcessStaticCommandRequest(IDotvvmRequestContext context)
        {
            JObject postData;
            using (var jsonReader = new JsonTextReader(new StreamReader(context.HttpContext.Request.Body)))
            {
                postData = JObject.Load(jsonReader);
            }
            // validate csrf token
            context.CsrfToken = postData["$csrfToken"].Value<string>();
            CsrfProtector.VerifyToken(context, context.CsrfToken);

            var command = postData["command"].Value<string>();
            var arguments = postData["args"] as JArray;
            var executionPlan =
                StaticCommandBindingCompiler.DecryptJson(Convert.FromBase64String(command), context.Services.GetService<IViewModelProtector>())
                .Apply(StaticCommandBindingCompiler.DeserializePlan);

            var actionInfo = new ActionInfo
            {
                IsControlCommand = false,
                Action = () => {
                    return ExecuteStaticCommandPlan(executionPlan, new Queue<JToken>(arguments), context);
                }
            };
            var filters = context.Configuration.Runtime.GlobalFilters.OfType<ICommandActionFilter>()
                .Concat(executionPlan.GetAllMethods().SelectMany(m => ActionFilterHelper.GetActionFilters<ICommandActionFilter>(m)))
                .ToArray();

            var result = await ExecuteCommand(actionInfo, context, filters);

            using (var writer = new StreamWriter(context.HttpContext.Response.Body))
            {
                var json = ViewModelSerializer.BuildStaticCommandResponse(context, result);
                writer.WriteLine(json);
            }
        }

        protected async Task<object> ExecuteCommand(ActionInfo action, IDotvvmRequestContext context, IEnumerable<ICommandActionFilter> methodFilters)
        {
            // run OnCommandExecuting on action filters
            foreach (var filter in methodFilters)
            {
                await filter.OnCommandExecutingAsync(context, action);
            }

            object result = null;
            Task resultTask = null;

            try
            {
                result = action.Action();

                resultTask = result as Task;
                if (resultTask != null)
                {
                    await resultTask;
                }
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
                await filter.OnCommandExecutedAsync(context, action, context.CommandException);
            }

            if (context.CommandException != null && !context.IsCommandExceptionHandled)
            {
                throw new Exception("Unhandled exception occured in the command!", context.CommandException);
            }

            if (resultTask != null)
            {
                return TaskUtils.GetResult(resultTask);
            }

            return result;
        }

        public static bool DetermineIsPostBack(IHttpContext context)
        {
            return context.Request.Method == "POST" && context.Request.Headers.ContainsKey(HostingConstants.SpaPostBackHeaderName);
        }

        public static bool DetermineSpaRequest(IHttpContext context)
        {
            return !string.IsNullOrEmpty(context.Request.Headers[HostingConstants.SpaContentPlaceHolderHeaderName]);
        }

        public static bool DeterminePartialRendering(IHttpContext context)
        {
            return DetermineIsPostBack(context) || DetermineSpaRequest(context);
        }

        public static string DetermineSpaContentPlaceHolderUniqueId(IHttpContext context)
        {
            return context.Request.Headers[HostingConstants.SpaContentPlaceHolderHeaderName];
        }
    }
}
