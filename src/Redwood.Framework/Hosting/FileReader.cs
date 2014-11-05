using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.Framework.Hosting
{
    public class FileReader : IReader
    {
        private StreamReader streamReader;
        private char currentChar = RwHtmlTokenizer.NullChar;
        private int position = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileReader"/> class.
        /// </summary>
        public FileReader(string fileName)
        {
            streamReader = new StreamReader(fileName, true);
            ReadCore();
        }

        public int Position
        {
            get { return position; }
        }

        public char Peek()
        {
            return currentChar;
        }

        public char Read()
        {
            var oldChar = currentChar;
            ReadCore();
            position++;
            return oldChar;
        }

        private void ReadCore()
        {
            currentChar = streamReader.EndOfStream ? RwHtmlTokenizer.NullChar : (char)streamReader.Read();
        }

        public void Dispose()
        {
            if (streamReader != null)
            {
                streamReader.Dispose();
            }
        }
    }
}