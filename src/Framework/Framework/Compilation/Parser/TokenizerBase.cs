using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Parser
{
	public abstract class TokenizerBase<TToken, TTokenType> where TToken : TokenBase<TTokenType>
	{
		public const char NullChar = '\0';
		private string sourceText = "";
		private int sourcePosition;

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
		protected TToken? LastToken { get; private set; }

		private int position;

		/// <summary>
		/// Gets the distance since last token.
		/// </summary>
		protected int DistanceSinceLastToken
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get { return position - LastTokenPosition; }
		}

		/// <summary>
		/// Gets or sets the current token chars.
		/// </summary>
		protected StringBuilder CurrentTokenChars { get; private set; }

		/// <summary>
		/// Occurs when a token is found.
		/// </summary>
		public event Action<TToken>? TokenFound;

		/// <summary>
		/// Gets the list of tokens.
		/// </summary>
		public List<TToken> Tokens { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenizerBase{TToken, TTokenType}"/> class.
		/// </summary>
		public TokenizerBase()
		{
			CurrentTokenChars = new StringBuilder();
			Tokens = new List<TToken>();
		}

		/// <summary>
		/// Performs default tokenizer action
		/// </summary>
		public abstract void Tokenize(string sourceText);

		/// <summary>
		/// Tokenizes the input.
		/// </summary>
		protected bool TokenizeInternal(string sourceText, Func<bool> readFunction)
		{
			this.sourceText = sourceText;

			try
			{
				position = 0;
				CurrentLine = 1;
				PositionOnLine = 0;
				LastToken = null;
				LastTokenPosition = 0;
				Tokens.Clear();
				CurrentTokenChars.Clear();

				return readFunction();
			}
			catch (Exception ex) when (ex.Message == "Assertion failed!")
			{
				return false;
			}
		}

		/// <summary>
		/// Skips the whitespace.
		/// </summary>
		protected void SkipWhitespace(bool allowEndLine = true)
		{
			while (char.IsWhiteSpace(Peek()) && (allowEndLine || (Peek() != '\r' && Peek() != '\n')))
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
		protected void ReadTextUntilNewLine(TTokenType tokenType, params char[] stopChars)
		{
			int lastNonwhitespaceDistance = 0;

			while (Peek() != '\r' && Peek() != '\n' && !stopChars.Contains(Peek()))
			{
				lastNonwhitespaceDistance = char.IsWhiteSpace(Peek()) ? lastNonwhitespaceDistance + 1 : 0;
				if (Read() == NullChar)
				{
					break;
				}
			}
			if (DistanceSinceLastToken > 0)
			{
				CreateToken(tokenType, lastNonwhitespaceDistance);
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

		protected bool ReadTextUntil(TTokenType tokenType, string stopString, bool stopOnNewLine, string? nestString = null)
		{
			int nestLevel = 0;
			while (Peek() != NullChar)
			{
				if (PeekIsString(stopString)) nestLevel--;
				if (PeekIsString(nestString)) nestLevel++;
				if (nestLevel < 0)
				{
					CreateToken(tokenType);
					for (int i = 0; i < stopString.Length; i++) Read();
					return true;
				}
				Read();
			}
			return false;
		}

		protected bool PeekIsString(string? str)
		{
			if (str is null) return false;
			for (int i = 0; i < str.Length; i++)
			{
				if (Peek(i) != str[i]) return false;
			}
			return true;
		}

		protected string? ReadOneOf(params string[] strings)
		{
			int index = 0;
			while (strings.Length > 0 && !strings.Any(s => s.Length <= index))
			{
				var ch = Peek();
				strings = strings.Where(s => s[index] == ch).ToArray();
				if (strings.Length == 0) return null;
				index++;
				Read();
			}
			return strings.FirstOrDefault(s => s.Length == index);
		}

		protected abstract TToken NewToken(string text, TTokenType type, int lineNumber, int columnNumber, int length, int startPosition);

		/// <summary>
		/// Creates the token.
		/// </summary>
		protected TToken CreateToken(TTokenType type, int charsFromEndToSkip = 0, Func<TToken, TokenError>? errorProvider = null)
		{
			var length = CurrentTokenChars.Length - charsFromEndToSkip;
			string text;

			
			if (length < 200)
			{
				Span<char> data = stackalloc char[length];
				CurrentTokenChars.CopyTo(0, data, length);
				text = ((ReadOnlySpan<char>)data).DotvvmInternString(trySystemIntern: length < 10);
			}
			else
			{
				text = CurrentTokenChars.ToString().Substring(0, length);
			}

			var t = NewToken(text,
							 type,
							 lineNumber: CurrentLine,
							 columnNumber: Math.Max(0, PositionOnLine - DistanceSinceLastToken - 1),
							 length: length,
							 startPosition: LastTokenPosition
					);
			Tokens.Add(t);
			if (errorProvider != null)
			{
				t.Error = errorProvider(t);
			}

			CurrentTokenChars.Remove(0, t.Length);
			LastTokenPosition = position - charsFromEndToSkip;

			OnTokenFound(t);

			return LastToken = t;
		}

		protected TokenError CreateTokenError()
		{
			return new NullTokenError<TToken, TTokenType>(this);
		}

		protected TokenError CreateTokenError(TToken lastToken, TTokenType firstTokenType, string errorMessage, bool isCritical = false)
		{
			return new BeginWithLastTokenOfTypeTokenError<TToken, TTokenType>(errorMessage, this, lastToken, firstTokenType, isCritical);
		}

		protected TokenError CreateTokenError(TToken token, string errorMessage)
		{
			return new SimpleTokenError<TToken, TTokenType>(errorMessage, this, token);
		}

		/// <summary>
		/// Called when a token is found.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void OnTokenFound(TToken token)
		{
			TokenFound?.Invoke(token);
		}

		/// <summary>
		/// Peeks the current char.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected char Peek() => sourcePosition < sourceText.Length ? sourceText[sourcePosition] : NullChar;

		protected char Peek(int delta) => sourcePosition + delta < sourceText.Length ? sourceText[sourcePosition + delta] : NullChar;

		/// <summary>
		/// Returns the current char and advances to the next one.
		/// </summary>
		protected char Read()
		{
			var ch = Peek();
			sourcePosition++;
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
				PositionOnLine++;
				position++;
			}

			return ch;
		}
	}
}
