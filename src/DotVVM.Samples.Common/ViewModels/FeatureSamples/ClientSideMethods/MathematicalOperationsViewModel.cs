
using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientSideMethods
{
    public class MathematicalOperationsViewModel : MasterpageViewModel
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Result { get; set; }

        public void SumOnServer()
        {
            var result = Left + Right;
            Result = result;
        }

        [ClientSideMethod]
        public void Sum()
        {
            var result = Left + Right;
            Result = result;
        }

        public void DivideOnServer()
        {
            if (Right == 0)
                return;
            Result = Left / Right;
        }

        [ClientSideMethod]
        public void Divide()
        {
            if (Right == 0)
                return;
            Result = Left / Right;
        }

        public void FibonacciOnServer()
        {
            int a = 0;
            int b = 1;
            for (int i = 0; i < Right; i++)
            {
                int temp = a;
                a = b;
                b = temp + b;
            }
            Result = a;
        }

        [ClientSideMethod]
        public void Fibonacci()
        {
            int a = 0;
            int b = 1;
            for (int i = 0; i < Right; i++)
            {
                int temp = a;
                a = b;
                b = temp + b;
            }
            Result = a;
        }
    }
}
