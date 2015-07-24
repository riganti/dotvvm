using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser
{
    public class TextRange
    {
        public int StartPosition { get; private set; }

        public int Length { get; private set; }

        public TextRange(int startPosition, int length)
        {
            StartPosition = startPosition;
            Length = length;
        }
    }
}