using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base
{
    public class DothtmlCompletionContext
    {

        public DothtmlTokenizer Tokenizer { get; set; }

        public DothtmlParser Parser { get; set; }

        public int CurrentTokenIndex { get; set; }

        public IList<DothtmlToken> Tokens { get; set; }

        public DothtmlNode CurrentNode { get; set; }
        
        public Workspace RoslynWorkspace { get; set; }

        public IGlyphService GlyphService { get; set; }

        public DTE2 DTE { get; set; }

        public DotvvmConfiguration Configuration { get; set; }

        public MetadataControlResolver MetadataControlResolver { get; internal set; }
    }
}