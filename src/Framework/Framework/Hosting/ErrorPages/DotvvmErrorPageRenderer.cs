using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class DotvvmErrorPageRenderer
    {
        private readonly IServiceProvider serviceProvider;

        public ErrorFormatter? Formatter { get; set; }

        public DotvvmErrorPageRenderer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Renders the error response.
        /// </summary>
        public Task RenderErrorResponse(IHttpContext context, Exception error)
        {
            try
            {
                context.Response.ContentType = "text/html; charset=utf-8";

                var text = (Formatter ?? (Formatter = CreateDefaultWithDemystifier()))
                    .ErrorHtml(error, context);
                return context.Response.WriteAsync(text);
            }
            catch (Exception exc)
            {
                return RenderFallbackMessage(context, error, exc);
            }
        }

        private static async Task RenderFallbackMessage(IHttpContext context, Exception error, Exception exc)
        {
            try
            {
                context.Response.ContentType = "text/plain";
                using (var writer = new StreamWriter(context.Response.Body))
                {
                    await writer.WriteLineAsync("Error in DotVVM Application:");
                    await writer.WriteLineAsync(error.ToString());
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync("Error occurred while displaying the error page. This is internal error and should not happen, please report it:");
                    await writer.WriteLineAsync(exc.ToString());
                }
            }
            catch { }
            throw new Exception("Error occurred inside DotVVM error handler, this is internal error and should not happen; \n Original error:" + error.ToString(), exc);
        }

        private ErrorFormatter CreateDefaultWithDemystifier()
        {
            var errorPageExtensions = serviceProvider.GetServices<IErrorPageExtension>();
            var errorFormatter = ErrorFormatter.CreateDefault(errorPageExtensions);

            var insertPosition = errorFormatter.Formatters.Count > 0 ? 1 : 0;
            errorFormatter.Formatters.Insert(insertPosition, (e, o) =>
                new ExceptionSectionFormatter(LoadDemystifiedException(errorFormatter, e)));

            return errorFormatter;
        }

        private ExceptionModel LoadDemystifiedException(ErrorFormatter formatter, Exception exception)
        {
            return formatter.LoadException(exception,
                stackFrameGetter: ex => {
                    var rawStackTrace = new StackTrace(ex, true).GetFrames();
                    if (rawStackTrace == null) return null; // demystifier throws in these cases
                    try
                    {
                        return new EnhancedStackTrace(ex).GetFrames();
                    }
                    catch
                    {
                        return rawStackTrace;
                    }
                },
                methodFormatter: f => (f as EnhancedStackFrame)?.MethodInfo?.ToString());
        }

    }
}
