using System.Text.Json.Serialization;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class STJPolymorphismWithOptoutViewModel : DotvvmViewModelBase
    {
        public BaseClass BaseObject { get; set; } = new Derived1 { Property1 = "abc", BaseProperty = "base1" };

        public string Result { get; set; }

        public void TestCommand()
        {
            Result = $"Command: Type={BaseObject.GetType().Name}, DerivedProperty={BaseObject switch {
                Derived1 d => d.Property1,
                Derived2 d => d.Property2.ToString(),
                _ => "unknown"
            }}";
        }

        public void ChangeToDerived1()
        {
            BaseObject = new Derived1 { Property1 = "derived1_value", BaseProperty = "base1" };
        }

        public void ChangeToDerived2()
        {
            BaseObject = new Derived2 { Property2 = 42, BaseProperty = "base2" };
        }

        [AllowStaticCommand]
        public static string TestStaticCommand(BaseClass obj) =>
            $"StaticCommand: Type={obj.GetType().Name}, DerivedProperty={obj switch {
                Derived1 d => d.Property1,
                Derived2 d => d.Property2.ToString(),
                _ => "unknown"
            }}";
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$t", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
    [JsonDerivedType(typeof(Derived1), typeDiscriminator: 1)]
    [JsonDerivedType(typeof(Derived2), typeDiscriminator: 2)]
    [DotvvmSerialization(DisableDotvvmConverter = true)]
    public class BaseClass
    {
        public string BaseProperty { get; set; }
    }

    public class Derived1 : BaseClass
    {
        public string Property1 { get; set; }
    }

    public class Derived2 : BaseClass
    {
        public int Property2 { get; set; }
    }

    public class Derived2B : Derived2
    {
        public int Property2B { get; set; }
    }
}
