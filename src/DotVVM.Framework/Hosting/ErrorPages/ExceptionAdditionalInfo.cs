using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class ExceptionAdditionalInfo
    {
        public string Title { get; set; }
        public object[] Objects { get; set; }
        public DisplayMode Display { get; set; }

        public enum DisplayMode
        {
            ToHtmlList,
            ToString,
            ObjectBrowser,
            KVTable
        }
    }
}
