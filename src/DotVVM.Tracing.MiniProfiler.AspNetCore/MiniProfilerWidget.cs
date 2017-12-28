using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;

namespace DotVVM.Tracing.MiniProfiler.AspNetCore
{
    public class MiniProfilerWidget : DotvvmControl
    {
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
            context.ResourceManager.AddStartupScript("DotVVM-MiniProfiler-Integration",
                @"(function() {
                    var miniProfilerUpdate = function(arg) { 
                        if(arg.xhr && arg.xhr.getResponseHeader) { 
                            var jsonIds = arg.xhr.getResponseHeader('X-MiniProfiler-Ids'); 
                            if (jsonIds) {
                                var ids = JSON.parse(jsonIds);
                                MiniProfiler.fetchResults(ids);
                            }
                        }
                    };
                    dotvvm.events.afterPostback.subscribe(miniProfilerUpdate);
                    dotvvm.events.spaNavigated.subscribe(miniProfilerUpdate);
                })()", "dotvvm");

            base.OnPreRender(context);
        }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var html = StackExchange.Profiling.MiniProfiler.Current.RenderIncludes(
                context.GetAspNetCoreContext(),
                position: Position,
                showTrivial: ShowTrivial,
                showTimeWithChildren: ShowTimeWithChildren,
                maxTracesToShow: MaxTraces,
                showControls: ShowControls,
                startHidden: StartHidden);

            writer.WriteUnencodedText(html.ToString());

            base.RenderControl(writer, context);
        }
    }
}
