using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DotVVM.Framework.Configuration
{
    /// <summary>
    /// Contains the encryption keys for ViewModel protection.
    /// </summary>
    public class DotvvmSecurityConfiguration
    {
        /// <summary>
        /// Gets or sets name of HTTP cookie used for Session ID
        /// </summary>
        [JsonProperty("sessionIdCookieName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue("dotvvm_sid_{0}")]
        public string SessionIdCookieName
        {
            get => sessionIdCookieName;
            set { ThrowIfFrozen(); sessionIdCookieName = value; }
        }
        private string sessionIdCookieName = "dotvvm_sid_{0}";

        /// <summary>
        /// When enabled, uses `X-Frame-Options: SAMEORIGIN` instead of DENY
        /// </summary>
        [JsonProperty("frameOptionsSameOrigin", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmFeatureFlag FrameOptionsSameOrigin { get; } = new();

        /// <summary>
        /// When enabled, does not add `X-Frame-Options: DENY` header. Enabling will force DotVVM to use SameSite=None on the session cookie
        /// </summary>
        [JsonProperty("frameOptionsCrossOrigin", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmFeatureFlag FrameOptionsCrossOrigin { get; } = new();

        /// <summary>
        /// When enabled, adds the `X-XSS-Protection: 1; mode=block` header, which enables some basic XSS filtering in browsers. This is enabled by default.
        /// </summary>
        [JsonProperty("xssProtectionHeader", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmFeatureFlag XssProtectionHeader { get; } = new() { Enabled = true };

        /// <summary>
        /// When enabled, adds the `X-Content-Type-Options: nosniff` header, which prevents browsers from incorrectly detecting non-scripts as scripts. This is enabled by default.
        /// </summary>
        [JsonProperty("contentTypeOptionsHeader", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmFeatureFlag ContentTypeOptionsHeader { get; } = new() { Enabled = true };

        /// <summary>
        /// Verifies Sec-Fetch headers on the GET request coming to dothtml pages. The request must have `Sec-Fetch-Dest: document` or `Sec-Fetch-Site: same-origin` if the request is for SPA. If the FrameOptionsSameOrigin is enabled, DotVVM will also allow `Sec-Fetch-Dest: document` and if FrameOptionsSameOrigin is enabled, DotVVM will also allow iframe from an cross-site request. This protects agains cross-site page scraping. Also prevents potential XSS bug to scrape the non-SPA pages.
        /// </summary>
        [JsonProperty("verifySecFetchForPages", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmFeatureFlag VerifySecFetchForPages { get; } = new() { Enabled = true };

        /// <summary>
        /// Verifies Sec-Fetch headers on the POST request executing staticCommands and commands. The request must have `Sec-Fetch-Site: same-origin`. This protects again cross-site malicious requests even if SameSite cookies and CSRF tokens would fail. It also prevents websites on a subdomain to perform postbacks.
        /// </summary>
        [JsonProperty("verifySecFetchForCommands", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmFeatureFlag VerifySecFetchForCommands { get; } = new() { Enabled = true };

        /// <summary>
        /// Requires that requests to dotvvm pages always have the Sec-Fetch-* headers. This may offer a slight protection against server-side request forgery attacks and against attacks exploiting obsolete web browsers (MS IE and Apple IE)
        /// </summary>
        [JsonProperty("requireSecFetchHeaders", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmFeatureFlag RequireSecFetchHeaders { get; } = new();

        private bool isFrozen = false;
        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmSecurityConfiguration));
        }
        public void Freeze()
        {
            this.isFrozen = true;
            this.FrameOptionsSameOrigin.Freeze();
        }
    }
}
