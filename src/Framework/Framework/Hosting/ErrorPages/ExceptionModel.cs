using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class ExceptionModel
    {
        public ExceptionModel(Type exceptionType, StackFrameModel[] stack, Exception originalException, ExceptionAdditionalInfo[] additionalInfos)
        {
            ExceptionType = exceptionType;
            Stack = stack;
            Exception = originalException;
            AdditionalInfo = additionalInfos;
        }

        public Type ExceptionType { get; set; }
        public StackFrameModel[] Stack { get; set; }
        public ExceptionModel? InnerException { get; set; }
        public Exception Exception { get; set; }
        public ExceptionAdditionalInfo[] AdditionalInfo { get; set; }
    }
}
