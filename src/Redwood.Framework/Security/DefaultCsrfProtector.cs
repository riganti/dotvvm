﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Security {
    /// <summary>
    /// Implements synchronizer token pattern for CSRF protection.
    /// <para>The token is generated based on Session ID (random 256-bit value persisted in cookie), 
    /// Request identity (full URI) and User identity (user name, if authenticated).</para>
    /// <para>Value of stored Session ID and the token itself is encrypted and signed.</para>
    /// </summary>
    public class DefaultCsrfProtector : ICsrfProtector {
        private const int SID_LENGTH = 32; // 256-bit identifier
        private const string KDF_LABEL_SID = "Redwood.Framework.Security.DefaultCsrfProtector.SID"; // Key derivation label for protecting SID
        private const string KDF_LABEL_TOKEN = "Redwood.Framework.Security.DefaultCsrfProtector.Token"; // Key derivation label for protecting token

        public string GenerateToken(RedwoodRequestContext context) {
            if (context == null) throw new ArgumentNullException("context");

            // Get SID
            var sid = this.GetOrCreateSessionId(context);

            // Get application key helper
            var keyHelper = new ApplicationKeyHelper(context.Configuration.Security);

            // Get token
            var userIdentity = context.OwinContext.Request.User.Identity.IsAuthenticated ? context.OwinContext.Request.User.Identity.Name : null;
            var requestIdentity = context.OwinContext.Request.Uri.ToString();
            var tokenData = keyHelper.ProtectData(sid, KDF_LABEL_TOKEN, userIdentity, requestIdentity);

            // Return encoded token
            return Convert.ToBase64String(tokenData);
        }

        public void VerifyToken(RedwoodRequestContext context, string token) {
            if (context == null) throw new ArgumentNullException("context");
            if (string.IsNullOrWhiteSpace(token)) throw new SecurityException("CSRF protection token is missing.");

            // Get expected token value
            var expectedToken = this.GenerateToken(context);

            // Throw exception if does not match supplied one
            if (!token.Equals(expectedToken, StringComparison.Ordinal)) throw new SecurityException("CSRF protection token is invalid.");
        }

        private byte[] GetOrCreateSessionId(RedwoodRequestContext context) {
            if (context == null) throw new ArgumentNullException("context");
            if (string.IsNullOrWhiteSpace(context.Configuration.Security.SessionIdCookieName)) throw new FormatException("Configured SessionIdCookieName is missing or empty.");

            // Get cookie manager
            var mgr = new ChunkingCookieManager(); // TODO: Make this configurable

            // Get application key helper
            var keyHelper = new ApplicationKeyHelper(context.Configuration.Security);

            // Get cookie value
            var sidCookieValue = mgr.GetRequestCookie(context.OwinContext, context.Configuration.Security.SessionIdCookieName);

            if (string.IsNullOrWhiteSpace(sidCookieValue)) {
                // No SID - generate and protect new one
                var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
                var sid = new byte[SID_LENGTH];
                rng.GetBytes(sid);
                sid = keyHelper.ProtectData(sid, KDF_LABEL_SID);

                // Save to cookie
                mgr.AppendResponseCookie(
                    context.OwinContext,
                    context.Configuration.Security.SessionIdCookieName, // Configured cookie name
                    Convert.ToBase64String(sid),                        // Base64-encoded SID value
                    new Microsoft.Owin.CookieOptions {
                        HttpOnly = true,                                // Don't allow client script access
                        Secure = context.OwinContext.Request.IsSecure   // If request goes trough HTTPS, mark as secure only
                    });

                // Return newly generated SID
                return sid;
            }
            else {
                // Try to read from cookie
                try {
                    var sid = Convert.FromBase64String(sidCookieValue);
                    sid = keyHelper.UnprotectData(sid, KDF_LABEL_SID);
                    return sid;
                }
                catch (Exception ex) {
                    throw new SecurityException("Value of the SessionID cookie is corrupted or has been tampered with.", ex);
                }
            }
        }

    }
}
