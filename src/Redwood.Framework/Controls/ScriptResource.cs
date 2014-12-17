using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    public class ScriptResource: HtmlResource
    {
        public string LoadCheckObject { get; set; }
        public ScriptResource(string localAddr, string cdnAddr, string loadCheckObject, IEnumerable<HtmlResource> prereq)
        {
            this.Prerequisities = prereq ?? new HtmlResource[0];
            this.LocalUri = localAddr;
            this.CdnUri = cdnAddr;
            this.LoadCheckObject = loadCheckObject;
        }
        public ScriptResource(string localAddr, string cdnAddr, string loadCheckObject, params HtmlResource[] prereq) : this(localAddr, cdnAddr, loadCheckObject, prereq as IEnumerable<HtmlResource>) { }

        public ScriptResource(string localAddr, IEnumerable<HtmlResource> prereq) : this(localAddr, null, null, prereq) { }
        public ScriptResource(string localAddr, params HtmlResource[] prereq) : this(localAddr, null, null, prereq) { }

        private const string CdnFallbackScript = "{0} || document.write(\"<script src='{1}' type='text/javascript'></script>\"";
        public override void Render(IHtmlWriter writer)
        {
            if(CdnUri != null)
            {
                writer.AddAttribute("src", CdnUri);
                writer.AddAttribute("type", "text/javascript");
                writer.RenderBeginTag("script");
                writer.RenderEndTag();

                if(LocalUri != null && LoadCheckObject != null)
                {
                    writer.RenderBeginTag("script");
                    writer.WriteUnencodedText(string.Format(CdnFallbackScript, LoadCheckObject, LocalUri));
                }
            }
            else if(LocalUri != null)
            {
                writer.AddAttribute("src", LocalUri);
                writer.AddAttribute("type", "text/javascript");
                writer.RenderBeginTag("script");
                writer.RenderEndTag();
            }
        }

        public override bool Equals(object obj)
        {
            var s = obj as ScriptResource;
            return s.CdnUri == CdnUri &&
                s.LocalUri == LocalUri;
        }

        public override int GetHashCode()
        {
            return (CdnUri ?? "").GetHashCode() ^ (LocalUri ?? "").GetHashCode();
        }
    }
}
