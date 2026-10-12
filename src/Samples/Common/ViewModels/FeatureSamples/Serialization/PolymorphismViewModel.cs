using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class PolymorphismViewModel : DotvvmViewModelBase
    {
        public PolymorphicItem Item { get; set; } = new PolymorphicTextItem { Text = "first" };

        public PolymorphicProtectedItem ProtectedItem { get; set; } = new PolymorphicProtectedTextItem();

        public string Result { get; set; }

        public void ChangeToText() => Item = CreateText();

        public void ChangeToNumber() => Item = CreateNumber();

        public void TestCommand() =>
            Result = Describe(Item) + "; protected: " + ProtectedItem.Secret;

        [AllowStaticCommand]
        public static PolymorphicItem CreateText() => new PolymorphicTextItem { Text = "new text" };

        [AllowStaticCommand]
        public static PolymorphicItem CreateNumber() => new PolymorphicNumberItem { Number = 42 };

        [AllowStaticCommand]
        public static string Describe(PolymorphicItem item) => item switch {
            PolymorphicTextItem text => "Text: " + text.Text,
            PolymorphicNumberItem number => "Number: " + number.Number,
            _ => "Unknown"
        };
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
    [JsonDerivedType(typeof(PolymorphicTextItem), "text")]
    [JsonDerivedType(typeof(PolymorphicNumberItem), "number")]
    public abstract class PolymorphicItem
    {
        [Bind(Direction.ServerToClient)]
        public string Description { get; set; } = "Server-owned description";
    }

    public class PolymorphicTextItem : PolymorphicItem
    {
        [Required(ErrorMessage = "Text is required.")]
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class PolymorphicNumberItem : PolymorphicItem
    {
        [Range(1, 100)]
        public int Number { get; set; }
    }

    [JsonDerivedType(typeof(PolymorphicProtectedTextItem))]
    public abstract class PolymorphicProtectedItem
    {
        [Protect(ProtectMode.EncryptData)]
        public string Secret { get; set; } = "secret";
    }

    public class PolymorphicProtectedTextItem : PolymorphicProtectedItem
    {
        [Protect(ProtectMode.SignData)]
        public string SignedText { get; set; } = "signed";
    }
}
