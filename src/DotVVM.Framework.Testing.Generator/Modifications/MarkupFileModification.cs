using System.Text;

namespace DotVVM.Framework.Testing.SeleniumGenerator.Modifications
{
    public abstract class MarkupFileModification
    {
        public int Position { get; set; }

        public abstract void Apply(StringBuilder markupFile);

    }
}
