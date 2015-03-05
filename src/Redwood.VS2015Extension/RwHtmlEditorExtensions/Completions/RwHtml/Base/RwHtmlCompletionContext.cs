using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Redwood.Framework.Configuration;
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
        
        public Workspace RoslynWorkspace { get; set; }

        public IGlyphService GlyphService { get; set; }

        public DTE2 DTE { get; set; }

        public RedwoodConfiguration Configuration { get; set; }

        public MetadataControlResolver MetadataControlResolver { get; internal set; }
    }
}