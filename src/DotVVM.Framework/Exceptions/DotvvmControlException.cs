using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Exceptions
{
    public class DotvvmControlException: Exception
    {
        public Type ControlType { get; set; }
        public int? LineNumber { get; set; }
        public string FileName { get; set; }

        public DotvvmControlException(DotvvmControl control, string message, Exception innerException = null)
            : base(message, innerException)
        {
            ControlType = control.GetType();
            LineNumber = (int?)Internal.MarkupLineNumberProperty.GetValue(control);
            FileName = (string)Internal.MarkupFileNameProperty.GetValue(control);
        }
    }
}
