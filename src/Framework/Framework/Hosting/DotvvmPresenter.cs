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
using System.Diagnostics;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Framework.Hosting
{
    [NotAuthorized] // DotvvmPresenter handles authorization itself, allowing authorization on it would make [NotAuthorized] attribute useless on ViewModel, since request would be interrupted earlier that VM is found
    public class DotvvmPresenter : IDotvvmPresenter
    {
        private readonly DotvvmConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmPresenter" /> class.
        /// </summary>
        public DotvvmPresenter(DotvvmConfiguration configuration, IDotvvmViewBuilder viewBuilder, IViewModelLoader viewModelLoader, IViewModelSerializer viewModelSerializer,
            IOutputRenderer outputRender, ICsrfProtector csrfProtector, IViewModelParameterBinder viewModelParameterBinder,
            StaticCommandExecutor staticCommandExecutor
        )
        {
            this.configuration = configuration;

            DotvvmViewBuilder = viewBuilder;
            ViewModelLoader = viewModelLoader;
            ViewModelSerializer = viewModelSerializer;
            OutputRenderer = outputRender;
            CsrfProtector = csrfProtector;
            ViewModelParameterBinder = viewModelParameterBinder;
            StaticCommandExecutor = staticCommandExecutor;
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

        public StaticCommandExecutor StaticCommandExecutor { get; }

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
            if (context.RequestType == DotvvmRequestType.Unknown)
            {
                await context.InterruptRequestAsMethodNotAllowedAsync();
            }

            await ValidateSecFetchHeaders(context);

            var requestTracer = context.Services.GetRequiredService<AggregateRequestTracer>();
            if (context.RequestType == DotvvmRequestType.StaticCommand)
            {
                await ProcessStaticCommandRequest(context);
                await requestTracer.TraceEvent(RequestTracingConstants.StaticCommandExecuted, context);
                return;
            }
            var isPostBack = context.RequestType == DotvvmRequestType.Command;

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
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.PreInit, context);
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
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Init, context);
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
                    DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Load, context);
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
                    DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.Load, context);
                    await requestTracer.TraceEvent(RequestTracingConstants.LoadCompleted, context);

                    // invoke the postback command
                    var actionInfo = ViewModelSerializer.ResolveCommand(context, page).NotNull("Command not found?");

                    // get filters
                    var methodFilters = context.Configuration.Runtime.GlobalFilters.OfType<ICommandActionFilter>()
                        .Concat(ActionFilterHelper.GetActionFilters<ICommandActionFilter>(context.ViewModel.GetType()));
                    if (actionInfo.Binding?.GetProperty<ActionFiltersBindingProperty>(ErrorHandlingMode.ReturnNull) is ActionFiltersBindingProperty filters)
                        methodFilters = methodFilters.Concat(filters.Filters.OfType<ICommandActionFilter>());
                    
                    var commandTimer = ValueStopwatch.StartNew();
                    try
                    {
                        commandResult = await ExecuteCommand(actionInfo, context, methodFilters);
                    }
                    finally
                    {
                        DotvvmMetrics.CommandInvocationDuration.Record(
                            commandTimer.ElapsedSeconds,
                            new KeyValuePair<string, object?>("command", actionInfo.Binding!.ToString()),
                            new KeyValuePair<string, object?>("result", context.CommandException is null ? "Ok" :
                                                                        context.IsCommandExceptionHandled ? "HandledException" :
                                                                        "UnhandledException"));
                    }
                    await requestTracer.TraceEvent(RequestTracingConstants.CommandExecuted, context);
                }

                if (context.ViewModel is IDotvvmViewModel)
                {
                    await ((IDotvvmViewModel)context.ViewModel).PreRender();
                }

                // run the prerender phase in the page
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.PreRender, context);

                // run the prerender complete phase in the page
                DotvvmControlCollection.InvokePageLifeCycleEventRecursive(page, LifeCycleEventType.PreRenderComplete, context);
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

                if (context.RequestType == DotvvmRequestType.Navigate)
                {
                    await OutputRenderer.WriteHtmlResponse(context, page);
                }
                else
                {
                    Debug.Assert(context.RequestType is DotvvmRequestType.Command or DotvvmRequestType.SpaNavigate);
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
                StaticCommandExecutor.DisposeServices(context);
            }
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
                var argumentPaths = postData["argumentPaths"]?.Values<string?>().ToArray();
                var command = postData["command"].Value<string>();
                var arguments = postData["args"] as JArray;
                var executionPlan = StaticCommandExecutor.DecryptPlan(command);

                var actionInfo = new ActionInfo(
                    binding: null,
                    () => { return StaticCommandExecutor.Execute(executionPlan, arguments.NotNull(), argumentPaths, context); },
                    false,
                    executionPlan.Method,
                    argumentPaths
                );
                var filters = context.Configuration.Runtime.GlobalFilters.OfType<ICommandActionFilter>()
                    .Concat(executionPlan.GetAllMethods().SelectMany(m => ActionFilterHelper.GetActionFilters<ICommandActionFilter>(m)))
                    .ToArray();

                var commandTimer = ValueStopwatch.StartNew();
                object? result = null;
                try
                {
                    result = await ExecuteCommand(actionInfo, context, filters);
                }
                finally
                {
                    DotvvmMetrics.StaticCommandInvocationDuration.Record(
                        commandTimer.ElapsedSeconds,
                        new KeyValuePair<string, object?>("command", executionPlan.ToString()),
                        new KeyValuePair<string, object?>("result", context.CommandException is null ? "Ok" :
                                                                    context.IsCommandExceptionHandled ? "HandledException" :
                                                                    "UnhandledException"));
                }

                await OutputRenderer.WriteStaticCommandResponse(
                    context,
                    ViewModelSerializer.BuildStaticCommandResponse(context, result, knownTypes));
            }
            finally
            {
                StaticCommandExecutor.DisposeServices(context);
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

                return await TaskUtils.ToObjectTask(commandResultOrNotYetComputedAwaitable);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException && ex.InnerException is object)
                {
                    ex = ex.InnerException;
                }

                if (ex is DotvvmInvalidStaticCommandModelStateException { StaticCommandModelState: {} staticCommandModelState })
                {
                    if (context.RequestType != DotvvmRequestType.StaticCommand)
                        throw new InvalidOperationException($"The StaticCommandModelState type may only be used in StaticCommand requests. Please use Context.ModelState in Commands.");
                    await RespondWithStaticCommandValidationFailure(action, context, staticCommandModelState);
                    context.IsCommandExceptionHandled = true;
                }
                else if (ex is DotvvmInterruptRequestExecutionException)
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

        async Task RespondWithStaticCommandValidationFailure(ActionInfo action, IDotvvmRequestContext context, StaticCommandModelState staticCommandModelState)
        {
            var invokedMethod = action.InvokedMethod!;
            var staticCommandAttribute = invokedMethod.GetCustomAttribute<AllowStaticCommandAttribute>();
            if (staticCommandAttribute?.Validation == StaticCommandValidation.None)
                throw new Exception($"Could not respond with validation failure, validation is disabled on method {ReflectionUtils.FormatMethodInfo(invokedMethod)}. Use [AllowStaticCommand(StaticCommandValidation.Manual)] to allow validation.");

            if (staticCommandModelState.Errors.FirstOrDefault(e => !e.IsResolved) is {} unresolvedError)
                throw new Exception("Could not respond with validation failure, some errors have unresolved paths: " + unresolvedError);

            DotvvmMetrics.ValidationErrorsReturned.Record(
                staticCommandModelState.ErrorsInternal.Count,
                context.RouteLabel(),
                context.RequestTypeLabel()
            );

            var jObject = new JObject
            {
                [ "modelState" ] = JArray.FromObject(staticCommandModelState.Errors),
                [ "action" ] = "validationErrors"
            };
            var result = jObject.ToString();

            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(result);
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.ArgumentsValidationFailed, "Argument contain validation errors!");
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
                        await context.RejectRequest($"""
                            Same site iframe are disabled in this application.
                            If you are the developer, you can enable iframes by setting DotvvmConfiguration.Security.FrameOptionsSameOrigin.IncludeRoute("{route}")
                            """);
                    else
                        await context.RejectRequest($"""
                        Cross site iframe are disabled in this application.
                        If you are the developer, you can enable cross-site iframes by setting DotvvmConfiguration.Security.FrameOptionsCrossOrigin.IncludeRoute("{route}"). Note that it's not recommended to enable cross-site iframes for sites / pages where security is important (due to Clickjacking)
                        """);
                }
            }

            if (!checksAllowed || string.IsNullOrEmpty(dest) || string.IsNullOrEmpty(site))
                return;

            if (isPost)
            {
                if (site != "same-origin")
                    await context.RejectRequest($"Cross site postbacks are disabled.");
                if (dest != "empty")
                    await context.RejectRequest($"Postbacks must have Sec-Fetch-Dest: empty");
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
                    if (context.RequestType is not DotvvmRequestType.SpaNavigate)
                        await context.RejectRequest($"""
                            Pages can not be loaded using Javascript for security reasons.
                            Try refreshing the page to get rid of the error.
                            If you are the developer, you can disable this check by setting DotvvmConfiguration.Security.VerifySecFetchForPages.ExcludeRoute("{route}"). [dest: {dest}, site: {site}]
                            """);
                    if (site != "same-origin")
                        await context.RejectRequest($"Cross site SPA requests are disabled.");
                }
                else
                    await context.RejectRequest($"Cannot load a DotVVM page with Sec-Fetch-Dest: {dest}.");
            }
        }

        [Obsolete("Use context.RequestType == DotvvmRequestType.StaticCommand")]
        public static bool DetermineIsStaticCommand(IDotvvmRequestContext context) =>
            context.RequestType == DotvvmRequestType.StaticCommand;
        [Obsolete("Use context.RequestType == DotvvmRequestType.Command")]
        public static bool DetermineIsPostBack(IHttpContext context) =>
            DotvvmRequestContext.DetermineRequestType(context) == DotvvmRequestType.Command;

        [Obsolete("Use context.RequestType == DotvvmRequestType.SpaGet")]
        public static bool DetermineSpaRequest(IHttpContext context) =>
            DotvvmRequestContext.DetermineRequestType(context) == DotvvmRequestType.SpaNavigate;

        [Obsolete("Use context.RequestType is DotvvmRequestType.Command or DotvvmRequestType.SpaGet")]
        public static bool DeterminePartialRendering(IHttpContext context) =>
            DotvvmRequestContext.DetermineRequestType(context) is DotvvmRequestType.Command or DotvvmRequestType.SpaNavigate;

        public static string? DetermineSpaContentPlaceHolderUniqueId(IHttpContext context)
        {
            return context.Request.Headers[HostingConstants.SpaContentPlaceHolderHeaderName];
        }
    }
}
