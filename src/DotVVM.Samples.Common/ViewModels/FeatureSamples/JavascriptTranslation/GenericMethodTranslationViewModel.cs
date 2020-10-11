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
        public JavascriptTranslationModel<bool, JavascriptTranslationInnerTestModel<double[]>> Model2 { get; set; }
            = new JavascriptTranslationModel<bool, JavascriptTranslationInnerTestModel<double[]>>() {
                VModel2 = true,
                VModel = new JavascriptTranslationInnerTestModel<double[]>() {
                    Value = new[] { 20d } } };

        public JavascriptTranslationModel<bool> Model3 { get; set; } = new JavascriptTranslationModel<bool>() { Model = true };

        public JavascriptTranslationModel<JavascriptTranslationInnerModel<bool>> Model4 { get; set; }
            = new JavascriptTranslationModel<JavascriptTranslationInnerModel<bool>>() {
                Model = new JavascriptTranslationInnerModel<bool>() {
                    Model = false } };

        public JavascriptTranslationModel<JavascriptTranslationInnerModel<string[]>[]> Model5 { get; set; }
            = new JavascriptTranslationModel<JavascriptTranslationInnerModel<string[]>[]>() {
                Model = new[]{
                    new JavascriptTranslationInnerModel<string[]>() { Model = new[] { "str1", "str2" } },
                    new JavascriptTranslationInnerModel<string[]>() { Model = new[] { "str3", "str4" } }
                }
            };

        public override Task Load()
        {
            var w1 = JavascriptTranslationTestMethods.Unwrap(Model, Model2);
            var w3 = JavascriptTranslationTestMethods.Unwrap(Model3);
            var w4 = JavascriptTranslationTestMethods.Unwrap(Model4);
            var w5 = JavascriptTranslationTestMethods.Unwrap(Model5);
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

        public static T Unwrap<T>(JavascriptTranslationModel<JavascriptTranslationInnerModel<T>> transferModel)
        {
            return transferModel.Model.Model;
        }
    }
    public class JavascriptTranslationModel<T>
    {
        public T Model { get; set; }

    }
    public class JavascriptTranslationInnerModel<T>
    {
        public T Model { get; set; }
    }
    public class JavascriptTranslationInner2Model<T>
    {
        public T Model { get; set; }
    }
    public class JavascriptTranslationModel<Tx, Ty>
    {
        public Ty VModel { get; set; }
        public Tx VModel2 { get; set; }
    }
}

