#nullable enable
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace DotVVM.Framework.Hosting
{
    public static class AuthorizationExtensions
    {
        public static void Authorize(this IDotvvmRequestContext context, string role) => Authorize(context, new[] { role });
        public static void Authorize(this IDotvvmRequestContext context, string[]? roles = null)
        {
            var owinContext = context.GetOwinContext();

            if (!IsUserAuthenticated(owinContext))
            {
                HandleUnauthenticatedRequest(owinContext);
            }
            if (!IsUserAuthorized(owinContext, roles))
            {
                HandleUnauthorizedRequest(owinContext);
            }
        }

        /// <summary>
        /// Returns whether the current user is authenticated (and is not anonymous).
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        private static bool IsUserAuthenticated(IOwinContext context)
        {
            var identity = context.Authentication.User?.Identity;
            return identity != null && identity.IsAuthenticated;
        }

        /// <summary>
        /// Returns whether the current user is in on of the specified <see cref="Roles" />.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        private static bool IsUserAuthorized(IOwinContext context, string[]? roles)
        {
            var user = context.Authentication.User;

            if (user == null)
            {
                return false;
            }

            if (roles != null && roles.Length > 0)
            {
                return roles.Any(r => user.IsInRole(r));
            }

            return true;
        }
        /// <summary>
        /// Handles requests that is not authenticated.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        private static void HandleUnauthenticatedRequest(IOwinContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            throw new DotvvmInterruptRequestExecutionException();
        }

        /// <summary>
        /// Handles requests that fail authorization.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        private static void HandleUnauthorizedRequest(IOwinContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            throw new DotvvmInterruptRequestExecutionException();
        }

        private static ClaimsPrincipal MergeUserPrincipal(ClaimsPrincipal? existingPrincipal, ClaimsPrincipal? additionalPrincipal)
        {
            if (existingPrincipal is null)
                return additionalPrincipal!;
            if (additionalPrincipal is null)
                return existingPrincipal;

            var result = new ClaimsPrincipal();
            result.AddIdentities(additionalPrincipal.Identities);
            result.AddIdentities(existingPrincipal.Identities.Where(i => i.IsAuthenticated || i.Claims.Any()));
            return result;
        }
    }
}
