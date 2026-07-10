using System;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Security
{
    /// <summary>
    /// Implements synchronizer token pattern for CSRF protection.
    /// <para>The token is generated based on Session ID (random 256-bit value persisted in cookie). 
    /// The token is bound to the current User identity (user name, if authenticated) so authentication changes invalidate old tokens.</para>
    /// <para>Value of stored Session ID and the token itself is encrypted and signed.</para>
    /// </summary>
    public class DefaultCsrfProtector : ICsrfProtector
    {
        private const int SID_LENGTH = 32; // 256-bit identifier
        private const string PURPOSE_SID = "DotVVM.Framework.Security.DefaultCsrfProtector.SID"; // Key derivation label for protecting SID
        private const string PURPOSE_TOKEN = "DotVVM.Framework.Security.DefaultCsrfProtector.Token"; // Key derivation label for protecting token

        private IDataProtectionProvider protectionProvider;
        private readonly ICookieManager cookieManager;

        public DefaultCsrfProtector(IDataProtectionProvider protectionProvider, ICookieManager cookieManager)
        {
            this.protectionProvider = protectionProvider;
            this.cookieManager = cookieManager;
        }

        public string GenerateToken(IDotvvmRequestContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Get SID
            var sid = this.GetOrCreateSessionId(context);

            // Construct protector with purposes
            var protector = this.protectionProvider.CreateProtector(PURPOSE_TOKEN);

            // Get token
            var tokenData = protector.Protect([ ..sid, ..StringUtils.Utf8.GetBytes(ProtectionHelpers.GetUserIdentity(context)) ]);

            // Return encoded token
            return Convert.ToBase64String(tokenData);
        }

        public void VerifyToken(IDotvvmRequestContext context, string token)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(token)) throw new CorruptedCsrfTokenException("CSRF protection token is missing.");

            // Construct protector with purposes
            var protector = this.protectionProvider.CreateProtector(PURPOSE_TOKEN);

            // Get token
            byte[] tokenData;
            try
            {
                var protectedTokenData = Convert.FromBase64String(token);
                tokenData = protector.Unprotect(protectedTokenData);
            }
            catch (Exception ex)
            {
                // Incorrect Base64 formatting of crypto protection error
                throw new CorruptedCsrfTokenException("CSRF protection token is invalid.", ex);
            }

            // Get SID from cookie and compare with token one
            var cookieSid = this.GetOrCreateSessionId(context, canGenerate: false); // should not generate new token
            byte[] expectedTokenData = [ ..cookieSid, ..StringUtils.Utf8.GetBytes(ProtectionHelpers.GetUserIdentity(context)) ];
            if (!CryptographicOperations.FixedTimeEquals(expectedTokenData, tokenData))
            {
                throw new CorruptedCsrfTokenException("CSRF protection token is invalid (SID or user does not match).");
            }
        }

        private byte[] GetOrCreateSessionId(IDotvvmRequestContext context, bool canGenerate = true)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var originalHttpContext = context.GetAspNetCoreContext();
            var sessionIdCookieName = GetSessionIdCookieName(context);
            if (string.IsNullOrWhiteSpace(sessionIdCookieName)) throw new FormatException("Configured SessionIdCookieName is missing or empty.");
            if (context.HttpContext.Request.IsHttps)
                sessionIdCookieName = "__Host-" + sessionIdCookieName;

            // Construct protector with purposes
            var protector = this.protectionProvider.CreateProtector(PURPOSE_SID);

            // Get cookie value
            var sidCookieValue = cookieManager.GetRequestCookie(originalHttpContext, sessionIdCookieName);

            if (!string.IsNullOrWhiteSpace(sidCookieValue))
            {
                // Try to read from cookie
                try
                {
                    var protectedSid = Convert.FromBase64String(sidCookieValue);
                    var sid = protector.Unprotect(protectedSid);
                    return sid;
                }
                catch (Exception ex)
                {
                    // Incorrect Base64 formatting of crypto protection error
                    // Generate new one or throw error if can't
                    if (!canGenerate)
                        throw new CorruptedCsrfTokenException("Value of the SessionID cookie is corrupted or has been tampered with.", ex);
                    // else suppress error and generate new SID
                }
            }

            var canUseSameSite = !context.Configuration.Security.FrameOptionsCrossOrigin.IsEnabledForAnyRoute();

            // No SID - generate and protect new one

            if(canGenerate)
            {
                var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
                var sid = new byte[SID_LENGTH];
                rng.GetBytes(sid);
                var protectedSid = protector.Protect(sid);

                // Save to cookie
                sidCookieValue = Convert.ToBase64String(protectedSid);
                cookieManager.AppendResponseCookie(
                    originalHttpContext,
                    sessionIdCookieName,                                // Configured cookie name
                    sidCookieValue,                                     // Base64-encoded SID value
                    new CookieOptions
                    {
                        HttpOnly = true,                                // Don't allow client script access
                        Secure = context.HttpContext.Request.IsHttps,   // If request goes trough HTTPS, mark as secure only
                        SameSite = canUseSameSite ? SameSiteMode.Lax : SameSiteMode.None
                    });

                // Return newly generated SID
                return sid;
            }
            else
            {
                throw new CorruptedCsrfTokenException("SessionID cookie is missing, so can't verify CSRF token.");
            }
        }

        private string GetSessionIdCookieName(IDotvvmRequestContext context)
        {
            var domain = context.HttpContext.Request.Url.Host;
            if (context.HttpContext.Request.Url.Port != (context.HttpContext.Request.IsHttps ? 443 : 80))
            {
                domain += "-" + context.HttpContext.Request.Url.Port;
            }
            return string.Format(context.Configuration.Security.SessionIdCookieName, domain);
        }
    }
}
