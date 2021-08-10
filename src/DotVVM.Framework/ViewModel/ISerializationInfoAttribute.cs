using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.ViewModel
{
    public interface ISerializationInfoAttribute
    {
        void SetOptions(ViewModelPropertyMap map);
    }
}
