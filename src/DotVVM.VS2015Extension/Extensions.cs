using DotVVM.VS2015Extension.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.VS2015Extension
{
    public static class Extensions
    {
        /// <summary>
        /// Replaces a range in a StringBuilder with specified content.
        /// </summary>
        public static void SetRange(this StringBuilder sb, int start, int length, string content)
        {
            length = Math.Min(content.Length, length);
            if (length < 0)
            {
                throw new ArgumentException("length");
            }

            for (int i = 0; i < length; i++)
            {
                if (start + i < sb.Length)
                {
                    sb[start + i] = content[i];
                }
                else
                {
                    sb.Append(content[i]);
                }
            }
        }

        public static IEnumerable<ITypeSymbol> GetThisAndAllBaseTypes(this ITypeSymbol symbol)
        {
            yield return symbol;
            while (symbol.BaseType != null)
            {
                yield return symbol.BaseType;
                symbol = symbol.BaseType;
            }
        }

        /// <summary>
        /// Parses text from ITextBuffer and returns root node of syntax tree.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static DothtmlRootNode GetDothtmlRootNode(this ITextBuffer buffer)
        {
            DothtmlTokenizer t = new DothtmlTokenizer();
            t.Tokenize(new StringReader(buffer.CurrentSnapshot.GetText()));
            var parser = new DothtmlParser();
            return parser.Parse(t.Tokens);
        }

        /// <summary>
        /// Filters collection by ContentType attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IEnumerable<T> WhereContentTypeAttribute<T>(this IEnumerable<T> collection, string contentType) where T : class
        {
            return collection.Where(w => w.GetType()
                .GetCustomAttributes(typeof(ContentTypeAttribute), true)
                .Cast<ContentTypeAttribute>()
                .Any(any => any.ContentTypes.Equals(contentType, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Filters collection by ContentType attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IEnumerable<T> WhereContentTypeAttributeNot<T>(this IEnumerable<T> collection, string contentType) where T : class
        {
            return collection.Where(w => !w.GetType()
                .GetCustomAttributes(typeof(ContentTypeAttribute), true)
                .Cast<ContentTypeAttribute>()
                .Any(any => any.ContentTypes.Equals(contentType, StringComparison.OrdinalIgnoreCase)));
        }
    }
}