#nullable enable
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Ensures that user is authenticated.
    /// </summary>
    public class AuthorizeActionFilter : IPageActionFilter, ICommandActionFilter, IViewModelActionFilter, IPresenterActionFilter
    {
        private static readonly ConcurrentDictionary<Type, bool> isAnonymousAllowedCache = new ConcurrentDictionary<Type, bool>();

        /// <param name="policy">The name of the policy to require for authorization.</param>
        /// <param name="roles">list of roles that are allowed to access the resource</param>
        public AuthorizeActionFilter(string[]? roles = null)
        {
            Roles = roles ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets a list of roles that are allowed to access the resource.
        /// </summary>
        public string[] Roles { get; }

        /// <inheritdoc />
        protected Task OnViewModelCreatedAsync(IDotvvmRequestContext context)
        {
            Authorize(context, context.ViewModel);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            Authorize(context, null);
            return Task.CompletedTask;
        }
        protected Task OnPresenterExecutingAsync(IDotvvmRequestContext context)
        {
            Authorize(context, context.Presenter);
            return Task.CompletedTask;
        }
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

        private void Authorize(IDotvvmRequestContext context, object? appliedOn)
        {
            if (!CanBeAuthorized(appliedOn ?? context.ViewModel))
            {
                return;
            }
            context.Authorize(Roles);
        }

        private static readonly ConcurrentDictionary<Type, bool> canBeAuthorizedCache = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Returns whether the view model does require authorization.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        internal static bool CanBeAuthorized(object? viewModel)
            => viewModel == null || canBeAuthorizedCache.GetOrAdd(viewModel.GetType(), t => !t.GetTypeInfo().IsDefined(typeof(NotAuthorizedAttribute)));

    }
}
