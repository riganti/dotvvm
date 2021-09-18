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
        public async void Test_NotSerializableProperty_RegularClass()
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
        public async void Test_NotSerializableProperty_ViewModel()
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
            {|#0:public FileInfo NonSerializableProperty { get; set; }|}
        }
    }",

            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.UseSerializablePropertiesRule)
                .WithLocation(0).WithArguments("System.IO.FileInfo"));
        }

        [Fact]
        public async void Test_NotSerializableList_ViewModel()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public int SerializableProperty { get; set; }
            {|#0:public List<Stream> NonSerializableList { get; set; }|}
        }
    }",

            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.UseSerializablePropertiesRule)
                .WithLocation(0).WithArguments("System.Collections.Generic.List<System.IO.Stream>"));
        }

        [Fact]
        public async void Test_WarnAboutInterface_ViewModel()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public int SerializableProperty { get; set; }
            {|#0:public IList<Stream> NonSerializableList { get; set; }|}
        }
    }",

            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.DoNotUseUninstantiablePropertiesRule)
                .WithLocation(0).WithArguments("System.Collections.Generic.IList<System.IO.Stream>"));
        }

        [Fact]
        public async void Test_Primitives_AreSerializableAndSupported_ViewModel()
        {
            var test = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public bool Bool { get; set; }
            public byte Byte { get; set; }
            public sbyte Sbyte { get; set; }
            public short Short { get; set; }
            public ushort Ushort { get; set; }
            public int Int { get; set; }
            public uint Uint { get; set; }
            public long Long { get; set; }
            public ulong Ulong { get; set; }
            public float Float { get; set; }
            public double Double { get; set; }
            public decimal Decimal { get; set; }
            public char Char { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async void Test_NullablePrimitives_AreSerializableAndSupported_ViewModel()
        {
            var test = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public bool? Bool { get; set; }
            public byte? Byte { get; set; }
            public sbyte? Sbyte { get; set; }
            public short? Short { get; set; }
            public ushort? Ushort { get; set; }
            public int? Int { get; set; }
            public uint? Uint { get; set; }
            public long? Long { get; set; }
            public ulong? Ulong { get; set; }
            public float? Float { get; set; }
            public double? Double { get; set; }
            public decimal? Decimal { get; set; }
            public char? Char { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async void Test_CommonTypesAreSerializableAndSupported_ViewModel()
        {
            var test = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public object Object { get; set; }
            public string String { get; set; }
            public DateTime DateTime { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public Guid Guid { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async void Test_CollectionAreSerializableAndSupported_ViewModel()
        {
            var test = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public int[] Array { get; set; }
            public List<int> List { get; set; }
            public Dictionary<int, int> Dictionary { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async void Test_UserTypesAreSerializableAndSupported_ViewModel()
        {
            var test = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public UserType Property { get; set; }
        }

        public class UserType
        {
            public string Property { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async void Test_NotSupportedProperty_ViewModel()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public int SerializableProperty { get; set; }
            {|#0:public LinkedList<object> LinkedList { get; set; }|}
        }
    }",

            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.UseSerializablePropertiesRule)
                .WithLocation(0).WithArguments("System.Collections.Generic.LinkedList<object>"));
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
