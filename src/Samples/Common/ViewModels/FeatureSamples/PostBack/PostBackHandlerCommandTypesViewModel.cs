using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.PostBack
{
    public class PostBackHandlerCommandTypesViewModel : DotvvmViewModelBase
    {

        public ValidObject ValidValue { get; set; } = new ValidObject();

        public InvalidObject InvalidValue { get; set; } = new InvalidObject();

        public int Value { get; set; }

        public void SuccessfulAction()
        {
            Thread.Sleep(3000);
            Value++;
        }

        [AllowStaticCommand]
        public int Increment(int value)
        {
            Thread.Sleep(3000);
            return value + 1;
        }

        public void ValidationFailAction()
        {
            Thread.Sleep(3000);
            this.AddModelError("Validation error");
        }

        [AllowStaticCommand]
        public int ErrorAction()
        {
            Thread.Sleep(3000);
            throw new Exception();
        }

        public class ValidObject
        {
        }

        public class InvalidObject
        {

            [Required]
            public string Property { get; set; }

        }
    }
}
