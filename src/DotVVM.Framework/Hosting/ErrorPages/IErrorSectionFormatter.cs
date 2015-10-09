using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public interface IErrorSectionFormatter
    {
        void WriteHead(IErrorWriter writer);
        void WriteBody(IErrorWriter writer);
        string DisplayName { get; }
        string Id { get; }
    }
}
