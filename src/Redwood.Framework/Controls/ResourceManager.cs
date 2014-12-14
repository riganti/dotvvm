using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    public class ResourceManager
    {
        private HashSet<HtmlResource> _resourceSet = new HashSet<HtmlResource>();
        private List<HtmlResource> _resourceList = new List<HtmlResource>();

        public IReadOnlyList<HtmlResource> Resources
        {
            get { return _resourceList.AsReadOnly(); }
        }

        public void RenderLinks(IHtmlWriter writer)
        {
            foreach (var r in _resourceList)
            {
                r.Render(writer);
            }
        }
        /// <summary>
        /// add resource to collection
        /// </summary>
        public void AddResource(HtmlResource res)
        {
            if(!_resourceSet.Contains(res))
            {
                foreach (var prereq in res.Prerequisities)
                {
                    AddResource(prereq);
                }

                _resourceSet.Add(res);
                _resourceList.Add(res);
            }
        }

        /// <summary>
        /// add globaly registered resource with specified name
        /// </summary>
        public void AddResource(string name)
        {
            AddResource(GetResource(name));
        }

        private static readonly Dictionary<string, HtmlResource> ResourceRepo = new Dictionary<string, HtmlResource>();
        /// <summary>
        /// registers globaly available resource
        /// you can get it using GetResource method
        /// </summary>
        public static void RegisterGlobalResource(string name, HtmlResource res)
        {
            if(ResourceRepo.ContainsKey(name))
            {
                if (ResourceRepo[name] != res) throw new InvalidOperationException("name is already registered by another resource");
            }
            else
            {
                ResourceRepo.Add(name, res);
            }
        }
        public static HtmlResource GetResource(string name)
        {
            return ResourceRepo[name];
        }
    }
}
