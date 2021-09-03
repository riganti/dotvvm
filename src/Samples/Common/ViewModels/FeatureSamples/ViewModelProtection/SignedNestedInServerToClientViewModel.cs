using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModelProtection
{
    public class SignedNestedInServerToClientViewModel : DotvvmViewModelBase
    {
        [Bind(Direction.ServerToClient)]
        public Bomb CartoonBomb { get; set; } = new Bomb();

        public class Bomb
        {
            [Protect(ProtectMode.SignData)]
            public int Countdown { get; set; } = 42;
        }
    }
}
