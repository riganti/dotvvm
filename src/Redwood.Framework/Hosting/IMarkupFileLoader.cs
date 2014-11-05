using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Hosting
{
    public interface IMarkupFileLoader
    {

        MarkupFile GetMarkup(RedwoodRequestContext context);

    }
}