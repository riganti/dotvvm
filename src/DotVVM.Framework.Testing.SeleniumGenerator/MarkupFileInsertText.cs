using System.Text;

namespace DotVVM.Framework.Testing.SeleniumGenerator
{
    public class MarkupFileInsertText : MarkupFileModification
    {

        public string Text { get; set; }


        public override void Apply(StringBuilder markupFile)
        {
            markupFile.Insert(Position, Text);
        }
    }
}