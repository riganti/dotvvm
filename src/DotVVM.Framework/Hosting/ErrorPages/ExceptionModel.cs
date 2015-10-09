using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class ExceptionModel
    {
        public string TypeName { get; set; }
        public string Message { get; set; }
        public StackFrameModel[] Stack { get; set; }
        public ExceptionModel InnerException { get; set; }
        public Exception OriginalException { get; set; }
        public ExceptionAdditionalInfo[] AdditionalInfo { get; set; }
    }
}
