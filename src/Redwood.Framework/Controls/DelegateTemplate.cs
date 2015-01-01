using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    public class DelegateTemplate : ITemplate
    {

        public Func<RedwoodControl> BuildContentBody { get; set; }


        public void BuildContent(RedwoodControl container)
        {
            var control = BuildContentBody();
            container.Children.Add(control);
        }
    }
}