namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public class ControlCommandBindingDTO
    {
        public string Value { get; set; } = "Init";
        public void Click()
        {
            Value = "changed";
        }
    }
}