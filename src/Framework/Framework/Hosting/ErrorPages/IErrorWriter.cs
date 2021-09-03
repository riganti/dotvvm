using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public interface IErrorWriter
    {
        void WriteUnencoded(string str);
        void WriteText(string str);
        void ObjectBrowser(object? obj);
        void WriteKVTable(IEnumerable keys, IEnumerable values);
        void WriteSourceCode(SourceModel source, bool collapse = true);
    }
}
