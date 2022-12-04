#if CSHARP_CLIENT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.CSharpClient;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CsharpClient
{
    public class MarshallingViewModel : DotvvmViewModelBase
    {

        public byte ByteValue { get; set; } = 0;
        public byte? NullableByteValue { get; set; } = null;
        public sbyte SByteValue { get; set; } = 1;
        public sbyte? NullableSByteValue { get; set; } = 2;
        public short ShortValue { get; set; } = 3;
        public short? NullableShortValue { get; set; } = 4;
        public ushort UShortValue { get; set; } = 5;
        public ushort? NullableUShortValue { get; set; } = 6;
        public int IntValue { get; set; } = 7;
        public int? NullableIntValue { get; set; } = 8;
        public uint UIntValue { get; set; } = 9;
        public uint? NullableUIntValue { get; set; } = 10;
        public long LongValue { get; set; } = 11;
        public long? NullableLongValue { get; set; } = 12;
        public ulong ULongValue { get; set; } = 13;
        public ulong? NullableULongValue { get; set; } = 14;
        public float FloatValue { get; set; } = 1.23f;
        public float? NullableFloatValue { get; set; } = null;
        public double DoubleValue { get; set; } = 4.5678;
        public double? NullableDoubleValue { get; set; } = 9999;
        public decimal DecimalValue { get; set; } = 1000000m;
        public decimal? NullableDecimalValue { get; set; } = 1000001m;
        public DateTime DateTimeValue { get; set; } = new DateTime(2020, 1, 2, 3, 4, 5);
        public DateTime? NullableDateTimeValue { get; set; } = null;
        public DateOnly DateOnlyValue { get; set; } = new DateOnly(2020, 10, 11);
        public DateOnly? NullableDateOnlyValue { get; set; } = new DateOnly(2020, 11, 12);
        public TimeOnly TimeOnlyValue { get; set; } = new TimeOnly(6, 0, 5);
        public TimeOnly? NullableTimeOnlyValue { get; set; } = null;
        public TimeSpan TimeSpanValue { get; set; } = new TimeSpan(2, 3, 4, 5);
        public TimeSpan? NullableTimeSpanValue { get; set; } = null;
        public char CharValue { get; set; } = 'b';
        public char? NullableCharValue { get; set; } = null;
        public Guid GuidValue { get; set; } = new Guid("2EF427B2-889C-42A6-B6C8-781839A46825");
        public Guid? NullableGuidValue { get; set; } = null;
        public string StringValue { get; set; } = "bababa";
        public ChildEnum EnumValue { get; set; } = ChildEnum.One;
        public ChildEnum? NullableEnumValue { get; set; } = null;
        public ChildObject ObjectValue { get; set; } = new ChildObject() { Int = 1, String = "abc" };
        public ChildRecord RecordValue { get; set; } = new ChildRecord(2, "def");
        public ChildObject[] ObjectArrayValue { get; set; } = new[] { new ChildObject() { Int = 1, String = "abc" }, new ChildObject() { Int = 3, String = "ghi" } };
        public ChildRecord[] RecordArrayValue { get; set; } = new[] { new ChildRecord(2, "def"), new ChildRecord(4, "jkl") };
        public int[] IntArrayValue { get; set; } = new[] { 2, 5 };
        public double?[] NullableDoubleArrayValue { get; set; } = new[] { 3.0, (double?)null };
        public string[] StringArrayValue { get; set; } = new[] { "abc", "def" };
        public ChildRecord[] TaskValue { get; set; } = new[] { new ChildRecord(6, "mno"), new ChildRecord(8, "pqr") };
    }
}
#endif
