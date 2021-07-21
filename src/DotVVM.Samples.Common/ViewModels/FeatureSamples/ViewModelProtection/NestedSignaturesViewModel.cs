using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModelProtection
{
    public class NestedSignaturesViewModel : DotvvmViewModelBase
    {
        [Protect(ProtectMode.SignData)]
        public TypeA ObjA { get; set; } = new TypeA() { Next = new TypeA() };

        public void CheckEverythingIsFine()
        {
            var ok =
                ObjA.SignedThing == "A" &&
                ObjA.Next.SignedThing == "A" &&
                ObjA.Next.EncryptedThing == "A" &&
                ObjA.Next.Next == null &&
                ObjA.EncryptedThing == "A";

            if (!ok)
                throw new Exception();
        }

        public class TypeA
        {
            [Protect(ProtectMode.SignData)]
            public TypeA Next { get; set; }

            [Protect(ProtectMode.SignData)]
            public string SignedThing { get; set; } = "A";

            [Protect(ProtectMode.EncryptData)]
            public string EncryptedThing { get; set; } = "A";
        }
    }
}
