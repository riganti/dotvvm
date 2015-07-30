using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{debuggerDisplay,nq}")]
    public class DothtmlBindingNode : DothtmlLiteralNode
    {

        #region debbuger display
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay
        {
            get
            {
                return "{" + Name + ": " + Value + "}";
            }
        }
        #endregion

        public string Name { get; set; }
        

        public DothtmlBindingNode()
        {
            Escape = true;
        }
    }
}