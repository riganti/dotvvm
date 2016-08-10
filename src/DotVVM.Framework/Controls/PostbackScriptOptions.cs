namespace DotVVM.Framework.Controls
{
    public class PostbackScriptOptions
    {
        public bool UseWindowSetTimeout { get; set; }
        public bool? ReturnValue { get; set; }
        public bool IsOnChange { get; set; }
        public string ElementAccessor { get; set; }

        public PostbackScriptOptions(bool useWindowSetTimeout = false,
            bool? returnValue = false, bool isOnChange = false, string elementAccessor = "this")
        {
            UseWindowSetTimeout = useWindowSetTimeout;
            ReturnValue = returnValue;
            IsOnChange = isOnChange;
            ElementAccessor = elementAccessor;
        }
    }
}