using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    public class RedwoodView : RedwoodBindableControl
    {

        public Dictionary<string, string> Directives { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodView"/> class.
        /// </summary>
        public RedwoodView()
        {
            Directives = new Dictionary<string, string>();
        }

    }
}
