namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.ComboBox
{
    public class ComboBoxBooleanViewModel
    {
        public bool? NullableSelectedValue { get; set; }
        public bool NonNullableSelectedValue { get; set; }

        public bool[] Items { get; } = new bool[] { true, false };
        public bool?[] NullableItems { get; } = new bool?[] { true, false, null };
    }
}
