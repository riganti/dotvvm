using System.Text;
using System.Collections.Generic;

namespace DotVVM.Tool.Templates
{
    public static class PageTemplate
    {
        public static string TransformText(
            string viewModel,
            string? master,
            bool isMaster,
            IEnumerable<string>? contentPlaceholderIds)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"@viewModel {viewModel}");
            if (master is object)
            {
                sb.AppendLine($"@masterPage {master}");
                sb.AppendLine();
                if (contentPlaceholderIds is object)
                {
                    foreach (var placeholderId in contentPlaceholderIds)
                    {
                        sb.Append(
$@"<dot:Content ContentPlaceHolderID=""{placeholderId}"">

</dot:Content>
");
                    }
                }
            }
            else
            {
                sb.Append(
$@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <title></title>
</head>
<body>
");
                if (isMaster)
                {
                    sb.AppendLine("    <dot:ContentPlaceHolder ID=\"MainContent\" />");
                }
                sb.Append(
$@"</body>
</html>
");
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
