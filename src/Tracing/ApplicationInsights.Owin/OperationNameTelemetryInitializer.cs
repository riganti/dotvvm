using System.Web;
using DotVVM.Framework.Hosting;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;

namespace DotVVM.Tracing.ApplicationInsights.Owin;

public class OperationNameTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        var url = HttpContext.Current?.GetOwinContext()?.GetDotvvmContext()?.Route?.Url;
        if (string.IsNullOrWhiteSpace(url) == false && telemetry is RequestTelemetry)
        {
            var method = HttpContext.Current.Request.HttpMethod;
            var operationName = $"{method} /{url}";

            var requestTelemetry = telemetry as RequestTelemetry;
            requestTelemetry.Name = operationName;
            requestTelemetry.Context.Operation.Name = operationName;
        }
    }
}
