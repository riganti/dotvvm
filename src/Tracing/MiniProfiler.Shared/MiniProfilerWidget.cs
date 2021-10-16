using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;

namespace DotVVM.Tracing.MiniProfiler
{
    public class MiniProfilerWidget : DotvvmControl
    {
        public const string IntegrationJSResourceName = "dotvvm.miniprofiler";
        public static readonly string IntegrationJSEmbeddedResourceName
            = $"{typeof(MiniProfilerWidget).Assembly.GetName().Name}.MiniProfilerIntegration.js";

        /// <summary>
        /// The UI position to render the profiler in (defaults to <see cref="MiniProfilerBaseOptions.PopupRenderPosition"/>).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public RenderPosition? Position
        {
            get { return (RenderPosition?)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DotvvmProperty PositionProperty
            = DotvvmProperty.Register<RenderPosition?, MiniProfilerWidget>(c => c.Position, null);

        /// <summary>
        /// Whether to show trivial timings column initially or not (defaults to <see cref="MiniProfilerBaseOptions.PopupShowTrivial"/>).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool? ShowTrivial
        {
            get { return (bool?)GetValue(ShowTrivialProperty); }
            set { SetValue(ShowTrivialProperty, value); }
        }

        public static readonly DotvvmProperty ShowTrivialProperty
            = DotvvmProperty.Register<bool?, MiniProfilerWidget>(c => c.ShowTrivial, null);

        /// <summary>
        /// Whether to show time with children column initially or not (defaults to <see cref="MiniProfilerBaseOptions.PopupShowTimeWithChildren"/>).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool? ShowTimeWithChildren
        {
            get { return (bool?)GetValue(ShowTimeWithChildrenProperty); }
            set { SetValue(ShowTimeWithChildrenProperty, value); }
        }

        public static readonly DotvvmProperty ShowTimeWithChildrenProperty
            = DotvvmProperty.Register<bool?, MiniProfilerWidget>(c => c.ShowTimeWithChildren, null);

        /// <summary>
        /// The maximum number of profilers to show (before the oldest is removed - defaults to <see cref="MiniProfilerBaseOptions.PopupMaxTracesToShow"/>).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public int? MaxTraces
        {
            get { return (int?)GetValue(MaxTracesProperty); }
            set { SetValue(MaxTracesProperty, value); }
        }

        public static readonly DotvvmProperty MaxTracesProperty
            = DotvvmProperty.Register<int?, MiniProfilerWidget>(c => c.MaxTraces, null);

        /// <summary>
        /// Whether to show the controls (defaults to <see cref="MiniProfilerBaseOptions.ShowControls"/>).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool? ShowControls
        {
            get { return (bool?)GetValue(ShowControlsProperty); }
            set { SetValue(ShowControlsProperty, value); }
        }

        public static readonly DotvvmProperty ShowControlsProperty
            = DotvvmProperty.Register<bool?, MiniProfilerWidget>(c => c.ShowControls, null);

        /// <summary>
        /// Whether to start hidden (defaults to <see cref="MiniProfilerBaseOptions.PopupStartHidden"/>).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool? StartHidden
        {
            get { return (bool?)GetValue(StartHiddenProperty); }
            set { SetValue(StartHiddenProperty, value); }
        }

        public static readonly DotvvmProperty StartHiddenProperty
            = DotvvmProperty.Register<bool?, MiniProfilerWidget>(c => c.StartHidden, null);

        protected override void OnPreRender(IDotvvmRequestContext context)
        {
            var authorized = false;
#if OWIN
            var options = (StackExchange.Profiling.MiniProfiler.Current?.Options ?? StackExchange.Profiling.MiniProfiler.DefaultOptions) as MiniProfilerOptions;
            authorized = options?.ResultsAuthorize?.Invoke(HttpContext.Current.Request) ?? false;
#else
            var options = (StackExchange.Profiling.MiniProfiler.Current?.Options ?? StackExchange.Profiling.MiniProfiler.DefaultOptions) as MiniProfilerOptions;
            if (options != null)
            {
                authorized = options.ResultsAuthorize?.Invoke(context.GetAspNetCoreContext().Request) ?? false;

                if (options.ResultsAuthorize == null && options.ResultsAuthorizeAsync is object)
                {
                    // TODO: REVIEW whether this usage is correctly implemented
                    authorized = Task.Run(async () => {
                        return await options.ResultsAuthorizeAsync(context.GetAspNetCoreContext().Request).ConfigureAwait(false);
                    }).GetAwaiter().GetResult();
                }
            }
#endif
            if (authorized)
            {
                context.ResourceManager.AddRequiredResource(IntegrationJSResourceName);
            }
            base.OnPreRender(context);
        }



        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.WriteUnencodedText(ClientTimingHelper.InitScript);

            if (StackExchange.Profiling.MiniProfiler.Current is object)
            {

#if AspNetCore
                var html = StackExchange.Profiling.MiniProfiler.Current.RenderIncludes(
                              context.GetAspNetCoreContext(),
                              position: Position,
                              showTrivial: ShowTrivial,
                              showTimeWithChildren: ShowTimeWithChildren,
                              maxTracesToShow: MaxTraces,
                              showControls: ShowControls,
                              startHidden: StartHidden);
#else
                var html = StackExchange.Profiling.MiniProfiler.Current.RenderIncludes(
                              position: Position,
                              showTrivial: ShowTrivial,
                              showTimeWithChildren: ShowTimeWithChildren,
                              maxTracesToShow: MaxTraces,
                              showControls: ShowControls,
                              startHidden: StartHidden);


#endif
                writer.WriteUnencodedText(html.ToString());
            }

            base.RenderControl(writer, context);
        }
    }
}
