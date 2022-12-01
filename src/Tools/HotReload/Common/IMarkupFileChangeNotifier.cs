using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.HotReload
{
    public interface IMarkupFileChangeNotifier
    {

        void NotifyFileChanged(IEnumerable<string> virtualPaths);

    }
}
