using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Analysers.Serializability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using VerifyCS = DotVVM.Analysers.Test.CSharpAnalyzerVerifier<
    DotVVM.Analysers.Serializability.ViewModelSerializabilityAnalyzer>;

namespace DotVVM.Analysers.Test.Serializability
{
    public class ViewModelSerializabilityTest
    {
        [Fact]
        public async void Test_NonSerializablePropertyRegularClass()
        {
            var test = @"
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            public int SerializableProperty { get; set; }
            public Stream NonSerializableProperty { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async void Test_NonSerializablePropertyInViewModel()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public int SerializableProperty { get; set; }
            {|#0:public Stream NonSerializableProperty { get; set; }|}
        }
    }",

            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.UseSerializablePropertiesRule)
                .WithLocation(0).WithArguments("Stream"));
        }

        [Fact]
        public async void Test_FieldsInViewModel()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public int SerializableProperty { get; set; }
            {|#0:public int Field;|}
        }
    }",

            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.DoNotUseFieldsRule).WithLocation(0));
        }
    }
}
