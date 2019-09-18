using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ComboBox
{
    public class ComboxItemBindingViewModel : DotvvmViewModelBase
    {
        public object SelectedValue { get; set; }
        public int? SelectedNullableInt { get; set; }
        public int SelectedInt { get; set; }
        public EnumType SelectedEnum2 { get; set; }
        public EnumType SelectedEnum { get; set; }
        public string SelectedString { get; set; }
        public ComplexType SelectedComplex { get; set; }
        public string[] EnumNames { get; set; } = Enum.GetNames(typeof(EnumType));
        public async override Task Load()
        {
            if (!Context.IsPostBack)
            {
                var enumValue = new[] { EnumType.EValue1, EnumType.EValue2, EnumType.EValue3 };
                ComplexData = Enumerable.Range(1, 10)
                    .Select(s => new ComplexType {
                        Id = s,
                        Text = $"Text {s}",
                        Date = new DateTime(2019, 10, s),
                        NestedComplex = new NestedComplexType { Text2 = $"Nested text {s}" },
                        EnumTypeValue = enumValue[(s - 1) % 3]
                    }).ToList();
                StringData = Enumerable.Range(1, 10).Select(s => $"Text string {s}").ToList();
                IntData = Enumerable.Range(1, 10).ToList();
            }
            await base.Load();
        }

        public void SetEnumValueToSecondaryField()
        {
            SelectedEnum2 = SelectedEnum;
        }

        public List<string> StringData { get; set; }
        public List<int> IntData { get; set; }
        public List<ComplexType> ComplexData { get; set; }

        public class ComplexType
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public DateTime Date { get; set; }
            public NestedComplexType NestedComplex { get; set; }
            public EnumType EnumTypeValue { get; set; }

        }
        public class NestedComplexType
        {
            public string Text2 { get; set; }
        }

        public enum EnumType
        {
            EValue1,
            EValue2,
            EValue3
        }
    }
}

