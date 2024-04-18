using DotVVM.Framework.Hosting;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Tracing.ApplicationInsights.AspNetCore;

public class OperationNameTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _accessor;

    public OperationNameTelemetryInitializer(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        var url = _accessor.HttpContext?.GetDotvvmContext()?.Route?.Url;
        if (string.IsNullOrWhiteSpace(url) == false && telemetry is RequestTelemetry)
        {
            var method = _accessor.HttpContext.Request.Method;
            var operationName = $"{method} /{url}";

            var requestTelemetry = telemetry as RequestTelemetry;
            requestTelemetry.Name = operationName;
            requestTelemetry.Context.Operation.Name = operationName;
        }
    }
}
