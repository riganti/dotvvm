using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class GenericMethodTranslationViewModel : DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPA.SiteViewModel
    {
        public JavascriptTranslationModel<string, JavascriptTranslationInnerTestModel<JavascriptTranslationInnerTestModel<int>>> Model { get; set; } = new JavascriptTranslationModel<string, JavascriptTranslationInnerTestModel<JavascriptTranslationInnerTestModel<int>>>() { VModel2 = "Some data", VModel = new JavascriptTranslationInnerTestModel<JavascriptTranslationInnerTestModel<int>>() { Value = new JavascriptTranslationInnerTestModel<int>() { Value = 20 } } };
        public JavascriptTranslationModel<bool, JavascriptTranslationInnerTestModel<double[]>> Model2 { get; set; } = new JavascriptTranslationModel<bool, JavascriptTranslationInnerTestModel<double[]>>() { VModel2 = true, VModel = new JavascriptTranslationInnerTestModel<double[]>() { Value = new[] { 20d } } };
        public JavascriptTranslationModel<bool> Model3 { get; set; } = new JavascriptTranslationModel<bool>() { Model = true };

        public override Task Load()
        {
            var w = JavascriptTranslationTestMethods.Unwrap(this.Model, this.Model2);
            return base.Load();
        }

    }
    public class JavascriptTranslationInnerTestModel<Tmodel>
    {
        public Tmodel Value { get; set; }
    }
    public static class JavascriptTranslationTestMethods
    {
        public static T2 Unwrap<T3, T2, T4, T>(JavascriptTranslationModel<T2, T> transferModel, JavascriptTranslationModel<T3, T4> transferModel2)
        {
            return transferModel.VModel2;
        }

        public static T Unwrap<T>(JavascriptTranslationModel<T> transferModel)
        {
            return transferModel.Model;
        }
    }
    public class JavascriptTranslationModel<T>
    {
        public T Model { get; set; }

    }
    public class JavascriptTranslationModel<Tx, Ty>
    {
        public Ty VModel { get; set; }
        public Tx VModel2 { get; set; }
    }
}

