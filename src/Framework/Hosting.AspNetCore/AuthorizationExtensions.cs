#nullable enable
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting
{
    public static class AuthorizationExtensions
    {
        public static async Task<ClaimsPrincipal?> Authorize(
            this IDotvvmRequestContext context,
            string[]? roles = null,
            string? policyName = null,
            string[]? authenticationSchemes = null,
            bool allowAnonymous = false
        )
        {
            var policy = await GetAuthorizationPolicy(
                context,
                policyName,
                roles ?? Array.Empty<string>(),
                authenticationSchemes ?? Array.Empty<string>()
            );

            if (policy == null)
            {
                return null;
            }

            var coreContext = context.GetAspNetCoreContext();

            if (policy.AuthenticationSchemes != null && policy.AuthenticationSchemes.Count > 0)
            {
                ClaimsPrincipal? principal = null;

                foreach (var scheme in policy.AuthenticationSchemes)
                {
                    var result = await coreContext.AuthenticateAsync(scheme);

                    if (result.Succeeded && result.Principal != null)
                    {
                        principal = MergeUserPrincipal(principal, result.Principal);
                    }
                }

                if (principal == null)
                {
                    principal = new ClaimsPrincipal(new ClaimsIdentity());
                }

                coreContext.User = principal;
            }

            var authService = coreContext.RequestServices.GetRequiredService<IAuthorizationService>();

            if (!allowAnonymous && !(await authService.AuthorizeAsync(coreContext.User, context, policy)).Succeeded)
            {
                if (coreContext.User.Identity?.IsAuthenticated == true)
                {
                    await HandleUnauthorizedRequest(coreContext, policy);
                }
                else
                {
                    await HandleUnauthenticatedRequest(coreContext, policy);
                }
            }
            return coreContext.User;
        }


        private static async Task<AuthorizationPolicy> GetAuthorizationPolicy(IDotvvmRequestContext context, string? policyName, string[] roles, string[] authenticationSchemes)
        {
            var policyProvider = context.Services.GetRequiredService<IAuthorizationPolicyProvider>();

            var policyBuilder = new AuthorizationPolicyBuilder();
            var useDefaultPolicy = true;

            if (!string.IsNullOrWhiteSpace(policyName))
            {
                var policy = await policyProvider.GetPolicyAsync(policyName);
                if (policy == null)
                {
                    throw new InvalidOperationException($"The policy '{policyName}' could not be found!");
                }
                policyBuilder.Combine(policy);
                useDefaultPolicy = false;
            }
            if (roles.Any())
            {
                policyBuilder.RequireRole(roles);
                useDefaultPolicy = false;
            }
            foreach (var authType in authenticationSchemes)
            {
                policyBuilder.AuthenticationSchemes.Add(authType);
            }
            if (useDefaultPolicy)
            {
                policyBuilder.Combine(await policyProvider.GetDefaultPolicyAsync());
            }

            return policyBuilder.Build();
        }

        private static async Task HandleUnauthenticatedRequest(HttpContext context, AuthorizationPolicy policy)
        {
            if (policy.AuthenticationSchemes != null && policy.AuthenticationSchemes.Count > 0)
            {
                foreach (var scheme in policy.AuthenticationSchemes)
                {
                    await context.ChallengeAsync(scheme);
                }
            }
            else
            {
                await context.ChallengeAsync();
            }

            throw new DotvvmInterruptRequestExecutionException("User unauthenticated");
        }

        private static async Task HandleUnauthorizedRequest(HttpContext context, AuthorizationPolicy policy)
        {
            if (policy.AuthenticationSchemes != null && policy.AuthenticationSchemes.Count > 0)
            {
                foreach (var scheme in policy.AuthenticationSchemes)
                {
                    await context.ForbidAsync(scheme);
                }
            }
            else
            {
                await context.ForbidAsync();
            }

            throw new DotvvmInterruptRequestExecutionException("User unauthorized");
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
