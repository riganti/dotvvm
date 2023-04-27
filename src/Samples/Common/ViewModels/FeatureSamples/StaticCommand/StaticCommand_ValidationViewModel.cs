using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommand_ValidationViewModel : DotvvmViewModelBase
    {
        public TestUser User { get; set; } = new TestUser();
        public string Text { get; set; }
        public string ErrorMessage { get; set; } = "Custom error";
        public string PropertyPath { get; set; } = "/";

        [AllowStaticCommand]
        public static void ValidateNotNull<T>(T arg)
        {
            var modelState = new StaticCommandModelState();
            if (arg == null)
                modelState.AddArgumentError(nameof(arg), "Input can not be null");
            modelState.FailOnInvalidModelState();
        }

        [AllowStaticCommand]
        public static void AddError(string propertyPath, string message)
        {
            var modelState = new StaticCommandModelState();
            modelState.AddRawError(propertyPath, message);
            modelState.FailOnInvalidModelState();
        }
    }

    public class TestUser
    {
        public string Name { get; set; }
    }
}
