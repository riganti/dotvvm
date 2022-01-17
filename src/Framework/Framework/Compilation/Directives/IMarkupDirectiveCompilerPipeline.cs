using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.Directives
{
    public interface IMarkupDirectiveCompilerPipeline
    {
        MarkupPageMetadata Compile(DothtmlRootNode dothtmlRoot, string fileName);
    }
}