using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Controls
{
    public class PostbackScriptOptions
    {
        public bool UseWindowSetTimeout { get; set; }
        public bool? ReturnValue { get; set; }
        public bool IsOnChange { get; set; }
        public CodeParameterAssignment ElementAccessor { get; set; }
        public CodeParameterAssignment? KoContext { get; set; }
        public CodeParameterAssignment? CommandArgs { get; set; }
        public bool AllowPostbackHandlers { get; }

        public PostbackScriptOptions(bool useWindowSetTimeout = false,
            bool? returnValue = false,
            bool isOnChange = false,
            string elementAccessor = "this",
            CodeParameterAssignment? koContext = null,
            CodeParameterAssignment? commandArgs = null,
            bool allowPostbackHandlers = true)
        {
            this.UseWindowSetTimeout = useWindowSetTimeout;
            this.ReturnValue = returnValue;
            this.IsOnChange = isOnChange;
            this.ElementAccessor = new CodeParameterAssignment(elementAccessor, OperatorPrecedence.Max);
            this.KoContext = koContext;
            this.CommandArgs = commandArgs;
            this.AllowPostbackHandlers = allowPostbackHandlers;
        }
    }
}