using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class EnumSerializationCoercionViewModel : DotvvmViewModelBase
    {
        public MyEnum EnumWithString { get; set; } = MyEnum.One;

        public MyEnum EnumWithoutString { get; set; }

        public void ChangeValues()
        {
            EnumWithString = (MyEnum)(-1);
            EnumWithoutString = MyEnum.Two;
        }
    }

    public enum MyEnum
    {
        One = 1,
        Two = 2
    }
}

