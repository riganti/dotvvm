using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    public class StyleResource : RwResource
    {
        public string Uri { get; set; }
        public StyleResource(string uri, IEnumerable<string> dependencies)
        {
            this.Uri = uri;
            this.Dependencies = dependencies;
        }

        public StyleResource(string uri, params string[] dependencies) : this(uri, dependencies as IEnumerable<string>) { }

        public override void Render(IHtmlWriter writer)
        {
            writer.AddAttribute("href", Uri);
            writer.AddAttribute("rel", "stylesheet");
            writer.AddAttribute("type", "text/css");
            writer.RenderSelfClosingTag("link");
        }
    }
}
