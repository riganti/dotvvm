using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVVM.CommandLine
{
    public static class Dothtml
    {
        public static List<string> ExtractPlaceholderIds(string filename, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;
            try
            {
                var sourceText = File.ReadAllText(filename);

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
            catch (Exception)
            {
                logger.LogError($"Could not extract ContentPlaceHoldersIds from '{filename}'.");
                return new List<string>();
            }
        }
    }
}
