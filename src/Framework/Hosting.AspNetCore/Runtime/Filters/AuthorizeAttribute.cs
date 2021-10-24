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
    /// Specifies that the class or method requires the specified authorization.
    /// </summary>
    [Obsolete("Please use the Context.Authorize method instead. You can call it, for example, from Init or from any of your commands. If you are using GlobalFilters, use AuthorizeActionFilter.")]
    public class AuthorizeAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<Type, bool> isAnonymousAllowedCache = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Microsoft.AspNetCore.Authorization.AuthorizeAttribute" /> class.
        /// </summary>
        public AuthorizeAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Microsoft.AspNetCore.Authorization.AuthorizeAttribute" /> class with the
        /// specified policy.
        /// </summary>
        /// <param name="policy">The name of the policy to require for authorization.</param>
        public AuthorizeAttribute(string policy)
        {
            Policy = policy;
        }

        /// <summary>
        /// Gets or sets the policy name that determines access to the resource.
        /// </summary>
        public string Policy { get; set; }

        /// <summary>
        /// Gets or sets a comma delimited list of roles that are allowed to access the resource.
        /// </summary>
        public string Roles { get; set; }

        /// <summary>
        /// Gets or sets a comma delimited list of schemes from which user information is constructed.
        /// </summary>
        public string AuthenticationSchemes { get; set; }

        /// <inheritdoc />
        protected override Task OnViewModelCreatedAsync(IDotvvmRequestContext context)
            => Authorize(context, context.ViewModel);

        /// <inheritdoc />
        protected override Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
            => Authorize(context, null);

        protected override Task OnPresenterExecutingAsync(IDotvvmRequestContext context)
            => Authorize(context, context.Presenter);

        private async Task Authorize(IDotvvmRequestContext context, object appliedOn)
        {
            if (!AuthorizeActionFilter.CanBeAuthorized(appliedOn ?? context.ViewModel))
            {
                return;
            }

            await context.Authorize(
                roles: this.Roles?.Split(",").Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r)).ToArray(),
                policyName: this.Policy,
                authenticationSchemes: this.AuthenticationSchemes?.Split(",").Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r)).ToArray(),
                allowAnonymous: AuthorizeActionFilter.IsAnonymousAllowed(appliedOn)
            );
        }
    }
}
