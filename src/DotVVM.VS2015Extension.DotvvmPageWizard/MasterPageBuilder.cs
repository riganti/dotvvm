using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    public class MasterPageBuilder
    {

        /// <summary>
        /// Extracts the place holder ids.
        /// </summary>
        public List<string> ExtractPlaceHolderIds(string fileName)
        {
            try
            {
                var tokenizer = new DothtmlTokenizer();
                tokenizer.Tokenize(new FileReader(fileName));
                var parser = new DothtmlParser();
                var tree = parser.Parse(tokenizer.Tokens);

                var results = new List<string>();
                foreach (var node in tree.EnumerateNodes().OfType<DothtmlElementNode>())
                {
                    if (node.FullTagName == "dot:ContentPlaceHolder" || node.FullTagName == "dot:SpaContentPlaceHolder")
                    {
                        var id = node.Attributes.FirstOrDefault(a => a.AttributeName == "ID");
                        if (id != null && id.Literal.GetType() == typeof(DothtmlLiteralNode))
                        {
                            results.Add(id.Literal.Value);
                        }
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                LogService.LogError(new Exception($"Cannot extract ContentPlaceHolderIds from a file '{fileName}'!", ex));
                return null;
            }
        }

    }
}
