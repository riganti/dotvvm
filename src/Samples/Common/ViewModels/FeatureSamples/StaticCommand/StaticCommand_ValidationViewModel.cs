using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommand_ValidationViewModel : DotvvmViewModelBase
    {
        public string Text { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = "Custom error";
        public string PropertyPath { get; set; } = "/";
        public TestUser User { get; set; } = new TestUser()
        {
            Name = string.Empty,
            Child = new TestUser() { Name = string.Empty }
        };

        [AllowStaticCommand(StaticCommandValidation.Manual)]
        public static void ValidateNotNullOrEmptyUsingNameof(string arg)
        {
            var modelState = new StaticCommandModelState();
            if (arg == null || arg.Length == 0)
                modelState.AddArgumentError(nameof(arg), "Input can not be null or empty");
            modelState.FailOnInvalidModelState();
        }

        [AllowStaticCommand(StaticCommandValidation.Manual)]
        public static void ValidateNotNullOrEmptyUsingLambda(string arg)
        {
            var modelState = new StaticCommandModelState();
            if (arg == null || arg.Length == 0)
                modelState.AddArgumentError(() => arg, "Input can not be null or empty");
            modelState.FailOnInvalidModelState();
        }

        [AllowStaticCommand(StaticCommandValidation.Manual)]
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
        public TestUser Child { get; set; }
    }
}
