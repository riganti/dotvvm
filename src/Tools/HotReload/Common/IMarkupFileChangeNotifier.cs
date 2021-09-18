using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Diagnostics.ViewHotReload
{
    public interface IMarkupFileChangeNotifier
    {

        void NotifyFileChanged(IEnumerable<string> virtualPaths);

    }
}
