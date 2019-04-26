using System.Text;

namespace DotVVM.Framework.Testing.SeleniumGenerator
{
    public abstract class MarkupFileModification
    {

        public int Position { get; set; }

        public abstract void Apply(StringBuilder markupFile);

    }
}