using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Microsoft.Owin;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Specifies that the class or method requires the specified authorization.
    /// </summary>
    [Obsolete("Please use the Context.Authorize method instead. You can call it, for example, from Init or any of your commands. If you are using GlobalFilters, use AuthorizeActionFilter.")]
    public class AuthorizeAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeAttribute" /> class.
        /// </summary>
        public AuthorizeAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeAttribute" /> class.
        /// </summary>
        /// <param name="roles">The comma-separated list of roles. The user must be at least in one of them.</param>
        public AuthorizeAttribute(string roles)
        {
            Roles = roles.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
        }

        /// <summary>
        /// Gets or sets the list of allowed roles.
        /// </summary>
        public string[] Roles { get; set; }

        /// <inheritdoc />
        protected internal override Task OnViewModelCreatedAsync(IDotvvmRequestContext context)
        {
            Authorize(context, context.ViewModel);
            return TaskUtils.GetCompletedTask();
        }

        /// <inheritdoc />
        protected internal override Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            Authorize(context, null);
            return TaskUtils.GetCompletedTask();
        }

        /// <inheritdoc />
        protected internal override Task OnPresenterExecutingAsync(IDotvvmRequestContext context)
        {
            Authorize(context, context.Presenter);
            return TaskUtils.GetCompletedTask();
        }

        /// <summary>
        /// Called when a request is being authorized. The authorization fails if: a) no user is associated with the request;
        /// b) the user is not authenticated; c) the user is not in any of the authorized <see cref="Roles" />.
        /// </summary>
        /// <param name="context">The request context.</param>
        /// <param name="appliedOn">The object which can contain [NotAuthorizedAttribute] that could suppress it.</param>
        protected virtual void Authorize(IDotvvmRequestContext context, object appliedOn)
        {
            if (!CanBeAuthorized(appliedOn ?? context.ViewModel))
            {
                return;
            }

            var owinContext = context.GetOwinContext();

            if (!IsUserAuthenticated(owinContext))
            {
                HandleUnauthenticatedRequest(owinContext);
            }
            if (!IsUserAuthorized(owinContext))
            {
                HandleUnauthorizedRequest(owinContext);
            }
        }

        private static readonly ConcurrentDictionary<Type, bool> canBeAuthorizedCache = new ConcurrentDictionary<Type, bool>();
        /// <summary>
        /// Returns whether the view model does require authorization.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        protected bool CanBeAuthorized(object viewModel)
            => viewModel == null || canBeAuthorizedCache.GetOrAdd(viewModel.GetType(), t => !t.GetTypeInfo().IsDefined(typeof(NotAuthorizedAttribute)));

        /// <summary>
        /// Handles requests that is not authenticated.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        protected virtual void HandleUnauthenticatedRequest(IOwinContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            throw new DotvvmInterruptRequestExecutionException();
        }

        /// <summary>
        /// Handles requests that fail authorization.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        protected virtual void HandleUnauthorizedRequest(IOwinContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
            throw new DotvvmInterruptRequestExecutionException();
        }

        /// <summary>
        /// Returns whether the current user is authenticated (and is not anonymous).
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        protected virtual bool IsUserAuthenticated(IOwinContext context)
        {
            var identity = context.Authentication.User?.Identity;
            return identity != null && identity.IsAuthenticated;
        }

        /// <summary>
        /// Returns whether the current user is in on of the specified <see cref="Roles" />.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        protected virtual bool IsUserAuthorized(IOwinContext context)
        {
            var user = context.Authentication.User;

            if (user == null)
            {
                return false;
            }

            if (Roles != null && Roles.Length > 0)
            {
                return Roles.Any(r => user.IsInRole(r));
            }

            return true;
        }
    }
}
