using System.Text;
using DotVVM.Framework.Testing.SeleniumGenerator;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
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