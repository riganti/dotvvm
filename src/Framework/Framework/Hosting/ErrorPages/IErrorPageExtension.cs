using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public interface IErrorPageExtension
    {

        string GetHeadContents(IDotvvmRequestContext context, Exception ex);

    }
}
