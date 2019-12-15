using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelSerializationMapper
    {
        ViewModelSerializationMap GetMap(Type type);
    }
}
