using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Recognizes <see cref="AuthorizeAttribute" /> filters and applies a specific <see cref="AuthorizationPolicy" />.
    /// </summary>
    public class AuthorizeFilterAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<Type, bool> isAnonymousAllowedCache = new ConcurrentDictionary<Type, bool>();

        /// <inheritdoc />
        protected override Task OnViewModelCreatedAsync(IDotvvmRequestContext context)
            => Authorize(context, null);

        /// <inheritdoc />
        protected override Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
            => Authorize(context, actionInfo);

        private async Task Authorize(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            var policy = await GetAuthorizationPolicy(context, actionInfo);

            if (policy == null)
            {
                return;
            }

            var coreContext = context.GetAspNetCoreContext();

            if (policy.AuthenticationSchemes != null && policy.AuthenticationSchemes.Count > 0)
            {
                ClaimsPrincipal principal = null;

                foreach (var scheme in policy.AuthenticationSchemes)
                {
                    var result = await coreContext.Authentication.AuthenticateAsync(scheme);

                    if (result != null)
                    {
                        principal = MergeUserPrincipal(principal, result);
                    }
                }

                if (principal == null)
                {
                    principal = new ClaimsPrincipal(new ClaimsIdentity());
                }

                coreContext.User = principal;
            }

            if (IsAnonymousAllowed(context.ViewModel))
            {
                return;
            }

            var authService = coreContext.RequestServices.GetRequiredService<IAuthorizationService>();

            if (!await authService.AuthorizeAsync(coreContext.User, context, policy))
            {
                await HandleUnauthorizedRequest(coreContext, policy);
            }
        }

        private Task<AuthorizationPolicy> GetAuthorizationPolicy(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            var policyProvider = GetPolicyProvider(context);
            var authorizeData = Enumerable.Empty<IAuthorizeData>();

            if (actionInfo != null)
            {
                authorizeData = GetAuthorizeData(actionInfo);
            }
            else if (context.ViewModel != null)
            {
                authorizeData = GetAuthorizeData(context.ViewModel.GetType());
            }

            return AuthorizationPolicy.CombineAsync(policyProvider, authorizeData);
        }

        private IAuthorizationPolicyProvider GetPolicyProvider(IDotvvmRequestContext context)
            => context.GetAspNetCoreContext().RequestServices.GetRequiredService<IAuthorizationPolicyProvider>();

        private IEnumerable<IAuthorizeData> GetAuthorizeData(Type viewModelType)
            => viewModelType.GetTypeInfo().GetCustomAttributes().OfType<IAuthorizeData>().Select(WrapInAuthorizeAttribute);

        private IEnumerable<IAuthorizeData> GetAuthorizeData(ActionInfo actionInfo)
            => actionInfo?.Binding?.ActionFilters != null ? actionInfo.Binding.ActionFilters.OfType<IAuthorizeData>().Select(WrapInAuthorizeAttribute) : Enumerable.Empty<IAuthorizeData>();

        private bool IsAnonymousAllowed(object viewModel)
            => viewModel != null && isAnonymousAllowedCache.GetOrAdd(viewModel.GetType(), t => t.GetTypeInfo().GetCustomAttributes().OfType<IAllowAnonymous>().Any());

        private async Task HandleUnauthorizedRequest(HttpContext context, AuthorizationPolicy policy)
        {
            if (policy.AuthenticationSchemes != null && policy.AuthenticationSchemes.Count > 0)
            {
                foreach (var scheme in policy.AuthenticationSchemes)
                {
                    await context.Authentication.ChallengeAsync(scheme);
                }
            }
            else
            {
                await context.Authentication.ChallengeAsync();
            }

            throw new DotvvmInterruptRequestExecutionException("User unauthorized");
        }

        private ClaimsPrincipal MergeUserPrincipal(ClaimsPrincipal existingPrincipal, ClaimsPrincipal additionalPrincipal)
        {
            var result = new ClaimsPrincipal();

            if (additionalPrincipal != null)
            {
                result.AddIdentities(additionalPrincipal.Identities);
            }

            if (existingPrincipal != null)
            {
                result.AddIdentities(existingPrincipal.Identities.Where(i => i.IsAuthenticated || i.Claims.Any()));
            }

            return result;
        }

        private IAuthorizeData WrapInAuthorizeAttribute(IAuthorizeData authorizeDatum)
        {
            // TODO: remove when fixed (see https://github.com/aspnet/Security/commit/651815c282bfc594762346f4445afd9e6b48bb1e)

            return new Microsoft.AspNetCore.Authorization.AuthorizeAttribute {
                Policy = authorizeDatum.Policy,
                Roles = authorizeDatum.Roles,
                ActiveAuthenticationSchemes = authorizeDatum.ActiveAuthenticationSchemes
            };
        }
    }
}