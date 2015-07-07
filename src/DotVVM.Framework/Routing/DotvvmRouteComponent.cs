using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotVVM.Framework.Routing
{
    /// <summary>
    /// Represents the route in the format "prefix{parameter}suffix". Only the prefix or parameter are required.
    /// </summary>
    public class DotvvmRouteComponent
    {

        private static readonly Regex urlPartRegex = new Regex(@"^([^{}]*)?(\{[a-zA-Z_][a-zA-Z0-9_]*\})?([^{}]*)?$");


        public string Prefix { get; private set; }

        public string Suffix { get; private set; }

        public string ParameterName { get; private set; }

        public bool HasParameter
        {
            get { return !string.IsNullOrEmpty(ParameterName); }
        }

        public bool HasPrefix
        {
            get { return !string.IsNullOrEmpty(Prefix); }
        }

        public bool HasSuffix
        {
            get { return !string.IsNullOrEmpty(Suffix); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRouteComponent"/> class.
        /// </summary>
        public DotvvmRouteComponent(string urlPart)
        {
            var match = urlPartRegex.Match(urlPart);

            if (!match.Success)
            {
                throw new ArgumentException(string.Format("The route part '{0}' contains invalid characters!", urlPart));
            }

            Prefix = match.Groups[1].Value;
            ParameterName = match.Groups[2].Value.TrimStart('{').TrimEnd('}');
            Suffix = match.Groups[3].Value;
        }


        /// <summary>
        /// Determines whether the specified p is match.
        /// </summary>
        public bool IsMatch(string urlPart, out string parameterValue)
        {
            if (HasPrefix)
            {
                if (!urlPart.StartsWith(Prefix))
                {
                    parameterValue = null;
                    return false;
                }
                urlPart = urlPart.Substring(Prefix.Length);
            }

            if (HasSuffix)
            {
                if (!urlPart.EndsWith(Suffix))
                {
                    parameterValue = null;
                    return false;
                }

                urlPart = urlPart.Substring(0, urlPart.Length - Suffix.Length);
            }

            if (HasParameter)
            {
                // the rest of the URL part is the parameter value
                parameterValue = urlPart;
                return true;
            }
            
            if (!HasParameter && string.IsNullOrEmpty(urlPart))
            {
                // no parameter and the URL part is empty
                parameterValue = null;
                return true;
            }

            // the URL part is not empty, no match
            parameterValue = null;
            return false;
        }

        /// <summary>
        /// Builds the URL.
        /// </summary>
        public void BuildUrl(StringBuilder stringBuilder, Dictionary<string, object> values)
        {
            if (HasPrefix)
            {
                stringBuilder.Append(Prefix);
            }
            if (HasParameter && values.ContainsKey(ParameterName))
            {
                stringBuilder.Append(values[ParameterName]);
            }
            if (HasSuffix)
            {
                stringBuilder.Append(Suffix);
            }
        }
    }
}
