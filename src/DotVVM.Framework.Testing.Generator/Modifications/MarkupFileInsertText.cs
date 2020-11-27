using System.Text;

namespace DotVVM.Framework.Testing.Generator.Modifications
{
    public class MarkupFileInsertText : MarkupFileModification
    {
        public virtual string Text { get; set; }
        public override void Apply(StringBuilder markupFile)
        {
            markupFile.Insert(Position, Text);
        }
    }
}
