using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Hosting
{
    public interface IOutputRenderer
    {

        Task RenderPage(RedwoodRequestContext context, RedwoodView view, string serializedViewModel);

        Task RenderViewModel(RedwoodRequestContext context, RedwoodView view, string serializedViewModel);
    }
}