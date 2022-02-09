using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Analyzers.Serializability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using VerifyCS = DotVVM.Analyzers.Tests.CSharpAnalyzerVerifier<
    DotVVM.Analyzers.Serializability.ViewModelSerializabilityAnalyzer>;

namespace DotVVM.Analyzers.Tests.Serializability
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
                .WithLocation(0).WithArguments("this.NonSerializableProperty"));
        }

        [Fact]
        public async void Test_SerializableRecordProperty_ViewModel()
        {
            var test = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public Employee Employee { get; set; }
        }

        public record Employee(int Id, string Name);
    }

    namespace System.Runtime.CompilerServices
    {
          internal static class IsExternalInit {}
    }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
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
                .WithLocation(0).WithArguments("this.NonSerializableList"));
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
            {|#0:public IList<IDisposable> PotentiallyNonSerializableList { get; set; }|}
        }
    }",

            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.UseSerializablePropertiesRule)
                .WithLocation(0).WithArguments("this.PotentiallyNonSerializableList"));
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
        public async void Test_Enums_AreSerializableAndSupported_ViewModel()
        {
            var test = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public enum MyEnum { A = 1, B = 2, C = 3 }

        [Flags]
        public enum MyFlagsEnum { A, B, C }

        public class DefaultViewModel : DotvvmViewModelBase
        {
            MyEnum Enum1 { get; set; }
            MyFlagsEnum Enum2 { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async void Test_DotVVMFriendlyObjects_AreSerializableAndSupported_ViewModel()
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
        public async void Test_NullableStructs_AreSerializableAndSupported_ViewModel()
        {
            var test = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public DateTime? DateTime { get; set; }
            public DateTimeOffset? DateTimeOffset { get; set; }
            public TimeSpan? TimeSpan { get; set; }
            public Guid? Guid { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async void Test_NullableReferenceTypes_AreSerializableAndSupported_ViewModel()
        {
            var test = @"
    #nullable enable
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public object? Object { get; set; }
            public string? String { get; set; }
            public Test? Test { get; set; }
        }

        public class Test
        {

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
            public DateTimeOffset DateTimeOffset { get; set; }
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
        public async void Test_NoWarningsForEnumerables_ViewModel()
        {
            var test = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public IEnumerable<int> Enumerable { get; set; }
            public IList<int> List { get; set; }
            public ICollection<int> Collection { get; set; }
            public ICollection<IList<IEnumerable<int>>> WowCollection { get; set; }
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
            public int? NullableProp { get; set; }
            public DateTime? NullableDateTime { get; set; }
            public DateTimeOffset? NullableDateTimeOffset { get; set; }
            public bool FlagProp { get; set; }
            public IList<int> List { get; set; }
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
            {|#0:public Action Action { get; set; }|}
        }
    }",

            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.UseSerializablePropertiesRule)
                .WithLocation(0).WithArguments("this.Action"));
        }

        [Fact]
        public async void Test_PublicFieldsInViewModel()
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
        [Fact]
        public async void Test_ConstFieldsInViewModel()
        {
            var text = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public const int Constant = 1;
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }


        [Fact]
        public async void Test_StaticPropertiesInViewModel()
        {
            var text = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public static Stream PublicProperty { get; set; }
            internal static Stream InternalProperty { get; set; }
            protected static Stream ProtectedProperty { get; set; }
            private static Stream privateProperty { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }

        [Fact]
        public async void Test_StaticFieldsInViewModel()
        {
            var text = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public static Stream PublicField { get; set; }
            internal static Stream InternalField { get; set; }
            protected static Stream ProtectedField { get; set; }
            private static Stream privateField { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }

        [Fact]
        public async void Test_NonPublicFieldsInViewModel()
        {
            var text = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public int SerializableProperty { get; set; }
            private int field;
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }

        [Fact]
        public async void Test_NonPublicPropertiesInViewModel()
        {
            var text = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public int SerializableProperty { get; set; }
            private Stream NonSerializableProperty { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }

        [Fact]
        public async void Test_IgnoreNonSerializedMembers_BindDirectionNone_ViewModel()
        {
            var text = @"
    using DotVVM.Framework.ViewModel;
    using Newtonsoft.Json;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            [Bind(Direction.None)]
            public Stream Property1 { get; set; }

            [JsonIgnore]
            public Stream Property2 { get; set; }

            [JsonIgnore]
            public int Field;
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }

        [Fact]
        public async void Test_SelfReferencingTypes_GenericArgs_ViewModel()
        {
            var text = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public List<List<List<int>>> Property { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }

        [Fact]
        public async void Test_SelfReferencingTypes_Properties_ViewModel()
        {
            var text = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public DefaultViewModel InnerVM { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }


        [Fact]
        public async void Test_WhiteListedDotvvmTypes_Properties_ViewModel()
        {
            var text = @"
    using DotVVM.Framework.Controls;
    using DotVVM.Framework.ViewModel;
    using System;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public GridViewDataSet<int> DataSet { get; set; }
            public UploadedFilesCollection UploadedFiles { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }

        [Fact]
        public async void Test_OverridenSerialization_OnProperty_ViewModel()
        {
            var text = @"
    using DotVVM.Framework.Controls;
    using DotVVM.Framework.ViewModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public NonSerializable Property { get; set; }
        }

        public class NonSerializable
        {
            public Stream Stream { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }

        [Fact]
        public async void Test_OverridenSerialization_OnTypeDeclaration_ViewModel()
        {
            var text = @"
    using DotVVM.Framework.Controls;
    using DotVVM.Framework.ViewModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public NonSerializable Property { get; set; }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public class NonSerializable
        {
            public Stream Stream { get; set; }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(text);
        }

        [Fact]
        public async void Test_InnerTypesWithNonSerializableProperties_RegularClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using DotVVM.Framework.Controls;
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;
    using System.Collections.Generic;


    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            {|#0:public List<Entry> Entries { get; set; }|}

            public class Entry
            {
                public Stream Stream { get; set; }
            }
        }
    }",
            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.UseSerializablePropertiesRule).WithLocation(0)
                .WithArguments("this.Entries.Stream"));
        }

        [Fact]
        public async void Test_GenericReferenceType_Properties_ViewModel()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using DotVVM.Framework.Controls;
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public WrappedValue<int> Value { get; set; }
            {|#0:public WrappedValue<Stream> NonSerializable { get; set; }|}
        }

        public class WrappedValue<T>
        {
            public string Name { get; set; }
            public T Value { get; set; }
        }
    }",

            VerifyCS.Diagnostic(ViewModelSerializabilityAnalyzer.UseSerializablePropertiesRule).WithLocation(0)
                .WithArguments("this.NonSerializable.Value"));
        }
    }
}
