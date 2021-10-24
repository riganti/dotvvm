#nullable enable
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Ensures that user is authenticated.
    /// </summary>
    public class AuthorizeActionFilter: IPageActionFilter, ICommandActionFilter, IViewModelActionFilter, IPresenterActionFilter
    {
        private static readonly ConcurrentDictionary<Type, bool> isAnonymousAllowedCache = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Microsoft.AspNetCore.Authorization.AuthorizeAttribute" /> class with the
        /// specified policy.
        /// </summary>
        /// <param name="policy">The name of the policy to require for authorization.</param>
        /// <param name="roles">list of roles that are allowed to access the resource</param>
        /// <param name="authenticationSchemes">list of schemes from which user information is constructed</param>
        /// <param name="allowAnonymous">If set to true, anonymous users are allowed</param>
        public AuthorizeActionFilter(
            string[]? roles = null,
            string? policy = null,
            string[]? authenticationSchemes = null,
            bool allowAnonymous = false
        )
        {
            Policy = policy;
            Roles = roles ?? Array.Empty<string>();
            AuthenticationSchemes = authenticationSchemes ?? Array.Empty<string>();
            AllowsAnonymous = allowAnonymous;
        }

        /// <summary>
        /// Gets the policy name that determines access to the resource.
        /// </summary>
        public string? Policy { get; }

        /// <summary>
        /// Gets a list of roles that are allowed to access the resource.
        /// </summary>
        public string[] Roles { get; }

        /// <summary>
        /// Gets a list of schemes from which user information is constructed.
        /// </summary>
        public string[] AuthenticationSchemes { get; }

        /// <summary>
        /// If set to true, anonymous users are allowed.
        /// </summary>
        public bool AllowsAnonymous { get; }

        /// <inheritdoc />
        protected  Task OnViewModelCreatedAsync(IDotvvmRequestContext context)
            => Authorize(context, context.ViewModel);

        /// <inheritdoc />
        protected Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
            => Authorize(context, null);

        protected Task OnPresenterExecutingAsync(IDotvvmRequestContext context)
            => Authorize(context, context.Presenter);

        public Task OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception) => Task.CompletedTask;
        public Task OnPageInitializedAsync(IDotvvmRequestContext context) => Task.CompletedTask;
        public Task OnPageRenderedAsync(IDotvvmRequestContext context) => Task.CompletedTask;
        Task ICommandActionFilter.OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo) => Task.CompletedTask;
        public Task OnCommandExecutedAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception? exception) => Task.CompletedTask;
        Task IViewModelActionFilter.OnViewModelCreatedAsync(IDotvvmRequestContext context) => Task.CompletedTask;
        public Task OnViewModelDeserializedAsync(IDotvvmRequestContext context) => Task.CompletedTask;
        public Task OnViewModelSerializingAsync(IDotvvmRequestContext context) => Task.CompletedTask;
        Task IPresenterActionFilter.OnPresenterExecutingAsync(IDotvvmRequestContext context) => Task.CompletedTask;
        public Task OnPresenterExecutedAsync(IDotvvmRequestContext context) => Task.CompletedTask;
        public Task OnPresenterExceptionAsync(IDotvvmRequestContext context, Exception exception) => Task.CompletedTask;

        private async Task Authorize(IDotvvmRequestContext context, object? appliedOn)
        {
            if (!CanBeAuthorized(appliedOn ?? context.ViewModel))
            {
                return;
            }

            await context.Authorize(
                roles: this.Roles,
                policyName: this.Policy,
                authenticationSchemes: this.AuthenticationSchemes,
                allowAnonymous: AllowsAnonymous || IsAnonymousAllowed(appliedOn)
            );
        }

        private static readonly ConcurrentDictionary<Type, bool> canBeAuthorizedCache = new ConcurrentDictionary<Type, bool>();
        /// <summary>
        /// Returns whether the view model does require authorization.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        internal static bool CanBeAuthorized(object? viewModel)
            => viewModel == null || canBeAuthorizedCache.GetOrAdd(viewModel.GetType(), t => !t.GetTypeInfo().IsDefined(typeof(NotAuthorizedAttribute)));

        internal static bool IsAnonymousAllowed(object? viewModel)
            => viewModel != null && isAnonymousAllowedCache.GetOrAdd(viewModel.GetType(), t => t.GetTypeInfo().GetCustomAttributes().OfType<IAllowAnonymous>().Any());

    }
}
