using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.RenderSettingsModeServer
{
    public class RepeaterCollectionSetToNullViewModel : DotvvmViewModelBase
    {
        public List<SomeObject> Objects { get; protected set; }

        public string Value { get; set; }
        

        public void Test()
        {
            Objects = null;
            Value = "Null assigned to the collection";
        }

        public void Test2()
        {
            Objects = new()
            {
                new SomeObject() { Id = 1 },
                new SomeObject() { Id = 2 } 
            };
            Value = "Non-null assigned to the collection";
        }

        public override Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                Objects = new()
                {
                    new SomeObject() { Id = 1 },
                    new SomeObject() { Id = 2 }
                };
            }
            return base.PreRender();
        }
    }

    public class SomeObject
    {
        public int Id { get; set; }
    }
}

