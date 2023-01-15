using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;

namespace DotVVM.Framework.Hosting
{
    /// <summary> Class containing all metrics collected by DotVVM framework </summary>
    public static class DotvvmMetrics
    {
        public static readonly Meter Meter = new Meter("dotvvm");

        /// <summary> Labeled with success=true/false </summary>
        public static readonly ObservableCounter<long> ViewsCompiled =
            Meter.CreateObservableCounter<long>("view_compiled_total",
                description: "Number of dothtml pages that were compiled. It should reach a steady state shortly after startup.",
                observeValues:  () => {
                    return new [] {
                        new Measurement<long>(BareCounters.ViewsCompiledFailed, new KeyValuePair<string, object?>("success", false)),
                        new Measurement<long>(BareCounters.ViewsCompiledOk, new KeyValuePair<string, object?>("success", true))
                    };
                });
        public static readonly ObservableCounter<double> ViewsCompilationTime =
            Meter.CreateObservableCounter<double>("view_compilation_seconds_total",
                unit: "seconds", description: "CPU time spent in dothtml page compilation (in seconds).",
                observeValue: () => BareCounters.ViewsCompilationTime * ValueStopwatch.TimestampToSeconds
            );

        public static readonly ObservableCounter<long> DotvvmPropertyInitialized =
            Meter.CreateObservableCounter<long>("property_initialized_total",
                description: "Number of DotvvmProperties that were initialized.",
                observeValue: () => BareCounters.DotvvmPropertyInitialized
            );

        /// <summary> Labeled with binding_type=value/resource/... and is_lazy_init=true/false </summary>
        public static readonly Counter<long> BindingsInitialized =
            Meter.CreateCounter<long>("binding_initialized_total", description: "Number of bindings that were initialized.");

        /// <summary> Labeled with binding_type=value/resource/... </summary>
        public static readonly Counter<long> BindingsCompiled =
            Meter.CreateCounter<long>("binding_compiled_total", description: "Number of bindings that were compiled. It should reach a steady state shortly after startup.");

        /// <summary> Labeled by route=RouteName and request_type=GET/POST </summary>
        public static readonly Histogram<long> ViewModelSize =
            Meter.CreateHistogram<long>("viewmodel_size_bytes", unit: "bytes", description: "Size of the viewmodel JSON in bytes.");

        /// <summary> Labeled by route=RouteName and request_type=GET/POST </summary>
        public static readonly Histogram<double> ViewModelStringificationTime =
            Meter.CreateHistogram<double>("viewmodel_stringification_seconds", unit: "seconds", description: "Time it took to stringify the resulting JSON view model.");

        /// <summary> Labeled by route=RouteName and request_type=GET/POST </summary>
        public static readonly Histogram<double> ViewModelSerializationTime =
            Meter.CreateHistogram<double>("viewmodel_serialization_seconds", unit: "seconds", description: "Time it took to serialize view model to JSON objects.");

        /// <summary> Labeled by route=RouteName and lifecycle_type=TODO </summary>
        public static readonly Histogram<double> LifecycleInfocationDuration =
            Meter.CreateHistogram<double>("control_lifecycle_seconds", unit: "seconds", description: "Time it took to process a request on the specific route.");

        /// <summary> Labeled by route=RouteName, dothtml_file=filepath, request_type=GET/POST </summary>
        public static readonly Histogram<double> RequestDuration =
            Meter.CreateHistogram<double>("request_duration_seconds", unit: "seconds", description: "Time it took to process a request on the specific route.");

        /// <summary> Labeled by route=RouteName, dothtml_file=filepath, request_type=GET/POST </summary>
        public static readonly Counter<long> RequestsRejected =
            Meter.CreateCounter<long>("request_rejected_total", description: "Number of requests rejected (for security reasons) on the specific route.");

        /// <summary> Labeled by command="method invoked", result=Ok/Exception/UnhandledException </summary>
        public static readonly Histogram<double> StaticCommandInvocationDuration =
            Meter.CreateHistogram<double>("staticcommand_invocation_seconds", unit: "seconds", description: "Time it took to invoke the staticCommand method. Note that serialization overhead is not included, look at request_duration_seconds{request_type=\"staticCommand\"}.");

        /// <summary> Labeled by command={command: TheBinding()}, result=Ok/Exception/UnhandledException </summary>
        public static readonly Histogram<double> CommandInvocationDuration =
            Meter.CreateHistogram<double>("command_invocation_seconds", unit: "seconds", description: "Time it took to invoke the command method. Note that this does not include any of the overhead which is quite heavy for commands. Look at request_duration_seconds{request_type=\"command\"}.");

        public static readonly Histogram<long> ValidationErrorsReturned =
            Meter.CreateHistogram<long>("viewmodel_validation_errors_total", description: "Number of validation errors returned to the client.");

