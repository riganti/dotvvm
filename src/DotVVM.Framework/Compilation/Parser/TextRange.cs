namespace DotVVM.Framework.Compilation.Parser
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