using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Manages resources for one page
    /// </summary>
    public class RwResourceManager
    {
        /// <summary>
        /// hashset for fast contains queries
        /// </summary>
        private HashSet<string> _resourceSet = new HashSet<string>();
        /// <summary>
        /// list for order of resources
        /// </summary>
        private List<string> _resourceList = new List<string>();
        public RwResourceRepository Repo { get; set; }

        public RwResourceManager(RwResourceRepository repository)
        {
            this.Repo = repository;
        }

        public IReadOnlyList<string> Resources
        {
            get { return _resourceList.AsReadOnly(); }
        }

        /// <summary>
        /// renders all resources in the list
        /// </summary>
        /// <param name="writer"></param>
        public void Render(IHtmlWriter writer)
        {
            foreach (var name in _resourceList)
            {
                var r = Repo.Resolve(name);
                r.Render(writer);
                writer.WriteUnencodedText("\r\n");
            }
        }
        /// <summary>
        /// add resource to collection
        /// </summary>
        public void AddResource(string name)
        {
            if (!_resourceSet.Contains(name))
            {
                var res = Repo.Resolve(name);
                foreach (var prereq in res.Dependencies)
                {
                    AddResource(prereq);
                }

                _resourceSet.Add(name);
                _resourceList.Add(name);
            }
        }
    }
}
