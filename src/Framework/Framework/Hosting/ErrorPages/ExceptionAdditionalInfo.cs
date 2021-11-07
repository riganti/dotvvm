using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public record ExceptionAdditionalInfo(
        string Title,
        object[]? Objects,
        ExceptionAdditionalInfo.DisplayMode Display
    )
    {
        public enum DisplayMode
        {
            ToHtmlList,
            ToString,
            ObjectBrowser,
            KVTable
        }
    }
}
