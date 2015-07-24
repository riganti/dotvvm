using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Parser
{
    /// <summary>
    /// Represents a parser reader that is based on string variable.
    /// </summary>
    public class StringReader : IReader
    {
        private readonly string text;


        /// <summary>
        /// Gets the position.
        /// </summary>
        public int Position { get; private set; }


        /// <summary>
        /// Returns the char at the cursor, or Char.Zero, if we are on the end of file.
        /// </summary>
        public char Peek()
        {
            if (Position < text.Length)
            {
                return text[Position];
            }
            return DothtmlTokenizer.NullChar;
        }

        /// <summary>
        /// Returns the char at the cursor and advances to the next char, or returns Char.Zero, if we are on the end of file.
        /// </summary>
        public char Read()
        {
            var currentChar = Peek();
            if (currentChar != DothtmlTokenizer.NullChar)
            {
                Position++;
            }
            return currentChar;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StringReader"/> class.
        /// </summary>
        public StringReader(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}