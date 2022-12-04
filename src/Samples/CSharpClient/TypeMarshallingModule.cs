using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DotVVM.Framework.Interop.DotnetWasm;

namespace DotVVM.Samples.BasicSamples.CSharpClient
{
    public class TypeMarshallingModule
    {
        public byte MarshallByte(byte a) => (byte)(a * 2);
        public byte? MarshallNullableByte(byte? a) => (byte?)(a * 2);
        public sbyte MarshallSByte(sbyte a) => (sbyte)(a * 2);
        public sbyte? MarshallNullableSByte(sbyte? a) => (sbyte?)(a * 2);
        public short MarshallShort(short a) => (short)(a * 2);
        public short? MarshallNullableShort(short? a) => (short?)(a * 2);
        public ushort MarshallUShort(ushort a) => (ushort)(a * 2);
        public ushort? MarshallNullableUShort(ushort? a) => (ushort?)(a * 2);
        public int MarshallInt(int a) => a * 2;
        public int? MarshallNullableInt(int? a) => a * 2;
        public uint MarshallUInt(uint a) => a * 2;
        public uint? MarshallNullableUInt(uint? a) => a * 2;
        public long MarshallLong(long a) => a * 2;
        public long? MarshallNullableLong(long? a) => a * 2;
        public ulong MarshallULong(ulong a) => a * 2;
        public ulong? MarshallNullableULong(ulong? a) => a * 2;
        public float MarshallFloat(float a) => a * 2;
        public float? MarshallNullableFloat(float? a) => a * 2;
        public double MarshallDouble(double a) => a * 2;
        public double? MarshallNullableDouble(double? a) => a * 2;
        public decimal MarshallDecimal(decimal a) => a * 2;
        public decimal? MarshallNullableDecimal(decimal? a) => a * 2;
        public DateTime MarshallDateTime(DateTime a) => a.AddDays(1);
        public DateTime? MarshallNullableDateTime(DateTime? a) => a?.AddDays(1);
        public DateOnly MarshallDateOnly(DateOnly a) => a.AddDays(1);
        public DateOnly? MarshallNullableDateOnly(DateOnly? a) => a?.AddDays(1);
        public TimeOnly MarshallTimeOnly(TimeOnly a) => a.AddHours(1);
        public TimeOnly? MarshallNullableTimeOnly(TimeOnly? a) => a?.AddHours(1);
        public TimeSpan MarshallTimeSpan(TimeSpan a) => a + TimeSpan.FromHours(1);
        public TimeSpan? MarshallNullableTimeSpan(TimeSpan? a) => a + TimeSpan.FromHours(1);
        public char MarshallChar(char a) => char.ToUpper(a);
        public char? MarshallNullableChar(char? a) => a == null ? null : char.ToUpper(a.Value);
        public Guid MarshallGuid(Guid a) => new Guid("C286C18D-ECD8-47E0-BFC6-6CE709C5D498");
        public Guid? MarshallNullableGuid(Guid? a) => a == null ? null : new Guid("C286C18D-ECD8-47E0-BFC6-6CE709C5D498");
        public string MarshallString(string a) => a.ToUpper();
        public ChildEnum MarshallEnum(ChildEnum a) => (ChildEnum)(4 - a);
        public ChildEnum? MarshallNullableEnum(ChildEnum? a) => (ChildEnum?)(4 - a);
        public ChildObject MarshallObject(ChildObject child) => new ChildObject() { Int = child.Int + 1, String = child.String.ToUpper() };
        public ChildRecord MarshallRecord(ChildRecord child) => new ChildRecord(child.Int + 1, child.String.ToUpper());
        public ChildObject[] MarshallObjectArray(ChildObject[] a) => a.Reverse().ToArray();
        public ChildRecord[] MarshallRecordArray(ChildRecord[] a) => a.Reverse().ToArray();
        public int[] MarshallIntArray(int[] a) => a.Reverse().ToArray();
        public double?[] MarshallNullableDoubleArray(double?[] a) => a.Reverse().ToArray();
        public string[] MarshallStringArray(string[] a) => a.Reverse().ToArray();
        public async Task<ChildRecord[]> MarshallTask(ChildRecord[] a)
        {
            await Task.Delay(1000);
            return a.Reverse().ToArray();
        }
        public Exception MarshallException() => throw new Exception("Test exception");

        public TypeMarshallingModule(IViewModuleContext context)
        {
        }
    }

    public class ChildObject
    {
        public int Int { get; set; }

        public string String { get; set; } = null!;
    }

    public record ChildRecord(int Int, string String);

    public enum ChildEnum
    {
        One = 1,
        Two = 2,
        Three = 3
    }
}
