using DotVVM.Framework.Parser.Dothtml.Parser;
using System.Linq;

namespace DotVVM.VS2015Extension.Bases.Directives
{
    public class ViewModelDirectiveValue : DirectiveValue
    {
        public DothtmlDirectiveNode DothtmlDirectiveNode { get; set; }
        private char separator = ',';

        public ViewModelDirectiveValue(DothtmlDirectiveNode dothtmlDirectiveNode) : base(dothtmlDirectiveNode.Value ?? "")
        {
            DothtmlDirectiveNode = dothtmlDirectiveNode;

            if (Value.Contains(separator))
            {
                var values = Value.Split(separator);
                TypeFullName = values[0];
                AssamblyName = values[1].Trim();
            }
            else
            {
                TypeFullName = Value;
            }
            TypeFullName = TypeFullName.Trim();
            if (TypeFullName.Contains('.'))
            {
                var splited = TypeFullName.Split('.').ToList();
                TypeName = splited.LastOrDefault()?.Trim();
                splited.Remove(TypeName);
                Namespace = string.Join(".", splited).Trim();
            }
        }

        public string AssamblyName { get; set; }

        public string Namespace { get; set; } = "";
        public string TypeFullName { get; set; }
        public string TypeName { get; set; }
    }
}