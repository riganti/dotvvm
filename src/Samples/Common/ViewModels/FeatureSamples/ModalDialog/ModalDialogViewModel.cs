
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ModalDialog
{
    public class ModalDialogViewModel : DotvvmViewModelBase
    {
        public bool Dialog1Shown { get; set; }
        public bool DialogChained1Shown { get; set; }
        public bool DialogChained2Shown { get; set; }
        public bool CloseEventDialogShown { get; set; }

        public int? NullableIntController { get; set; }
        public string NullableStringController { get; set; }

        public DialogModel DialogWithModel { get; set; } = null;

        public int CloseEventCounter { get; set; } = 0;

        public void ShowDialogWithModel()
        {
            DialogWithModel = new DialogModel() { Property = "Hello" };
        }

        public void CloseDialogWithEvent()
        {
            CloseEventDialogShown = false;
        }

        public class DialogModel
        {
            public string Property { get; set; }
        }
    }

}
