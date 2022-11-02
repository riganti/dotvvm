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
using System.Security;
using System.Runtime.CompilerServices;

namespace DotVVM.Framework.Hosting
{
    [NotAuthorized] // DotvvmPresenter handles authorization itself, allowing authorization on it would make [NotAuthorized] attribute useless on ViewModel, since request would be interrupted earlier that VM is found
    public class DotvvmPresenter : IDotvvmPresenter
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmPresenter" /> class.
        /// </summary>
        public DotvvmPresenter(DotvvmConfiguration configuration, IDotvvmViewBuilder viewBuilder, IViewModelLoader viewModelLoader, IViewModelSerializer viewModelSerializer,
            IOutputRenderer outputRender, ICsrfProtector csrfProtector, IViewModelParameterBinder viewModelParameterBinder,
#pragma warning disable CS0618
            IStaticCommandServiceLoader staticCommandServiceLoader
#pragma warning restore CS0618
        )
        {
            DotvvmViewBuilder = viewBuilder;
            ViewModelLoader = viewModelLoader;
            ViewModelSerializer = viewModelSerializer;
            OutputRenderer = outputRender;
            CsrfProtector = csrfProtector;
            ViewModelParameterBinder = viewModelParameterBinder;
#pragma warning disable CS0618
            StaticCommandServiceLoader = staticCommandServiceLoader;
#pragma warning restore CS0618
            ApplicationPath = configuration.ApplicationPhysicalPath;
            SecurityConfiguration = configuration.Security;
        }

        public IDotvvmViewBuilder DotvvmViewBuilder { get; }

        public IViewModelLoader ViewModelLoader { get; }

        public IViewModelSerializer ViewModelSerializer { get; }

        public IOutputRenderer OutputRenderer { get; }

        public ICsrfProtector CsrfProtector { get; }

        public IViewModelParameterBinder ViewModelParameterBinder { get; }

        public DotvvmSecurityConfiguration SecurityConfiguration { get; }

#pragma warning disable CS0618
        [Obsolete(DefaultStaticCommandServiceLoader.DeprecationNotice)]

