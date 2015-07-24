using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser
{
    public abstract class TokenBase
    {

        public int StartPosition { get; set; }

        public int Length { get; set; }

        public string Text { get; set; }

        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; }

        public TokenError Error { get; set; }

        public bool HasError
        {
            get { return Error != null; }
        }
        public override string ToString()
        {
            return string.Format("Token ({0}:{1}): {2}", StartPosition, Length, Text);
        }
    }

    public abstract class TokenBase<TTokenType> : TokenBase
    {

        public TTokenType Type { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1}:{2}): {3}", Type, StartPosition, Length, Text);
        }
    }
}