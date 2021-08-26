using System;

namespace DotVVM.Framework.Controls
{
    public class DotvvmControlException : Exception
    {
        public Type? ControlType { get; set; }
        public int? LineNumber { get; set; }
        public string? FileName { get; set; }
        public (int, int)[]? Ranges { get; set; }

        public DotvvmControlException(DotvvmBindableObject control, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            ControlType = control.GetType();
            LineNumber = (int?)Internal.MarkupLineNumberProperty.GetValue(control);
            if (control.Parent != null) control = control.Parent;
            FileName = (string?)Internal.MarkupFileNameProperty.GetValue(control);
        }

        public DotvvmControlException(
            string message,
            Exception? innerException = null,
            Type? controlType = null,
            int? lineNumber = null,
            string? fileName = null,
            (int, int)[]? ranges = null)
            :base(message, innerException)
        {
            this.ControlType = ControlType;
            this.LineNumber = lineNumber;
            this.FileName = fileName;
            this.Ranges = ranges;
        }
    }
}
