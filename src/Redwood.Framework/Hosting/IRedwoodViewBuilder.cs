using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Hosting
{
    public interface IRedwoodViewBuilder
    {

        RedwoodView BuildView(RedwoodRequestContext context);
    
    }
}
