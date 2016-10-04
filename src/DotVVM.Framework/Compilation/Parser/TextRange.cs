using System.Diagnostics;

namespace DotVVM.Framework.Compilation.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TextRange : ITextRange
    {
        public int StartPosition { get; private set; }
        public int Length { get; private set; }
        public int EndPosition => StartPosition + Length;

        private string DebuggerDisplay => $"[{StartPosition}..{EndPosition}), Length={Length}";

        public TextRange(int startPosition, int length)
        {
            StartPosition = startPosition;
            Length = length;
        }

        public static TextRange FromBounds(int start, int end)
            => new TextRange(start, end - start);
    }
}