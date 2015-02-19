using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Parser
{
    public abstract class TokenBase<TTokenType> 
    {

        public TTokenType Type { get; set; }

        public int StartPosition { get; set; }

        public int Length { get; set; }

        public string Text { get; set; }

        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; }

        public string ErrorMessage { get; set; }

        public bool HasError
        {
            get { return ErrorMessage != null; }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}:{2}): {3}", Type, StartPosition, Length, Text);
        }
    }
}