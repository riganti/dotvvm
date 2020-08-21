using System.Text;

namespace DotVVM.Tool.Templates
{
    public static class ControlTemplate
    {
        public static string TransformText(string? codeBehind)
        {
            var sb = new StringBuilder();
            sb.AppendLine("@viewModel System.Object");
            if (codeBehind is object)
            {
                sb.AppendLine($"@baseType {codeBehind}");
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
