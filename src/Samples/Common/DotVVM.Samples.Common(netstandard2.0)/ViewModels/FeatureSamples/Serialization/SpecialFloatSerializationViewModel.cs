using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class SpecialFloatSerializationViewModel : DotvvmViewModelBase
    {
        public double NaNValue { get; set; } = 0;
        public double PositiveInfinity { get; set; } = 0;
        public double NegativeInfinity { get; set; } = 0;
        public string Result { get; set; }

        public override Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                NaNValue = double.NaN;
                PositiveInfinity = double.PositiveInfinity;
                NegativeInfinity = double.NegativeInfinity;
            }
            return base.PreRender();
        }

        public void SerializeValues()
        {
            Result = $"Command NaNValue: {NaNValue}, PositiveInfinity: {PositiveInfinity}, NegativeInfinity: {NegativeInfinity}";
        }

        [AllowStaticCommand]
        public static string SerializeValuesStatic(double nanValue, double positiveInfinity, double negativeInfinity)
        {
            return $"staticCommand1 NaNValue: {nanValue}, PositiveInfinity: {positiveInfinity}, NegativeInfinity: {negativeInfinity}";
        }

        [AllowStaticCommand]
        public static string SerializeWholeViewModel(SpecialFloatSerializationViewModel vm)
        {
            return $"staticCommand2 NaNValue: {vm.NaNValue}, PositiveInfinity: {vm.PositiveInfinity}, NegativeInfinity: {vm.NegativeInfinity}";
        }
    }
}
