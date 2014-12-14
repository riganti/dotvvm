using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    public abstract class HtmlResource
    {
        public string LocalUri { get; set; }
        public string CdnUri { get; set; }
        public virtual IEnumerable<HtmlResource> Prerequisities { get; set; }

        public abstract void Render(IHtmlWriter writer);
    }
}
