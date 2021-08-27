using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class ExceptionModel
    {
        public ExceptionModel(string typeName, string message, StackFrameModel[] stack, Exception originalException, ExceptionAdditionalInfo[] additionalInfos)
        {
            TypeName = typeName;
            Message = message;
            Stack = stack;
            OriginalException = originalException;
            AdditionalInfo = additionalInfos;
        }

        public string TypeName { get; set; }
        public string Message { get; set; }
        public StackFrameModel[] Stack { get; set; }
        public ExceptionModel? InnerException { get; set; }
        public Exception OriginalException { get; set; }
        public ExceptionAdditionalInfo[] AdditionalInfo { get; set; }
    }
}
