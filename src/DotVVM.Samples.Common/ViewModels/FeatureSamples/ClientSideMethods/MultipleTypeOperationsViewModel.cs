using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientSideMethods
{
    public class MultipleTypeOperationsViewModel : MasterpageViewModel
    {
        public string Title { get; set; }
        public int Number { get; set; }
        public bool IsVisible { get; set; }

        [ClientSideMethod]
        public void Increase()
        {
            Number++;
            SetTitle();
        }

        [ClientSideMethod]
        public void Decrease()
        {
            Number--;
            SetTitle();
        }

        [ClientSideMethod]
        public void SetTitle()
        {
            Title = Number == 0 ? "Click me!" : "You have clicked me: " + Number + " times";
        }

        [ClientSideMethod]
        public void Reset()
        {
            Number = 0;
        }

        [ClientSideMethod]
        public void Show()
        {
            IsVisible = true;
        }

        [ClientSideMethod]
        public void Hide()
        {
            IsVisible = false;
        }
    }
}
