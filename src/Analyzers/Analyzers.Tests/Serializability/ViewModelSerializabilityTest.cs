using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using DotVVM.Analyzers.Serializability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynTestKit;
using Xunit;

namespace DotVVM.Analyzers.Tests.Serializability
{
    public class ViewModelSerializabilityTest : AnalyzerTestFixture
    {
        protected override string LanguageName => LanguageNames.CSharp;
        protected override DiagnosticAnalyzer CreateAnalyzer() => new ViewModelSerializabilityAnalyzer();
        protected override IReadOnlyCollection<MetadataReference> References { get; } = new List<MetadataReference>()
        {
            // CoreLib
            MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(object))!.Location),
            // System.Runtime
            MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(GCSettings))!.Location),
            // DotVVM.Framework
            MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(Framework.Controls.GridView))!.Location),
            // DotVVM.Core
            MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(Framework.ViewModel.BindAttribute))!.Location),
            // Newtonsoft.Json
            MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(Newtonsoft.Json.JsonConvert))!.Location)
        };
        private readonly Func<Document, ImmutableArray<Diagnostic>> analysisRunner;

        public ViewModelSerializabilityTest()
        {
            var getDiagnosticsInfo = typeof(AnalyzerTestFixture).GetMethod("GetDiagnostics", BindingFlags.Instance | BindingFlags.NonPublic)!;
            analysisRunner = (Func<Document, ImmutableArray<Diagnostic>>)getDiagnosticsInfo.CreateDelegate(typeof(Func<Document, ImmutableArray<Diagnostic>>), this);
        }

        [Fact]
        public void Test_NotSerializableProperty_RegularClass()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_NotSerializableProperty_ViewModel()
        {
            var code = @"
    using DotVVM.Framework.ViewModel;
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class DefaultViewModel : DotvvmViewModelBase
        {
            public int SerializableProperty { get; set; }
            public FileInfo NonSerializableProperty { get; set; }
        }
    }";

            var diagnostics = analysisRunner.Invoke(CreateDocumentFromCode(code));
            Assert.Single(diagnostics);
            Assert.Equal(DotvvmDiagnosticIds.UseSerializablePropertiesInViewModelRuleId, diagnostics[0].Id);
            Assert.Equal("public FileInfo NonSerializableProperty { get; set; }", GetAffectedCode(diagnostics[0], code));
        }

        [Fact]
        public void Test_SerializableRecordProperty_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_NotSerializableList_ViewModel()
        {
            var code = @"
            using DotVVM.Framework.ViewModel;
            using System;
            using System.Collections.Generic;
            using System.IO;

            namespace ConsoleApplication1
            {
                public class DefaultViewModel : DotvvmViewModelBase
                {
                    public int SerializableProperty { get; set; }
                    public List<Stream> NonSerializableList { get; set; }
                }
            }";

            var diagnostics = analysisRunner.Invoke(CreateDocumentFromCode(code));
            Assert.Single(diagnostics);
            Assert.Equal(DotvvmDiagnosticIds.UseSerializablePropertiesInViewModelRuleId, diagnostics[0].Id);
            Assert.Equal("public List<Stream> NonSerializableList { get; set; }", GetAffectedCode(diagnostics[0], code));
            Assert.Equal("this.NonSerializableList", GetMessageArgument(diagnostics[0]));
        }

        [Fact]
        public void Test_WarnAboutInterface_ViewModel()
        {
            var code = @"
            using DotVVM.Framework.ViewModel;
            using System;
            using System.Collections.Generic;
            using System.IO;

            namespace ConsoleApplication1
            {
                public class DefaultViewModel : DotvvmViewModelBase
                {
                    public IList<IDisposable> PotentiallyNonSerializableList { get; set; }
                }
            }";

            var diagnostics = analysisRunner.Invoke(CreateDocumentFromCode(code));
            Assert.Single(diagnostics);
            Assert.Equal(DotvvmDiagnosticIds.UseSerializablePropertiesInViewModelRuleId, diagnostics[0].Id);
            Assert.Equal("public IList<IDisposable> PotentiallyNonSerializableList { get; set; }", GetAffectedCode(diagnostics[0], code));
            Assert.Equal("this.PotentiallyNonSerializableList", GetMessageArgument(diagnostics[0]));
        }

        [Fact]
        public void Test_Primitives_AreSerializableAndSupported_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_Enums_AreSerializableAndSupported_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_DotVVMFriendlyObjects_AreSerializableAndSupported_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_NullablePrimitives_AreSerializableAndSupported_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_NullableStructs_AreSerializableAndSupported_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_NullableReferenceTypes_AreSerializableAndSupported_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_CommonTypesAreSerializableAndSupported_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_CollectionAreSerializableAndSupported_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_NoWarningsForEnumerables_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_UserTypesAreSerializableAndSupported_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_NotSupportedProperty_ViewModel()
        {
            var code = @"
            using DotVVM.Framework.ViewModel;
            using System;
            using System.Collections.Generic;

            namespace ConsoleApplication1
            {
                public class DefaultViewModel : DotvvmViewModelBase
                {
                    public int SerializableProperty { get; set; }
                    public Action Action { get; set; }
                }
            }";

            var diagnostics = analysisRunner.Invoke(CreateDocumentFromCode(code));
            Assert.Single(diagnostics);
            Assert.Equal(DotvvmDiagnosticIds.UseSerializablePropertiesInViewModelRuleId, diagnostics[0].Id);
            Assert.Equal("public Action Action { get; set; }", GetAffectedCode(diagnostics[0], code));
            Assert.Equal("this.Action", GetMessageArgument(diagnostics[0]));
        }

        [Fact]
        public void Test_PublicFieldsInViewModel()
        {
            var code = @"
            using DotVVM.Framework.ViewModel;
            using System;
            using System.IO;

            namespace ConsoleApplication1
            {
                public class DefaultViewModel : DotvvmViewModelBase
                {
                    public int SerializableProperty { get; set; }
                    public int Field;
                }
            }";

            var diagnostics = analysisRunner.Invoke(CreateDocumentFromCode(code));
            Assert.Single(diagnostics);
            Assert.Equal(DotvvmDiagnosticIds.DoNotUseFieldsInViewModelRuleId, diagnostics[0].Id);
            Assert.Equal("public int Field;", GetAffectedCode(diagnostics[0], code));
        }

        [Fact]
        public void Test_StaticPropertiesInViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_StaticFieldsInViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_NonPublicFieldsInViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_NonPublicPropertiesInViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        // https://github.com/dotnet/roslyn/issues/30248
        [Fact(Skip = "Roslyn is unable to resolve custom attributes for adhoc workspaces")]
        public void Test_IgnoreNonSerializedMembers_BindDirectionNone_ViewModel()
        {
            var code = @"
            using DotVVM.Framework.ViewModel;
            using System;
            using System.IO;

            namespace ConsoleApplication1
            {
                public class DefaultViewModel : DotvvmViewModelBase
                {
                    [Bind(Direction.None)]
                    public Stream Property { get; set; }
                }
            }";

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_SelfReferencingTypes_GenericArgs_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_SelfReferencingTypes_Properties_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }


        [Fact]
        public void Test_WhiteListedDotvvmTypes_Properties_ViewModel()
        {
            var code = @"
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

            Assert.Empty(analysisRunner.Invoke(CreateDocumentFromCode(code)));
        }

        [Fact]
        public void Test_InnerTypesWithNonSerializableProperties_RegularClass()
        {
            var code = @"
            using DotVVM.Framework.Controls;
            using DotVVM.Framework.ViewModel;
            using System;
            using System.IO;
            using System.Collections.Generic;


            namespace ConsoleApplication1
            {
                public class DefaultViewModel : DotvvmViewModelBase
                {
                    public List<Entry> Entries { get; set; }

                    public class Entry
                    {
                        public Stream Stream { get; set; }
                    }
                }
            }";

            var diagnostics = analysisRunner.Invoke(CreateDocumentFromCode(code));
            Assert.Single(diagnostics);
            Assert.Equal(DotvvmDiagnosticIds.UseSerializablePropertiesInViewModelRuleId, diagnostics[0].Id);
            Assert.Equal("public List<Entry> Entries { get; set; }", GetAffectedCode(diagnostics[0], code));
            Assert.Equal("this.Entries.Stream", GetMessageArgument(diagnostics[0]));
        }

        [Fact]
        public void Test_GenericReferenceType_Properties_ViewModel()
        {
            var code = @"
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
                    public WrappedValue<Stream> NonSerializable { get; set; }
                }

                public class WrappedValue<T>
                {
                    public string Name { get; set; }
                    public T Value { get; set; }
                }
            }";

            var diagnostics = analysisRunner.Invoke(CreateDocumentFromCode(code));
            Assert.Single(diagnostics);
            Assert.Equal(DotvvmDiagnosticIds.UseSerializablePropertiesInViewModelRuleId, diagnostics[0].Id);
            Assert.Equal("public WrappedValue<Stream> NonSerializable { get; set; }", GetAffectedCode(diagnostics[0], code));
            Assert.Equal("this.NonSerializable.Value", GetMessageArgument(diagnostics[0]));
        }

        private static string GetAffectedCode(Diagnostic diagnostic, string code)
        {
            var location = diagnostic.Location.SourceSpan;
            return code.Substring(location.Start, location.Length);
        }

        private static string GetMessageArgument(Diagnostic diagnostic)
        {
            var message = diagnostic.GetMessage().Split(':').Last()!;
            var argStartIndex = message.IndexOf('\'') + 1;
            var argEndIndex = message.LastIndexOf('\'');
            return message.Substring(argStartIndex, argEndIndex - argStartIndex);
        }
    }
}
