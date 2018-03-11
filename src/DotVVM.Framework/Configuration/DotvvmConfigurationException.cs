using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using DotVVM.Framework.Routing;

namespace DotVVM.Framework.Configuration
{
    internal class DotvvmConfigurationException : Exception
    {
        internal DotvvmConfigurationException()
        {
        }

        internal DotvvmConfigurationException(string message) : base(message)
        {
        }

        internal DotvvmConfigurationException(List<DotvvmConfigurationAssertResult<RouteBase>> routes, List<DotvvmConfigurationAssertResult<DotvvmControlConfiguration>> controls) : base(message: BuildMessage(routes, controls))
        {


        }

        private static string BuildMessage(List<DotvvmConfigurationAssertResult<RouteBase>> routes, List<DotvvmConfigurationAssertResult<DotvvmControlConfiguration>> controls)
        {

            var sb = new StringBuilder();
            sb.AppendLine("DotvvmConfiguration contains incorrect registrations.");
            if (routes != null && routes.Any())
            {
                var routeNameMissing = false;
                sb.AppendLine("Invalid route registrations: ");
                foreach (var routeBase in routes)
                {

                    if (routeBase.Reason == DotvvmConfigurationAssertReason.MissingRouteName)
                    {
                        routeNameMissing = true;
                    }
                    if (routeBase.Reason == DotvvmConfigurationAssertReason.MissingFile)
                    {
                        sb.Append("Route '");
                        sb.Append(routeBase.Value.RouteName);
                        sb.Append("' has missing file '");
                        sb.Append(routeBase.Value.VirtualPath);
                        sb.Append("'.");
                        sb.AppendLine();
                    }
                }

                if (routeNameMissing)
                {
                    sb.AppendLine("One ore more routes have missing name!");
                }
                sb.AppendLine();
            }
            if (controls != null && controls.Any())
            {
                sb.AppendLine("Invalid control registrations: ");
                var missingTagNameAndTagPrefix = false;
                foreach (var control in controls)
                {
                    if (control.Reason == DotvvmConfigurationAssertReason.MissingControlTagPrefix)
                    {
                        if (!string.IsNullOrWhiteSpace(control.Value.TagName))
                        {
                            sb.Append("Control with tag name '");
                            sb.Append(control.Value.TagName);
                            sb.Append("' has missing tag prefix.");
                        }
                        else
                        {

                            missingTagNameAndTagPrefix = true;
                        }
                    }
                    if (control.Reason == DotvvmConfigurationAssertReason.MissingControlTagName)
                    {
                        sb.Append("Control with tag prefix '");
                        sb.Append(control.Value.TagPrefix);
                        sb.Append("' and src'");
                        sb.Append(control.Value.Src);
                        sb.Append("' has missing tag name.");
                    }
                    if (control.Reason == DotvvmConfigurationAssertReason.MissingFile)
                    {
                        sb.Append("Control '");
                        sb.Append(control.Value.TagPrefix);
                        sb.Append(":");
                        sb.Append(control.Value.TagName);
                        sb.Append("' has missing file '");
                        sb.Append(control.Value.Src);
                        sb.Append("'.");
                    }


                }

                if (missingTagNameAndTagPrefix)
                {
                    sb.AppendLine("One ore more controls have missing tag prefix!");
                }
            }
            return sb.ToString();


        }

        protected DotvvmConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
