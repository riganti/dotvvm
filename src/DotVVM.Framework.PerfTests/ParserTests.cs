using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.PerfTests
{
    public class ParserTests
    {
        string data;
        public void DownloadData(string page)
        {
            using (var wc = new WebClient()) {
                data = wc.DownloadString(page);
            }
        }

        public void TokenizeAndParse()
        {
            var t = new DothtmlTokenizer();
            t.Tokenize(data);
            var p = new DothtmlParser();
            var node = p.Parse(t.Tokens);
        }

        public void Tokenize()
        {
            var t = new DothtmlTokenizer();
            t.Tokenize(data);
        }
    }
}
