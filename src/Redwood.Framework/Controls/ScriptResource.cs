using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    public class ScriptResource : RwResource
    {
        private const string CdnFallbackScript = "{0} || document.write(\"<script src='{1}' type='text/javascript'><\\/script>\")";
        
        /// <summary>
        /// Gets or sets the URI of the script on current server.
        /// </summary>
        public string LocalUri { get; set; }

        /// <summary>
        /// Gets or sets the URI of the script in CDN.
        /// </summary>
        public string CdnUri { get; set; }

        /// <summary>
        /// Gets or sets the javascript expression that check if script was loaded (typically name of the object created by library in the global scope)
        /// It is used to check whether script from CDN was loaded or whether to load it from local URL.
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


        /// <summary>
        /// Renders the script resource to the page.
        /// </summary>
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
