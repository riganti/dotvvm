using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.CommandLine.Commands.Logic
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
                var sourceText = File.ReadAllText(fileName);

                var tokenizer = new DothtmlTokenizer();
                tokenizer.Tokenize(sourceText);
                var parser = new DothtmlParser();
                var tree = parser.Parse(tokenizer.Tokens);

                var results = new List<string>();
                foreach (var node in tree.EnumerateNodes().OfType<DothtmlElementNode>())
                {
                    if (node.FullTagName == "dot:ContentPlaceHolder" || node.FullTagName == "dot:SpaContentPlaceHolder")
                    {
                        var id = node.Attributes.FirstOrDefault(a => a.AttributeName == "ID");
                        if (id != null && id.ValueNode is DothtmlValueTextNode)
                        {
                            results.Add((id.ValueNode as DothtmlValueTextNode).Text );
                        }
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot extract ContentPlaceHolderIds from a file '{fileName}'!", ex);
            }
        }

    }
}
