using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Reference to Javascript file
    /// </summary>
    public class ScriptResource : RwResource
    {
        /// <summary>
        /// address of script on this server
        /// </summary>
        public string LocalUri { get; set; }
        /// <summary>
        /// address of script in some content delivery network
        /// </summary>
        public string CdnUri { get; set; }
        /// <summary>
        /// javascript expression to check if script was loaded (typcaly name of object created by library)
        /// used to check if script from cdn was loaded and determine whether to load it from local uri
        /// </summary>
        public string LoadCheckObject { get; set; }
        public ScriptResource(string localAddr, string cdnAddr, string loadCheckObject, IEnumerable<string> prereq)
        {
            this.Dependencies = prereq ?? new string[0];
            this.LocalUri = localAddr;
            this.CdnUri = cdnAddr;
            this.LoadCheckObject = loadCheckObject;
        }
        public ScriptResource(string localAddr, string cdnAddr, string loadCheckObject, params string[] prereq) : this(localAddr, cdnAddr, loadCheckObject, prereq as IEnumerable<string>) { }

        public ScriptResource(string localAddr, IEnumerable<string> prereq) : this(localAddr, null, null, prereq) { }
        public ScriptResource(string localAddr, params string[] prereq) : this(localAddr, null, null, prereq) { }

        private const string CdnFallbackScript = "{0} || document.write(\"<script src='{1}' type='text/javascript'><\\/script>\")";
        public override void Render(IHtmlWriter writer)
        {
            if (CdnUri != null)
            {
                writer.AddAttribute("src", CdnUri);
                writer.AddAttribute("type", "text/javascript");
                writer.RenderBeginTag("script");
                writer.RenderEndTag();

                if (LocalUri != null && LoadCheckObject != null)
                {
                    writer.RenderBeginTag("script");
                    writer.WriteUnencodedText(string.Format(CdnFallbackScript, LoadCheckObject, LocalUri));
                    writer.RenderEndTag();
                }
            }
            else if (LocalUri != null)
            {
                writer.AddAttribute("src", LocalUri);
                writer.AddAttribute("type", "text/javascript");
                writer.RenderBeginTag("script");
                writer.RenderEndTag();
            }
        }
    }
}
