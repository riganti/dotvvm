using System.Text;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public abstract class MarkupFileModification
    {

        public int Position { get; set; }

        public abstract void Apply(StringBuilder markupFile);

    }
}