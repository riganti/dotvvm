using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace DotVVM.Framework.Routing
{
    public record LocalizedRouteUrl
    {
        /// <summary>
        /// Gets or sets the culture identifier. Allowed formats are language-REGION (e.g. en-US) or language (e.g. en).
        /// </summary>
        public string CultureIdentifier { get; }

        /// <summary>
        /// Get or sets the corresponding route URL.
        /// </summary>
        public string RouteUrl { get; }

        /// <summary>
        /// Represents a localized route URL.
        /// </summary>
        /// <param name="cultureIdentifier">Culture identifier. Allowed formats are language-REGION (e.g. en-US) or language (e.g. en)</param>
        /// <param name="routeUrl">Corresponding route URL for the culture.</param>
        public LocalizedRouteUrl(string cultureIdentifier, string routeUrl)
        {
            LocalizedDotvvmRoute.ValidateCultureName(cultureIdentifier);

            CultureIdentifier = cultureIdentifier;
            RouteUrl = routeUrl;
        }

    }
}
