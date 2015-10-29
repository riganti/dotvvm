using System;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Tests.Runtime
{
    public class ControlLifeCycleMock : HtmlGenericControl
    {

        public Action<HtmlGenericControl, IDotvvmRequestContext> PreInitAction { get; set; } = (control, context) => { };

        public Action<HtmlGenericControl, IDotvvmRequestContext> InitAction { get; set; } = (control, context) => { };

        public Action<HtmlGenericControl, IDotvvmRequestContext> LoadAction { get; set; } = (control, context) => { };

        public Action<HtmlGenericControl, IDotvvmRequestContext> PreRenderAction { get; set; } = (control, context) => { };

        public Action<HtmlGenericControl, IDotvvmRequestContext> PreRenderCompleteAction { get; set; } = (control, context) => { };


        internal override void OnPreInit(IDotvvmRequestContext context)
        {
            PreInitAction(this, context);
            base.OnPreInit(context);
        }

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            InitAction(this, context);
            base.OnInit(context);
        }

        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            LoadAction(this, context);
            base.OnLoad(context);
        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            PreRenderAction(this, context);
            base.OnPreRender(context);
        }

        internal override void OnPreRenderComplete(IDotvvmRequestContext context)
        {
            PreRenderCompleteAction(this, context);
            base.OnPreRenderComplete(context);
        }
    }
}