using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Redwood.Framework.Resources;

namespace Redwood.Framework.Parser
{
    public abstract class TokenizerBase<TToken, TTokenType> where TToken : TokenBase<TTokenType>, new()
    {

        public const char NullChar = '\0';
        private IReader reader;
        private string fileName;


        /// <summary>
        /// Gets the type of the text token.
        /// </summary>
        protected abstract TTokenType TextTokenType { get; }

        /// <summary>
        /// Gets the type of the white space token.
        /// </summary>
        protected abstract TTokenType WhiteSpaceTokenType { get; }


        /// <summary>
        /// Gets or sets the current line number.
        /// </summary>
        protected int CurrentLine { get; private set; }

        /// <summary>
        /// Gets or sets the position on current line.
        /// </summary>
        protected int PositionOnLine { get; private set; }

        /// <summary>
        /// Gets the last token position.
        /// </summary>
        protected int LastTokenPosition { get; private set; }

        /// <summary>
        /// Gets the last token.
        /// </summary>
        protected TToken LastToken { get; private set; }

        /// <summary>
        /// Gets the distance since last token.
        /// </summary>
        protected int DistanceSinceLastToken
        {
            get { return reader.Position - LastTokenPosition; }
        }

        /// <summary>
        /// Gets or sets the current token chars.
        /// </summary>
        protected StringBuilder CurrentTokenChars { get; private set; }

        /// <summary>
        /// Occurs when a token is found.
        /// </summary>
        public event Action<TToken> TokenFound;

        /// <summary>
        /// Gets the list of tokens.
        /// </summary>
        public List<TToken> Tokens { get; private set; } 

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public List<ParserException> Errors { get; private set; } 


        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizerBase"/> class.
        /// </summary>
        public TokenizerBase()
        {
            CurrentTokenChars = new StringBuilder();
            Tokens = new List<TToken>();
            Errors = new List<ParserException>();
        }


        /// <summary>
        /// Tokenizes the input.
        /// </summary>
        public void Tokenize(IReader reader, string fileName)
        {
            this.reader = reader;
            this.fileName = fileName;

            try
            {
                CurrentLine = 1;
                PositionOnLine = 0;
                LastToken = null;
                LastTokenPosition = 0;
                Tokens.Clear();
                CurrentTokenChars.Clear();

                TokenizeCore();
            }
            finally
            {
                reader.Dispose();
            }
        }

        /// <summary>
        /// Tokenizes the core.
        /// </summary>
        protected abstract void TokenizeCore();


        /// <summary>
        /// Skips the whitespace.
        /// </summary>
        protected void SkipWhitespace()
        {
            while (Char.IsWhiteSpace(Peek()))
            {
                if (Read() == NullChar)
                {
                    break;
                }
            }
            if (DistanceSinceLastToken > 0)
            {
                CreateToken(WhiteSpaceTokenType);
            }
        }

        /// <summary>
        /// Skips the until new line or when it hits the specified stop chars. 
        /// When the new line is hit, the method automatically consumes it and creates WhiteSpace token.
        /// When the stopchar is hit, it is not consumed.
        /// </summary>
        protected void ReadTextUntilNewLine(params char[] stopChars)
        {
            while (Peek() != '\r' && Peek() != '\n' && !stopChars.Contains(Peek()))
            {
                if (Read() == NullChar)
                {
                    break;
                }
            }
            if (DistanceSinceLastToken > 0)
            {
                CreateToken(TextTokenType);
            }

            if (Peek() == '\r')
            {
                // \r can be followed by \n which is still one new line
                Read();
            }
            if (Peek() == '\n')
            {
                Read();
            }

            if (DistanceSinceLastToken > 0)
            {
                CreateToken(WhiteSpaceTokenType);
            }
        }


        /// <summary>
        /// Creates the token.
        /// </summary>
        protected TToken CreateToken(TTokenType type, int charsFromEndToSkip = 0)
        {
            LastToken = new TToken()
            {
                LineNumber = CurrentLine,
                ColumnNumber = PositionOnLine,
                StartPosition = LastTokenPosition,
                Length = DistanceSinceLastToken - charsFromEndToSkip,
                Type = type,
                Text = CurrentTokenChars.ToString().Substring(0, DistanceSinceLastToken - charsFromEndToSkip)
            };
            Tokens.Add(LastToken);
            
            CurrentTokenChars.Remove(0, LastToken.Length);
            LastTokenPosition = reader.Position - charsFromEndToSkip;

            OnTokenFound(LastToken);

            return LastToken;
        }


        /// <summary>
        /// Called when a token is found.
        /// </summary>
        protected virtual void OnTokenFound(TToken token)
        {
            var handler = TokenFound;
            if (handler != null)
            {
                handler(token);
            }
        }


        /// <summary>
        /// Peeks the current char.
        /// </summary>
        protected char Peek()
        {
            return reader.Peek();
        }

        /// <summary>
        /// Returns the current char and advances to the next one.
        /// </summary>
        protected char Read()
        {
            var ch = reader.Read();
            if (ch != NullChar)
            {
                CurrentTokenChars.Append(ch);

                if (ch == '\r' && Peek() != '\n')
                {
                    CurrentLine++;
                    PositionOnLine = 0;
                }
                else if (ch == '\n')
                {
                    CurrentLine++;
                    PositionOnLine = 0;
                }
            }

            PositionOnLine++;
            return ch;
        }

        /// <summary>
        /// Reports the error.
        /// </summary>
        protected void ReportError(string message, bool stopParsing = false)
        {
            Errors.Add(new ParserException(message, fileName, CurrentLine, PositionOnLine));

            if (stopParsing)
            {
                throw new ParserException(Parser_RwHtml.ParsingInterrupted, fileName);
            }
        }

    }

    
}