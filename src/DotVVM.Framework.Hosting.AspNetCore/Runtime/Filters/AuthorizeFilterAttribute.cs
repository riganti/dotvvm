using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
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
        protected override void OnViewModelCreated(IDotvvmRequestContext context)
        {
            Authorize(context, null);
            base.OnViewModelCreated(context);
        }

        /// <inheritdoc />
        protected override void OnCommandExecuting(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            Authorize(context, actionInfo);
            base.OnCommandExecuting(context, actionInfo);
        }

        private void Authorize(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            var policy = GetAuthorizationPolicy(context, actionInfo);

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
                    var result = coreContext.Authentication.AuthenticateAsync(scheme).Result;

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

            if (!authService.AuthorizeAsync(coreContext.User, context, policy).Result)
            {
                HandleUnauthorizedRequest(coreContext, policy);
            }
        }

        private AuthorizationPolicy GetAuthorizationPolicy(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            // TODO: async action filters

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

            return AuthorizationPolicy.CombineAsync(policyProvider, authorizeData).Result;
        }

        private IAuthorizationPolicyProvider GetPolicyProvider(IDotvvmRequestContext context)
            => context.GetAspNetCoreContext().RequestServices.GetRequiredService<IAuthorizationPolicyProvider>();

        private IEnumerable<IAuthorizeData> GetAuthorizeData(Type viewModelType)
            => viewModelType.GetTypeInfo().GetCustomAttributes().OfType<IAuthorizeData>().Select(WrapInAuthorizeAttribute);

        private IEnumerable<IAuthorizeData> GetAuthorizeData(ActionInfo actionInfo)
            => actionInfo?.Binding?.ActionFilters != null ? actionInfo.Binding.ActionFilters.OfType<IAuthorizeData>().Select(WrapInAuthorizeAttribute) : Enumerable.Empty<IAuthorizeData>();

        private bool IsAnonymousAllowed(object viewModel)
            => viewModel != null && isAnonymousAllowedCache.GetOrAdd(viewModel.GetType(), t => t.GetTypeInfo().GetCustomAttributes().OfType<IAllowAnonymous>().Any());

        private void HandleUnauthorizedRequest(HttpContext context, AuthorizationPolicy policy)
        {
            if (policy.AuthenticationSchemes != null && policy.AuthenticationSchemes.Count > 0)
            {
                foreach (var scheme in policy.AuthenticationSchemes)
                {
                    context.Authentication.ChallengeAsync(scheme).Wait();
                }
            }
            else
            {
                context.Authentication.ChallengeAsync().Wait();
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