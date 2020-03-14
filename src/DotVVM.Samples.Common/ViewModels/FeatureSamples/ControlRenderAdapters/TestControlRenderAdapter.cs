using System;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ControlRenderAdapters
{
    public class TestControlRenderAdapter : IRenderAdapter
    {
        public Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext> AddAttributesToRender => AddAttributesToRenderImp;
        public Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext> RenderBeginTag => RenderBeginTagImp;
        public Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext> RenderContents => RenderContentsImp;
        public Action<IDotvvmControl, IHtmlWriter, IDotvvmRequestContext> RenderEndTag => RenderEndTagImp;


        private void AddAttributesToRenderImp(IDotvvmControl control, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddAttribute("id", "replaced");
            writer.AddAttribute("test", "testValue");
        }

        private void RenderBeginTagImp(IDotvvmControl control, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.RenderBeginTag("div");
        }

        private void RenderContentsImp(IDotvvmControl control, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.WriteText("REPLACEMENT TEXT");
        }

        private void RenderEndTagImp(IDotvvmControl control, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.RenderEndTag();
        }
    }
}
