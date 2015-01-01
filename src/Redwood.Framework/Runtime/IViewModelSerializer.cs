using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Runtime
{
    public interface IViewModelSerializer
    {

        string SerializeViewModel(object viewModel, RedwoodView view);

        void PopulateViewModel(object viewModel, RedwoodView view, string serializedPostData, out Action invokedCommand);

    }
}