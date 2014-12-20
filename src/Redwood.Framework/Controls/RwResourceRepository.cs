
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    public class RwResourceRepository
    {
        public Dictionary<string, RwResource> Resources { get; private set; }

        public RwResourceRepository Parent { get; set; }

        public RwResource Resolve(string name)
        {
            if (Resources.ContainsKey(name)) return Resources[name];
            else if (Parent != null) return Parent.Resolve(name);
            else throw new KeyNotFoundException("html resource was not found");
        }

        public bool IsRegistered(string name)
        {
            return Resources.ContainsKey(name) || (Parent != null && Parent.IsRegistered(name));
        }

        public void Register(string name, RwResource resource, bool replaceIfExists = true)
        {
            if (Resources.ContainsKey(name) && replaceIfExists)
                Resources[name] = resource;
            else Resources.Add(name, resource);
        }

        public RwResourceRepository Nest()
        {
            return new RwResourceRepository(this);
        }

        public RwResourceRepository(RwResourceRepository parent)
        {
            this.Resources = new Dictionary<string, RwResource>();
            this.Parent = parent;
        }

        public RwResourceRepository() : this(null) { }
    }
}
