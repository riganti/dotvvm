using DotVVM.Framework.Configuration;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base
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

        public ICompletionSession CompletionSession { get; set; }

        public DotvvmConfiguration Configuration { get; set; }

        public MetadataControlResolver MetadataControlResolver { get; internal set; }
    }
}