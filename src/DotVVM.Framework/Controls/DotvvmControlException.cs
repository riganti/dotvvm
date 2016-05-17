using System;

namespace DotVVM.Framework.Controls
{
    public class DotvvmControlException: Exception
    {
        public Type ControlType { get; set; }
        public int? LineNumber { get; set; }
        public string FileName { get; set; }

        public DotvvmControlException(DotvvmBindableObject control, string message, Exception innerException = null)
            : base(message, innerException)
        {
            ControlType = control.GetType();
            LineNumber = (int?)Internal.MarkupLineNumberProperty.GetValue(control);
            if (control.Parent != null) control = control.Parent;
            FileName = (string)Internal.MarkupFileNameProperty.GetValue(control);
        }
    }
}
