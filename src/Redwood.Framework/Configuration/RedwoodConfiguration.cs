using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Redwood.Framework.Routing;
using Redwood.Framework.Parser;
using Redwood.Framework.ResourceManagement;

namespace Redwood.Framework.Configuration
{
    public class RedwoodConfiguration
    {
        public const string RedwoodControlTagPrefix = "rw";
        public static readonly ImmutableArray<string> SupportedGlobalizationCultures = ImmutableArray.Create(new string[] { "af-ZA", "af", "am-ET", "am", "ar-AE", "ar-BH", "ar-DZ", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA", "ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-YE", "ar", "arn-CL", "arn", "as-IN", "as", "az-Cyrl-AZ", "az-Cyrl", "az-Latn-AZ", "az-Latn", "az", "ba-RU", "ba", "be-BY", "be", "bg-BG", "bg", "bn-BD", "bn-IN", "bn", "bo-CN", "bo", "br-FR", "br", "bs-Cyrl-BA", "bs-Cyrl", "bs-Latn-BA", "bs-Latn", "bs", "ca-ES", "ca", "co-FR", "co", "cs-CZ", "cs", "cy-GB", "cy", "da-DK", "da", "de-AT", "de-CH", "de-DE", "de-LI", "de-LU", "de", "dsb-DE", "dsb", "dv-MV", "dv", "el-GR", "el", "en-029", "en-AU", "en-BZ", "en-CA", "en-GB", "en-IE", "en-IN", "en-JM", "en-MY", "en-NZ", "en-PH", "en-SG", "en-TT", "en-US", "en-ZA", "en-ZW", "es-AR", "es-BO", "es-CL", "es-CO", "es-CR", "es-DO", "es-EC", "es-ES", "es-GT", "es-HN", "es-MX", "es-NI", "es-PA", "es-PE", "es-PR", "es-PY", "es-SV", "es-US", "es-UY", "es-VE", "es", "et-EE", "et", "eu-ES", "eu", "fa-IR", "fa", "fi-FI", "fi", "fil-PH", "fil", "fo-FO", "fo", "fr-BE", "fr-CA", "fr-CH", "fr-FR", "fr-LU", "fr-MC", "fr", "fy-NL", "fy", "ga-IE", "ga", "gd-GB", "gd", "gl-ES", "gl", "gsw-FR", "gsw", "gu-IN", "gu", "ha-Latn-NG", "ha-Latn", "ha", "he-IL", "he", "hi-IN", "hi", "hr-BA", "hr-HR", "hr", "hsb-DE", "hsb", "hu-HU", "hu", "hy-AM", "hy", "id-ID", "id", "ig-NG", "ig", "ii-CN", "ii", "is-IS", "is", "it-CH", "it-IT", "it", "iu-Cans-CA", "iu-Cans", "iu-Latn-CA", "iu-Latn", "iu", "ja-JP", "ja", "ka-GE", "ka", "kk-KZ", "kk", "kl-GL", "kl", "km-KH", "km", "kn-IN", "kn", "ko-KR", "ko", "kok-IN", "kok", "ky-KG", "ky", "lb-LU", "lb", "lo-LA", "lo", "lt-LT", "lt", "lv-LV", "lv", "mi-NZ", "mi", "mk-MK", "mk", "ml-IN", "ml", "mn-Cyrl", "mn-MN", "mn-Mong-CN", "mn-Mong", "mn", "moh-CA", "moh", "mr-IN", "mr", "ms-BN", "ms-MY", "ms", "mt-MT", "mt", "nb-NO", "nb", "ne-NP", "ne", "nl-BE", "nl-NL", "nl", "nn-NO", "nn", "no", "nso-ZA", "nso", "oc-FR", "oc", "or-IN", "or", "pa-IN", "pa", "pl-PL", "pl", "prs-AF", "prs", "ps-AF", "ps", "pt-BR", "pt-PT", "pt", "qut-GT", "qut", "quz-BO", "quz-EC", "quz-PE", "quz", "rm-CH", "rm", "ro-RO", "ro", "ru-RU", "ru", "rw-RW", "rw", "sa-IN", "sa", "sah-RU", "sah", "se-FI", "se-NO", "se-SE", "se", "si-LK", "si", "sk-SK", "sk", "sl-SI", "sl", "sma-NO", "sma-SE", "sma", "smj-NO", "smj-SE", "smj", "smn-FI", "smn", "sms-FI", "sms", "sq-AL", "sq", "sr-Cyrl-BA", "sr-Cyrl-CS", "sr-Cyrl-ME", "sr-Cyrl-RS", "sr-Cyrl", "sr-Latn-BA", "sr-Latn-CS", "sr-Latn-ME", "sr-Latn-RS", "sr-Latn", "sr", "sv-FI", "sv-SE", "sv", "sw-KE", "sw", "syr-SY", "syr", "ta-IN", "ta", "te-IN", "te", "tg-Cyrl-TJ", "tg-Cyrl", "tg", "th-TH", "th", "tk-TM", "tk", "tn-ZA", "tn", "tr-TR", "tr", "tt-RU", "tt", "tzm-Latn-DZ", "tzm-Latn", "tzm", "ug-CN", "ug", "uk-UA", "uk", "ur-PK", "ur", "uz-Cyrl-UZ", "uz-Cyrl", "uz-Latn-UZ", "uz-Latn", "uz", "vi-VN", "vi", "wo-SN", "wo", "xh-ZA", "xh", "yo-NG", "yo", "zh-CHS", "zh-CHT", "zh-CN", "zh-Hans", "zh-Hant", "zh-HK", "zh-MO", "zh-SG", "zh-TW", "zh", "zu-ZA", "zu" });

        /// <summary>
        /// Gets or sets the application physical path.
        /// </summary>
        [JsonIgnore]
        public string ApplicationPhysicalPath { get; set; }

        /// <summary>
        /// Gets the settings of the markup.
        /// </summary>
        [JsonProperty("markup")]
        public RedwoodMarkupConfiguration Markup { get; private set; }

        /// <summary>
        /// Gets the route table.
        /// </summary>
        [JsonIgnore()]
        public RedwoodRouteTable RouteTable { get; private set; }

        /// <summary>
        /// Gets the configuration of resources.
        /// </summary>
        [JsonProperty("resources")]
        [JsonConverter(typeof(ResourceRepositoryJsonConverter))]
        public RedwoodResourceRepository Resources { get; private set; }

        /// <summary>
        /// Gets the security configuration.
        /// </summary>
        [JsonProperty("security")]
        public RedwoodSecurityConfiguration Security { get; private set; }

        /// <summary>
        /// Gets the runtime configuration.
        /// </summary>
        [JsonProperty("runtime")]
        public RedwoodRuntimeConfiguration Runtime { get; private set; }

        /// <summary>
        /// Gets or sets the default culture.
        /// </summary>
        [JsonProperty("defaultCulture")]
        public string DefaultCulture { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodConfiguration"/> class.
        /// </summary>
        internal RedwoodConfiguration()
        {
            DefaultCulture = Thread.CurrentThread.CurrentCulture.Name;
            Markup = new RedwoodMarkupConfiguration();
            RouteTable = new RedwoodRouteTable(this);
            Resources = new RedwoodResourceRepository();
            Security = new RedwoodSecurityConfiguration();
            Runtime = new RedwoodRuntimeConfiguration();
        }

        /// <summary>
        /// Creates the default configuration.
        /// </summary>
        public static RedwoodConfiguration CreateDefault()
        {
            var configuration = new RedwoodConfiguration();

            configuration.Markup.Controls.AddRange(new[]
            {
                new RedwoodControlConfiguration() { TagPrefix = "rw", Namespace = "Redwood.Framework.Controls", Assembly = "Redwood.Framework" },
                new RedwoodControlConfiguration() { TagPrefix = "bootstrap", Namespace = "Redwood.Framework.Controls.Bootstrap", Assembly = "Redwood.Framework" },
            });

            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.JQueryResourceName,
                    Url = "/Scripts/jquery-2.1.1.min.js",
                    CdnUrl = "https://code.jquery.com/jquery-2.1.1.min.js",
                    GlobalObjectName = "$"
                });
            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.KnockoutJSResourceName,
                    Url = "/Scripts/knockout-3.2.0.js",
                    GlobalObjectName = "ko"
                });
            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.KnockoutMapperResourceName,
                    Url = "/Scripts/knockout.mapper.js",
                    GlobalObjectName = "ko.mapper",
                    Dependencies = new[] { Constants.KnockoutJSResourceName }
                });
            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.RedwoodResourceName,
                    Url = "/Scripts/Redwood.js",
                    GlobalObjectName = "redwood",
                    Dependencies = new[] { Constants.KnockoutJSResourceName, Constants.KnockoutMapperResourceName }
                });
            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.BootstrapResourceName,
                    Url = "/Scripts/bootstrap.min.js",
                    CdnUrl = "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/js/bootstrap.min.js",
                    GlobalObjectName = "typeof $().emulateTransitionEnd == 'function'",
                    Dependencies = new[] { Constants.BootstrapCssResourceName, Constants.JQueryResourceName }
                });
            configuration.Resources.Register(
                new StylesheetResource()
                {
                    Name = Constants.BootstrapCssResourceName,
                    Url = "/Content/bootstrap/bootstrap.min.css"
                });

            RegisterGlobalizeResources(configuration);

            return configuration;
        }

        private static void RegisterGlobalizeResources(RedwoodConfiguration configuration)
        {
            configuration.Resources.Register(new ScriptResource()
            {
                Name = Constants.GlobalizeResourceName,
                Url = "Redwood.Framework.Resources.Scripts.Globalize.globalize.js",
                EmbeddedResourceAssembly = typeof(RedwoodConfiguration).Assembly.GetName().Name
            });

            foreach (var culture in SupportedGlobalizationCultures)
            {
                configuration.Resources.Register(new ScriptResource()
                {
                    Name = string.Format(Constants.GlobalizeCultureResourceName, culture),
                    Url = "Redwood.Framework.Resources.Scripts.Globalize.cultures.{0}.globalize.js",
                    EmbeddedResourceAssembly = typeof(RedwoodConfiguration).Assembly.GetName().Name,
                    Dependencies = new[] { Constants.GlobalizeResourceName }
                });    
            }
        }

    }
}
