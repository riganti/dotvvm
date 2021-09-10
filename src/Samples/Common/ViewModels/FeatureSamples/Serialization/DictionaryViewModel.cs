using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class DictionaryViewModel : DotvvmViewModelBase
    {
        public List<KeyValuePair<string, string>> ListKeyValue { get; set; }
        public Dictionary<string, string> Dictionary { get; set; }
        public bool Result { get; set; }

        public override Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                Dictionary = GetData();

                ListKeyValue = Dictionary.ToList();
            }
            return base.PreRender();
        }

        private Dictionary<string, string> GetData() => new Dictionary<string, string> {
                    { "Prop1" ,"Value1"},
                    { "Prop2" ,"Value2"},
                    { "Prop3" ,"Value3"}
                };
        public void VerifyDeserialization()
        {
            var data = GetData();
            if (data.Count != Dictionary.Count || data.Count != ListKeyValue.Count)
            {
                Result = false;
            }
            for (int i = 0; i < data.Count; i++)
            {
                var item = data.ElementAt(i);
                var dictItem = Dictionary.ElementAt(i);
                var listItem = ListKeyValue[i];

                if (!(item.Key == dictItem.Key && item.Key == listItem.Key)
                    || !(item.Value == dictItem.Value && item.Value == listItem.Value))
                {
                    Result = false;
                    return;
                }
            }
            Result = true;
        }

    }
}

