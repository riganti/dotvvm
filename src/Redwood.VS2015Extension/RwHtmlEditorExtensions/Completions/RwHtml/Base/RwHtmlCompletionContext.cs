using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.LanguageServices;
using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
{
    public class RwHtmlCompletionContext
    {

        public RwHtmlTokenizer Tokenizer { get; set; }

        public RwHtmlParser Parser { get; set; }

        public int CurrentTokenIndex { get; set; }

        public IList<RwHtmlToken> Tokens { get; set; }

        public RwHtmlNode CurrentNode { get; set; }
        
        public VisualStudioWorkspace RoslynWorkspace { get; set; }

    }
}