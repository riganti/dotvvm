using System.CommandLine.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace System.CommandLine
{
    public static class CommandLineExtensions
    {
        public const string VerboseAlias = "--verbose";

        public static ILoggerFactory Factory = new NullLoggerFactory();

        public static CommandLineBuilder UseLogging(this CommandLineBuilder builder)
        {
            builder.AddGlobalOption(new Option<bool>(
                aliases: new [] {"-v", VerboseAlias},
                description: "Print more verbose output"));
            return builder.UseMiddleware(async (c, next) =>
                {
                    var logLevel = c.ParseResult.ValueForOption<bool>(VerboseAlias)
                        ? LogLevel.Debug
                        : LogLevel.Information;
                    Factory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(logLevel));
                    var loggerName = $"{c.ParseResult.CommandResult.Command.Name}";
                    c.BindingContext.AddService(_ => Factory.CreateLogger(loggerName));
                    await next(c);
                    Factory.Dispose();
                });
        }
    }
}
