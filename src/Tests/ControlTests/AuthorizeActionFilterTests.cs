#if DotNetCore
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests;
[TestClass]
public class AuthorizeActionFilterTests
{
    private static readonly ActionInfo commandAction = new ActionInfo(
        binding: null,
        action: () => null,
        isControlCommand: false,
        invokedMethod: null,
        argumentPaths: null
    );

    [TestMethod]
    public async Task ViewModelCreated_UnauthenticatedUser_IsRejected()
    {
        var filter = new AuthorizeActionFilter();
        var context = CreateContext(new SecuredViewModel());

        await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(
            () => ((IViewModelActionFilter)filter).OnViewModelCreatedAsync(context)
        );

        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.HttpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task CommandExecuting_UnauthenticatedUser_IsRejected()
    {
        var filter = new AuthorizeActionFilter();
        var context = CreateContext(new SecuredViewModel());

        await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(
            () => ((ICommandActionFilter)filter).OnCommandExecutingAsync(context, commandAction)
        );

        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.HttpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task PresenterExecuting_UnauthenticatedUser_IsRejected()
    {
        var filter = new AuthorizeActionFilter();
        var context = CreateContext(new SecuredViewModel());
        context.Presenter = new SecuredPresenter();

        await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(
            () => ((IPresenterActionFilter)filter).OnPresenterExecutingAsync(context)
        );

        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.HttpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task ViewModelCreated_NotAuthorizedAttribute_SkipsAuthorization()
    {
        var filter = new AuthorizeActionFilter();
        var context = CreateContext(new NotAuthorizedViewModel());

        await ((IViewModelActionFilter)filter).OnViewModelCreatedAsync(context);

        Assert.AreEqual(StatusCodes.Status200OK, context.HttpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task ViewModelCreated_AllowAnonymousAttribute_SkipsAuthorization()
    {
        var filter = new AuthorizeActionFilter();
        var context = CreateContext(new AnonymousViewModel());

        await ((IViewModelActionFilter)filter).OnViewModelCreatedAsync(context);

        Assert.AreEqual(StatusCodes.Status200OK, context.HttpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task PresenterExecuting_AllowAnonymousAttribute_SkipsAuthorization()
    {
        var filter = new AuthorizeActionFilter();
        var context = CreateContext(new SecuredViewModel());
        context.Presenter = new AnonymousPresenter();

        await ((IPresenterActionFilter)filter).OnPresenterExecutingAsync(context);

        Assert.AreEqual(StatusCodes.Status200OK, context.HttpContext.Response.StatusCode);
    }

    private static TestDotvvmRequestContext CreateContext(object viewModel)
    {
        var serviceProvider = CreateServiceProvider();
        var coreContext = new DefaultHttpContext {
            RequestServices = serviceProvider,
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        var httpContext = new DotvvmHttpContext(coreContext);
        httpContext.Request = new DotvvmHttpRequest(coreContext.Request, httpContext);
        httpContext.Response = new DotvvmHttpResponse(coreContext.Response, httpContext, new DotvvmHeaderCollection(coreContext.Response.Headers));

        return new TestDotvvmRequestContext {
            Services = serviceProvider,
            HttpContext = httpContext,
            ViewModel = viewModel,
            Presenter = new SecuredPresenter()
        };
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        services.AddAuthorization(options => {
            options.DefaultPolicy = new AuthorizationPolicyBuilder("Test")
                .RequireAuthenticatedUser()
                .Build();
        });
        return services.BuildServiceProvider();
    }

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder
        ) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
            Task.FromResult(AuthenticateResult.NoResult());

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    }

    private sealed class SecuredViewModel
    {
    }

    [NotAuthorized]
    private sealed class NotAuthorizedViewModel
    {
    }

    [AllowAnonymous]
    private sealed class AnonymousViewModel
    {
    }

    private sealed class SecuredPresenter : IDotvvmPresenter
    {
        public Task ProcessRequest(IDotvvmRequestContext context) => Task.CompletedTask;
    }

    [AllowAnonymous]
    private sealed class AnonymousPresenter : IDotvvmPresenter
    {
        public Task ProcessRequest(IDotvvmRequestContext context) => Task.CompletedTask;
    }
}
#endif
