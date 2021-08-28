using System;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System.Collections.Generic;

namespace DotVVM.Framework.Tests.Runtime
{
    internal class ControlLifeCycleMock : HtmlGenericControl
    {
        private List<ControlLifeCycleEvent> eventLog;

        public string Name { get; }

        public ControlLifeCycleMock(List<ControlLifeCycleEvent> eventLog = null, string name = null)
        {
            LifecycleRequirements = ControlLifecycleRequirements.All;

            this.eventLog = eventLog;
            Name = name;
        }

        public Action<HtmlGenericControl, IDotvvmRequestContext> PreInitAction { get; set; } = (control, context) => { };

        public Action<HtmlGenericControl, IDotvvmRequestContext> InitAction { get; set; } = (control, context) => { };

        public Action<HtmlGenericControl, IDotvvmRequestContext> LoadAction { get; set; } = (control, context) => { };

        public Action<HtmlGenericControl, IDotvvmRequestContext> PreRenderAction { get; set; } = (control, context) => { };

        public Action<HtmlGenericControl, IDotvvmRequestContext> PreRenderCompleteAction { get; set; } = (control, context) => { };

        public Action<HtmlGenericControl, IDotvvmRequestContext> RenderAction { get; set; } = (control, context) => { };

        internal override void OnPreInit(IDotvvmRequestContext context)
        {
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.PreInit, true));
            PreInitAction(this, context);
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.PreInit, false));
            base.OnPreInit(context);
        }

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.Init, true));
            InitAction(this, context);
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.Init, false));
            base.OnInit(context);
        }

        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderAction(this, context);
            //base.Render(writer, context);
        }

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.Load, true));
            LoadAction(this, context);
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.Load, false));
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.PreRender, true));
            PreRenderAction(this, context);
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.PreRender, false));
            base.OnPreRender(context);
        }

        internal override void OnPreRenderComplete(IDotvvmRequestContext context)
        {
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.PreRenderComplete, true));
            PreRenderCompleteAction(this, context);
            eventLog?.Add(new ControlLifeCycleEvent(this, LifeCycleEventType.PreRenderComplete, false));
            base.OnPreRenderComplete(context);
        }
    }
}