        public IStaticCommandServiceLoader StaticCommandServiceLoader { get; }
#pragma warning restore CS0618

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
            catch (CorruptedCsrfTokenException ex)
            {
                // TODO this should be done by IOutputRender or something like that. IOutputRenderer does not support that, so should we make another IJsonErrorOutputWriter?
                context.HttpContext.Response.StatusCode = 400;
                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
                var settings = DefaultSerializerSettingsProvider.Instance.Settings;
                await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { action = "invalidCsrfToken", message = ex.Message }, settings));
            }
            catch (DotvvmExceptionBase ex)
            {
                if (ex.GetLocation() is { FileName: not null } location)
                {
                    ex.Location = location with { FileName = Path.Combine(ApplicationPath, location.FileName) };
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
                await context.InterruptRequestAsMethodNotAllowedAsync();
            }

            await ValidateSecFetchHeaders(context);

            var requestTracer = context.Services.GetRequiredService<AggregateRequestTracer>();
            if (context.HttpContext.Request.Headers["X-PostbackType"] == "StaticCommand")
            {
                await ProcessStaticCommandRequest(context);
                await requestTracer.TraceEvent(RequestTracingConstants.StaticCommandExecuted, context);
                return;
            }
            var isPostBack = context.IsPostBack = DetermineIsPostBack(context.HttpContext);

            // build the page view
            var page = DotvvmViewBuilder.BuildView(context);
            page.SetValue(Internal.RequestContextProperty, context);
            context.View = page;
            await requestTracer.TraceEvent(RequestTracingConstants.ViewInitialized, context);

            // locate and create the view model
            context.ViewModel = ViewModelLoader.InitializeViewModel(context, page);

            // get action filters
            var viewModelFilters = ActionFilterHelper.GetActionFilters<IViewModelActionFilter>(context.ViewModel.GetType())
                .Concat(context.Configuration.Runtime.GlobalFilters.OfType<IViewModelActionFilter>());

            var requestFilters = ActionFilterHelper.GetActionFilters<IPageActionFilter>(context.ViewModel.GetType())
                .Concat(context.Configuration.Runtime.GlobalFilters.OfType<IPageActionFilter>());

            foreach (var f in requestFilters)
            {
                await f.OnPageInitializedAsync(context);
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
                await requestTracer.TraceEvent(RequestTracingConstants.ViewModelCreated, context);

                // perform parameter binding
                if (context.ViewModel is DotvvmViewModelBase dotvvmViewModelBase)
                {
                    dotvvmViewModelBase.ExecuteOnViewModelRecursive(v => ViewModelParameterBinder.BindParameters(context, v));
                }
                else
                {
                    ViewModelParameterBinder.BindParameters(context, context.ViewModel);
                }

                // init the view model lifecycle
                if (context.ViewModel is IDotvvmViewModel viewModel)
                {
                    viewModel.Context = context;
                    ChildViewModelsCache.SetViewModelClientPath(viewModel, ChildViewModelsCache.RootViewModelPath);
                    await viewModel.Init();
                }

                // run the init phase in the page
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Init);
                await requestTracer.TraceEvent(RequestTracingConstants.InitCompleted, context);

                object? commandResult = null;
                if (!isPostBack)
                {
                    // perform standard get
                    if (context.ViewModel is IDotvvmViewModel)
                    {
                        await ((IDotvvmViewModel)context.ViewModel).Load();
                    }

                    // run the load phase in the page
                    DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Load);
                    await requestTracer.TraceEvent(RequestTracingConstants.LoadCompleted, context);
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
                    await requestTracer.TraceEvent(RequestTracingConstants.ViewModelDeserialized, context);

                    // validate CSRF token
                    try
                    {
                        CsrfProtector.VerifyToken(context, context.CsrfToken.NotNull());
                    }
                    catch (SecurityException exc)
                    {
                        await context.InterruptRequestAsync(HttpStatusCode.BadRequest, exc.Message);
                    }

                    if (context.ViewModel is IDotvvmViewModel)
                    {
                        await ((IDotvvmViewModel)context.ViewModel).Load();
                    }

                    // run the load phase in the page
                    DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Load);
                    await requestTracer.TraceEvent(RequestTracingConstants.LoadCompleted, context);

                    // invoke the postback command
                    var actionInfo = ViewModelSerializer.ResolveCommand(context, page).NotNull("Command not found?");

                    // get filters
                    var methodFilters = context.Configuration.Runtime.GlobalFilters.OfType<ICommandActionFilter>()
                        .Concat(ActionFilterHelper.GetActionFilters<ICommandActionFilter>(context.ViewModel.GetType()));
                    if (actionInfo.Binding?.GetProperty<ActionFiltersBindingProperty>(ErrorHandlingMode.ReturnNull) is ActionFiltersBindingProperty filters)
                        methodFilters = methodFilters.Concat(filters.Filters.OfType<ICommandActionFilter>());

                    commandResult = await ExecuteCommand(actionInfo, context, methodFilters);
                    await requestTracer.TraceEvent(RequestTracingConstants.CommandExecuted, context);
                }

                if (context.ViewModel is IDotvvmViewModel)
                {
                    await ((IDotvvmViewModel)context.ViewModel).PreRender();
                }

                // run the prerender phase in the page
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.PreRender);

                // run the prerender complete phase in the page
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.PreRenderComplete);
                await requestTracer.TraceEvent(RequestTracingConstants.PreRenderCompleted, context);

                // generate CSRF token if required
                if (string.IsNullOrEmpty(context.CsrfToken) && !context.Configuration.ExperimentalFeatures.LazyCsrfToken.IsEnabledForRoute(context.Route?.RouteName))
                {
                    context.CsrfToken = CsrfProtector.GenerateToken(context);
                }

                // run OnViewModelSerializing on action filters
                foreach (var filter in viewModelFilters)
                {
                    await filter.OnViewModelSerializingAsync(context);
                }
                await requestTracer.TraceEvent(RequestTracingConstants.ViewModelSerialized, context);

                ViewModelSerializer.BuildViewModel(context, commandResult);

                if (!context.IsInPartialRenderingMode)
                {
                    // standard get
                    await OutputRenderer.WriteHtmlResponse(context, page);
                }
                else
                {
                    // postback or SPA content
                    var postBackUpdates = OutputRenderer.RenderPostbackUpdatedControls(context, page);
                    ViewModelSerializer.AddPostBackUpdatedControls(context, postBackUpdates);

                    // resources must be added after the HTML is rendered - some controls may request resources in the render phase
                    ViewModelSerializer.AddNewResources(context);

                    await OutputRenderer.WriteViewModelResponse(context, page);
                }
                await requestTracer.TraceEvent(RequestTracingConstants.OutputRendered, context);

                foreach (var f in requestFilters) await f.OnPageRenderedAsync(context);
            }
            catch (CorruptedCsrfTokenException) { throw; }
            catch (DotvvmInterruptRequestExecutionException ex) when (ex.InterruptReason == InterruptReason.CachedViewModelMissing)
            {
                // the client needs to repeat the postback and send the full viewmodel
                await context.SetCachedViewModelMissingResponse();
                throw;
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
            finally
            {
                if (context.ViewModel != null)
                {
                    ViewModelLoader.DisposeViewModel(context.ViewModel);
                }
#pragma warning disable CS0618
                StaticCommandServiceLoader.DisposeStaticCommandServices(context);
#pragma warning restore CS0618
            }
        }

        private object? ExecuteStaticCommandPlan(StaticCommandInvocationPlan plan, Queue<JToken> arguments, IDotvvmRequestContext context)
        {
            var methodArgs = plan.Arguments.Select((a, index) =>
                a.Type == StaticCommandParameterType.Argument ? arguments.Dequeue().ToObject((Type)a.Arg!) :
                a.Type == StaticCommandParameterType.Constant || a.Type == StaticCommandParameterType.DefaultValue ? a.Arg :
                a.Type == StaticCommandParameterType.Inject ?
#pragma warning disable CS0618

                                                              StaticCommandServiceLoader.GetStaticCommandService((Type)a.Arg!, context) :
#pragma warning restore CS0618
                a.Type == StaticCommandParameterType.Invocation ? ExecuteStaticCommandPlan((StaticCommandInvocationPlan)a.Arg!, arguments, context) :
                throw new NotSupportedException("" + a.Type)
            ).ToArray();
            return plan.Method.Invoke(plan.Method.IsStatic ? null : methodArgs.First(), plan.Method.IsStatic ? methodArgs : methodArgs.Skip(1).ToArray());
        }

        public async Task ProcessStaticCommandRequest(IDotvvmRequestContext context)
        {
            try
            {
                JObject postData;
                using (var jsonReader = new JsonTextReader(new StreamReader(context.HttpContext.Request.Body)))
                {
                    postData = await JObject.LoadAsync(jsonReader);
                }

                // validate csrf token
                context.CsrfToken = postData["$csrfToken"].Value<string>();
                CsrfProtector.VerifyToken(context, context.CsrfToken);

                var knownTypes = postData["knownTypeMetadata"].Values<string>().ToArray();
                var command = postData["command"].Value<string>();
                var arguments = postData["args"] as JArray;
                var executionPlan =
                    StaticCommandExecutionPlanSerializer.DecryptJson(Convert.FromBase64String(command), context.Services.GetRequiredService<IViewModelProtector>())
                        .Apply(StaticCommandExecutionPlanSerializer.DeserializePlan);

                var actionInfo = new ActionInfo(
                    binding: null,
                    () => { return ExecuteStaticCommandPlan(executionPlan, new Queue<JToken>(arguments.NotNull()), context); },
                    false
                );
                var filters = context.Configuration.Runtime.GlobalFilters.OfType<ICommandActionFilter>()
                    .Concat(executionPlan.GetAllMethods().SelectMany(m => ActionFilterHelper.GetActionFilters<ICommandActionFilter>(m)))
                    .ToArray();

                var result = await ExecuteCommand(actionInfo, context, filters);

                await OutputRenderer.WriteStaticCommandResponse(
                    context,
                    ViewModelSerializer.BuildStaticCommandResponse(context, result, knownTypes));
            }
            finally
            {
#pragma warning disable CS0618
                StaticCommandServiceLoader.DisposeStaticCommandServices(context);
#pragma warning restore CS0618
            }
        }

        protected async Task<object?> ExecuteCommand(ActionInfo action, IDotvvmRequestContext context, IEnumerable<ICommandActionFilter> methodFilters)
        {
            // run OnCommandExecuting on action filters
            foreach (var filter in methodFilters)
            {
                await filter.OnCommandExecutingAsync(context, action);
            }

            try
            {
                var commandResultOrNotYetComputedAwaitable = action.Action();

                if (commandResultOrNotYetComputedAwaitable is Task commandTask)
                {
                    await commandTask;
                    return TaskUtils.GetResult(commandTask);
                }

                var resultType = commandResultOrNotYetComputedAwaitable?.GetType();
                var possibleResultAwaiter = resultType?.GetMethod(nameof(Task.GetAwaiter), new Type[] { });

                if(resultType != null && possibleResultAwaiter != null)
                {
                    throw new NotSupportedException($"The command uses unsupported awaitable type {resultType.FullName}, please use System.Task instead.");
                }
                
                return commandResultOrNotYetComputedAwaitable;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException && ex.InnerException is object)
                {
                    ex = ex.InnerException;
                }
                if (ex is DotvvmInterruptRequestExecutionException)
                {
                    throw new DotvvmInterruptRequestExecutionException("The request execution was interrupted in the command!", ex);
                }
                context.CommandException = ex;
            }
            finally
            {
                // run OnCommandExecuted on action filters
                foreach (var filter in methodFilters.Reverse())
                {
                    await filter.OnCommandExecutedAsync(context, action, context.CommandException);
                }

                if (context.CommandException != null && !context.IsCommandExceptionHandled)
                {
                    throw new Exception("Unhandled exception occurred in the command!", context.CommandException);
                }
            }
            
            return null;
        }

        async Task ValidateSecFetchHeaders(IDotvvmRequestContext context)
        {
            var route = context.Route?.RouteName;
            var isPost = context.HttpContext.Request.Method switch {
                "POST" => true,
                "GET" => false,
                _ => throw new NotSupportedException()
            };
            var checksAllowed = (isPost ? SecurityConfiguration.VerifySecFetchForCommands : SecurityConfiguration.VerifySecFetchForPages).IsEnabledForRoute(route);
            var dest = context.HttpContext.Request.Headers["Sec-Fetch-Dest"];
            var site = context.HttpContext.Request.Headers["Sec-Fetch-Site"];

            if (SecurityConfiguration.RequireSecFetchHeaders.IsEnabledForRoute(route))
                if (string.IsNullOrEmpty(dest) || string.IsNullOrEmpty(site))
                    await context.RejectRequest("Sec-Fetch-Dest header is required. Please, use a web browser with security in mind: https://www.mozilla.org/en-US/firefox/new/");

            // if the request has Dest: iframe, check if we allow iframes. Otherwise, we can throw an error right away, since the iframe will not load anyway due to the X-Frame-Options header

            if (dest is "frame" or "iframe")
            {
                if (SecurityConfiguration.FrameOptionsCrossOrigin.IsEnabledForRoute(route))
                { // fine
                }
                else if (SecurityConfiguration.FrameOptionsSameOrigin.IsEnabledForRoute(route) && site == "same-origin")
                { // samesite allowed - also fine
                }
                else
                {
                    if (site == "same-origin")
                        await context.RejectRequest($"Same site iframe are disabled in this application. If you are the developer, you can enable iframes by setting DotvvmConfiguration.Security.FrameOptionsSameOrigin.EnableForRoute(\"{route}\")");
                    else
                        await context.RejectRequest($"Cross site iframe are disabled in this application. If you are the developer, you can enable cross-site iframes by setting DotvvmConfiguration.Security.FrameOptionsCrossOrigin.EnableForRoute(\"{route}\"). Note that it's not recommended to enable cross-site iframes for sites / pages where security is important (due to Clickjacking)");
                }
            }

            if (!checksAllowed || string.IsNullOrEmpty(dest) || string.IsNullOrEmpty(site))
                return;

            if (isPost)
            {
                if (site != "same-origin")
                    await context.RejectRequest($"Cross site postbacks are disabled.");
                if (dest != "empty")
                    await context.RejectRequest($"postbacks must have Sec-Fetch-Dest: empty");
            }
            else
            {
                if (dest is "document" or "frame" or "iframe")
                { // fine, this is allowed even cross-site
                }
                // if SPA is used, dest will be empty, since it's initiated from JS
                // we only allow this with the X-DotVVM-SpaContentPlaceHolder header
                // we "trust" the client - as if he lies about it being a SPA request,
                // he'll will just get a redirect response, not anything useful
                else if (dest is "empty")
                {
                    if (!DetermineSpaRequest(context.HttpContext))
                        await context.RejectRequest($"Pages can not be loaded using Javascript for security reasons. If you are the developer, you can disable this check by setting DotvvmConfiguration.Security.VerifySecFetchForPages.DisableForRoute(\"{route}\")");
                    if (site != "same-origin")
                        await context.RejectRequest($"Cross site SPA requests are disabled.");
                }
                else
                    await context.RejectRequest("Can not load a DotVVM page with this Sec-Fetch-Dest.");
            }
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

        public static string? DetermineSpaContentPlaceHolderUniqueId(IHttpContext context)
        {
            return context.Request.Headers[HostingConstants.SpaContentPlaceHolderHeaderName];
        }
    }
}