        public static readonly Histogram<double> ResourceServeDuration =
            Meter.CreateHistogram<double>("resource_serve_seconds", unit: "seconds", description: "Time it took to lookup and serve a resource.");

        public static readonly Counter<long> LazyCsrfTokenGenerated =
            Meter.CreateCounter<long>("lazy_csrf_token_created_total", description: "Number of lazy CSRF tokens created.");

        public static readonly Histogram<long> UploadedFileSize =
            Meter.CreateHistogram<long>("uploaded_file_bytes", unit: "bytes", description: "Total size of user-uploaded files");

        public static readonly Histogram<long> ReturnedFileSize =
            Meter.CreateHistogram<long>("returned_file_bytes", unit: "bytes", description: "Total size of returned files. Measured when the file is downloaded by user - if it's downloaded twice, it's counted twice; if it's not downloaded, it's not counted.");

        /// <summary> Labeled by route=RouteName </summary>
        public static readonly Counter<long> ViewModelCacheHit =
            Meter.CreateCounter<long>("viewmodel_cache_hit_total", description: "Number of requests with view model cache enabled which were successful");
        /// <summary> Labeled by route=RouteName </summary>
        public static readonly Counter<long> ViewModelCacheMiss =
            Meter.CreateCounter<long>("viewmodel_cache_miss_total", description: "Number of requests with view model cache enabled which were not successful");

        /// <summary> Labeled by route=RouteName </summary>
        public static readonly Counter<long> ViewModelCacheBytesLoaded =
            Meter.CreateCounter<long>("viewmodel_cache_loaded_bytes_total", "bytes", description: "Total number of bytes loaded from view model cache");


        public static double[]? TryGetRecommendedBuckets(Instrument instrument)
        {
            if (instrument.Meter != Meter)
                return null;

            if (instrument == ValidationErrorsReturned)
                return new double[] { 1, 2, 3, 5, 8, 13, 21 };

            var secStart = 1.0 / 128.0; // about 10ms, so that 1second is a boundary
            if (instrument == ResourceServeDuration)
                return ExponentialBuckets(secStart, 2, 0.5);

            if (instrument == ResourceServeDuration)
                return ExponentialBuckets(secStart, 2, 0.5);

            if (instrument == ResourceServeDuration)
                return ExponentialBuckets(secStart, 2, 1);

            if (instrument == RequestDuration || instrument == CommandInvocationDuration || instrument == StaticCommandInvocationDuration)
                return ExponentialBuckets(secStart, 2, 65);

            if (instrument.Unit == "seconds")
                return ExponentialBuckets(secStart, 2, 2.0);

            if (instrument.Unit == "bytes")
                return ExponentialBuckets(1024, 2, 130 * 1024 * 1024); // 1KB ... 128MB

            return ExponentialBuckets(secStart, 2, 10);
        }

        // The Counter from metrics doesn't count anything when there isn't a listener.
        // Since the listener may be initialized after we have registered DotvvmProperties, compiled some views, ...
        // the metrics would present incorrect data.
        internal class BareCounters
        {
            public static long ViewsCompiledOk = 0;
            public static long ViewsCompiledFailed = 0;
            public static long ViewsCompilationTime = 0;
            public static long DotvvmPropertyInitialized = 0;
        }

        internal static double[] ExponentialBuckets(double start, double factor, double end)
        {
            return Enumerable.Range(0, 1000)
                .Select(i => start * Math.Pow(factor, i))
                .TakeWhile(b => b <= end)
                .ToArray();
        }

        internal static KeyValuePair<string, object?> RouteLabel(this IDotvvmRequestContext context) =>
            new("route", context.Route?.RouteName);
        internal static KeyValuePair<string, object?> RequestTypeLabel(this IDotvvmRequestContext context)
        {
            var type = context.RequestType;
            return new("request_type", type.ToString());
        }

    }


    // stolen from https://source.dot.net/#Microsoft.Extensions.Http/ValueStopwatch.cs,492ce3a1c6245cd8
    internal struct ValueStopwatch
    {
        public static readonly double TimestampToSeconds = 1 / (double)Stopwatch.Frequency;

        private long _startTimestamp;

        public bool IsActive => _startTimestamp != 0;

        private ValueStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
        }

        public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());
        public static ValueStopwatch StartNew(bool isActive) =>
            isActive ? new ValueStopwatch(Stopwatch.GetTimestamp()) : default;

        public long ElapsedTicks
        {
            get
            {
                // Start timestamp can't be zero in an initialized ValueStopwatch. It would have to be literally the first thing executed when the machine boots to be 0.
                // So it being 0 is a clear indication of default(ValueStopwatch)
                if (!IsActive)
                {
                    return 0;
                }

                long end = Stopwatch.GetTimestamp();
                return end - _startTimestamp;
            }
        }

        public double ElapsedSeconds => ElapsedTicks * TimestampToSeconds;
    }
}
