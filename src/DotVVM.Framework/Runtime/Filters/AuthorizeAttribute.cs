using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DotVVM.Framework.Hosting;
using System.Collections.Concurrent;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// A filter that checks the authorize attributes and redirects to the login page.
    /// </summary>
    public class AuthorizeAttribute : ActionFilterAttribute
    {

        /// <summary>
        /// Gets or sets the comma-separated list of roles.
        /// </summary>
        public string[] Roles { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class.
        /// </summary>
        public AuthorizeAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="roles">The comma-separated list of roles. The user must be at least in one of them.</param>
        public AuthorizeAttribute(string roles)
        {
            Roles = roles.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
        }

        /// <summary>
        /// Called before the command is invoked.
        /// </summary>
        protected internal override void OnCommandExecuting(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            Authorize(context);
            base.OnCommandExecuting(context, actionInfo);
        }

        protected internal override void OnViewModelCreated(IDotvvmRequestContext context)
        {
            Authorize(context);
            base.OnViewModelCreated(context);
        }

        public void Authorize(IDotvvmRequestContext context)
        {
            // check for [NotAuthorized] attribute
            if (!CanBeAuthorized(context.ViewModel.GetType())) return;

            // the user must not be anonymous
            if (context.OwinContext.Request.User == null || !context.OwinContext.Request.User.Identity.IsAuthenticated)
            {
                SetUnauthorizedResponse(context);
            }

            // if the role is set
            if (Roles != null && Roles.Length > 0)
            {
                if (!Roles.Any(r => context.OwinContext.Request.User.IsInRole(r)))
                {
                    SetUnauthorizedResponse(context);
                }
            }
        }

        private static ConcurrentDictionary<Type, bool> canBeAuthorizedCache = new ConcurrentDictionary<Type, bool>();
        protected static bool CanBeAuthorized(Type viewModelType)
        {
            return canBeAuthorizedCache.GetOrAdd(viewModelType, t => IsDefined(t, typeof(NotAuthorizedAttribute)));
        }

        protected virtual void SetUnauthorizedResponse(IDotvvmRequestContext context)
        {
            throw new UnauthorizedAccessException();
        }
    }
}