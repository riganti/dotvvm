using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Text.Unicode;
using DotVVM.Framework.Utils;

// interface IBufferConsumer
// {
//     bool AllowPartialWrites { get; }
//     void Write(ref byte[]? buffer, int offset, int count);
//     void Write(ReadOnlySpan<byte> value);
// }

sealed class Utf8StringWriter : IDisposable, IBufferWriter<byte>
{
    private byte[] buffer;
    private int bufferIndex;

    private readonly Stream? stream;
    private readonly bool leaveStreamOpen;

    private const int PreferredBufferSize = 8192;
    // we will always keep the last 32 bytes of the buffer free
    // this allows us to flush after small writes, which is sligly cheaper:
    // Version when we Flush before write:
    //      Utf8StringWriter:WriteQuot():this (FullOpts):
    //      G_M54106_IG01:  ;; offset=0x0000
    //             push     r15
    //             push     rbx
    //             push     rax
    //      G_M54106_IG02:  ;; offset=0x0004
    //             mov      ebx, dword ptr [rdi+0x10]
    //             mov      r15, gword ptr [rdi+0x08]
    //             lea      eax, [rbx+0x08]
    //             cmp      dword ptr [r15+0x08], eax
    //             jg       SHORT G_M54106_IG04
    //      G_M54106_IG03:  ;; offset=0x0014
    //             call     [Utf8StringWriter:FlushBuffer():this]
    //             xor      ebx, ebx
    //      G_M54106_IG04:  ;; offset=0x001C
    //             movsxd   rax, ebx
    //             mov      rcx, 0x3B746F757126
    //             mov      qword ptr [r15+rax+0x10], rcx
    //      G_M54106_IG05:  ;; offset=0x002E
    //             add      rsp, 8
    //             pop      rbx
    //             pop      r15
    //             ret    
    // And when we Flush after write (the JIT now uses volatile registers, avoiding push r15, pop r15, ...):
    //     Utf8StringWriter:WriteQuot():this (FullOpts):
    //     G_M54106_IG01:  ;; offset=0x0000
    //     G_M54106_IG02:  ;; offset=0x0000
    //            mov      eax, dword ptr [rdi+0x10]
    //            mov      rcx, gword ptr [rdi+0x08]
    //            cmp      byte  ptr [rcx], cl
    //            movsxd   rdx, eax
    //            mov      rsi, 0x3B746F757126
    //            mov      qword ptr [rcx+rdx+0x10], rsi
    //            add      eax, 38
    //            cmp      dword ptr [rcx+0x08], eax
    //            jg       SHORT G_M54106_IG04
    //     G_M54106_IG03:  ;; offset=0x0023
    //            tail.jmp [Utf8StringWriter:FlushBuffer():this]
    //     G_M54106_IG04:  ;; offset=0x0029
    //            ret
    private const int MinFreeBufferSpace = 32;
    private const int CodepointMaxLength = 4; // single unicode codepoint can be up to 4 bytes long in UTF-8
    // In the worst case, a single UTF-16 character could be expanded to 3 UTF-8 bytes.
    // Only surrogate pairs expand to 4 UTF-8 bytes but that is a transformation of 2 UTF-16 characters going to 4 UTF-8 bytes (factor of 2).
    // All other UTF-16 characters can be represented by either 1 or 2 UTF-8 bytes.
    public const int Utf16CodeMaxLength = 3;


    public Utf8StringWriter(Stream stream, bool leaveStreamOpen, int bufferSize = PreferredBufferSize)
    {
        this.stream = stream;
        this.leaveStreamOpen = leaveStreamOpen;
        bufferIndex = 0;
        buffer = ArrayPool<byte>.Shared.Rent(Math.Max(4 * MinFreeBufferSpace, bufferSize));

        if (!BitConverter.IsLittleEndian)
            throw new Exception("This thing only works on little endian machines");
    }

    public Utf8StringWriter() : this(bufferSize: PreferredBufferSize)
    {
    }

    public Utf8StringWriter(int bufferSize)
    {
        this.stream = null;
        bufferIndex = 0;
        buffer = ArrayPool<byte>.Shared.Rent(Math.Max(4 * MinFreeBufferSpace, bufferSize));
    }

    public int BufferSize => buffer.Length;
    public ReadOnlySpan<byte> PendingBytes => buffer.AsSpan(0, bufferIndex);
    public int BufferPosition => bufferIndex;

    public void Clear()
    {
        bufferIndex = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureBufferFreeSpace()
    {
        if (bufferIndex + MinFreeBufferSpace >= buffer.Length)
        {
            FlushBuffer();
        }
        AssertInvariants();
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void FlushBuffer()
    {
        Debug.Assert(bufferIndex <= buffer.Length);
        if (stream is { })
        {
            if (bufferIndex > 0)
            {
                stream.Write(buffer, 0, bufferIndex);
                bufferIndex = 0;
            }
        }
        else
        {
            EnlargeBuffer(buffer.Length * 2);
        }
    }

    private void EnlargeBuffer(int minSize)
    {
        if (minSize < buffer.Length)
            return;

        var oldBuffer = buffer;
        var newBuffer = ArrayPool<byte>.Shared.Rent(minSize);
        oldBuffer.CopyTo(newBuffer.AsSpan());
        this.buffer = newBuffer;
        ArrayPool<byte>.Shared.Return(oldBuffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> BufferRestSpan() =>
#if NET5_0_OR_GREATER
        MemoryMarshal.CreateSpan(ref BufferRef(), buffer.Length - bufferIndex);
#else
        buffer.AsSpan(bufferIndex);
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref byte BufferRef() =>
        ref BufferRef(this.buffer, this.bufferIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte BufferRef(byte[] buffer, int index)
    {
#if NET5_0_OR_GREATER
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(buffer), index);
#else
        return ref buffer[index];
#endif
    }

    public void WriteQuot() // test for disasm
    {
        WriteByte((byte)'&', (byte)'q', (byte)'u', (byte)'o', (byte)'t', (byte)';');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte a)
    {
        Debug.Assert(a < 0x80, "Invalid UTF-8");
        Debug.Assert(this.bufferIndex + 1 < buffer.Length);
        BufferRef() = a;
        bufferIndex++;
        EnsureBufferFreeSpace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte a, byte b)
    {
        Debug.Assert(Utf8.IsValid([a, b]), "Invalid UTF-8");
        Debug.Assert(this.bufferIndex + 2 < buffer.Length);
        ushort value = (ushort)(a | (b << 8));
        Unsafe.WriteUnaligned<ushort>(ref BufferRef(), value);
        bufferIndex += 2;
        EnsureBufferFreeSpace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte a, byte b, byte c)
    {
        Debug.Assert(Utf8.IsValid([a, b, c]), "Invalid UTF-8");
        Debug.Assert(this.bufferIndex + 4 < buffer.Length);
        uint value = (uint)a | ((uint)b << 8) | ((uint)c << 16);
        Unsafe.WriteUnaligned<uint>(ref BufferRef(), value);
        bufferIndex += 3;
        EnsureBufferFreeSpace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte a, byte b, byte c, byte d)
    {
        Debug.Assert(Utf8.IsValid([a, b, c, d]), "Invalid UTF-8");
        Debug.Assert(this.bufferIndex + 4 < buffer.Length);
        uint value = (uint)a | ((uint)b << 8) | ((uint)c << 16) | ((uint)d << 24);
        Unsafe.WriteUnaligned<uint>(ref BufferRef(), value);
        bufferIndex += 4;
        EnsureBufferFreeSpace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte a, byte b, byte c, byte d, byte e)
    {
        Debug.Assert(Utf8.IsValid([a, b, c, d, e]), "Invalid UTF-8");
        Debug.Assert(this.bufferIndex + 8 < buffer.Length);
        ulong value = a | ((ulong)b << 8) | ((ulong)c << 16) | ((ulong)d << 24) | ((ulong)e << 32);
        Unsafe.WriteUnaligned<ulong>(ref BufferRef(), value);
        bufferIndex += 5;
        EnsureBufferFreeSpace();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte a, byte b, byte c, byte d, byte e, byte f)
    {
        Debug.Assert(Utf8.IsValid([a, b, c, d, e, f]), "Invalid UTF-8");
        Debug.Assert(this.bufferIndex + 8 < buffer.Length);
        ulong value = a | ((ulong)b << 8) | ((ulong)c << 16) | ((ulong)d << 24) | ((ulong)e << 32) | ((ulong)f << 40);
        Unsafe.WriteUnaligned<ulong>(ref BufferRef(), value);
        bufferIndex += 6;
        EnsureBufferFreeSpace();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte a, byte b, byte c, byte d, byte e, byte f, byte g)
    {
        Debug.Assert(Utf8.IsValid([a, b, c, d, e, f, g]), "Invalid UTF-8");
        Debug.Assert(this.bufferIndex + 8 < buffer.Length);
        ulong value = a | ((ulong)b << 8) | ((ulong)c << 16) | ((ulong)d << 24) | ((ulong)e << 32) | ((ulong)f << 40) | ((ulong)g << 48);
        Unsafe.WriteUnaligned<ulong>(ref BufferRef(), value);
        bufferIndex += 7;
        EnsureBufferFreeSpace();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h)
    {
        Debug.Assert(Utf8.IsValid([a, b, c, d, e, f, g, h]), "Invalid UTF-8");
        Debug.Assert(this.bufferIndex + 8 < buffer.Length);
        ulong value = a | ((ulong)b << 8) | ((ulong)c << 16) | ((ulong)d << 24) | ((ulong)e << 32) | ((ulong)f << 40) | ((ulong)g << 48) | ((ulong)h << 56);
        Unsafe.WriteUnaligned<ulong>(ref BufferRef(), value);
        bufferIndex += 8;
        EnsureBufferFreeSpace();
    }

    private void WriteSmall(ReadOnlySpan<byte> value)
    {
        var valueLength = (nuint)value.Length;
        Debug.Assert(valueLength <= 32);
        Debug.Assert(buffer.Length >= bufferIndex + value.Length);
        Debug.Assert(buffer.Length >= bufferIndex + MinFreeBufferSpace);
#if DEBUG
        var lastBufferChar = bufferIndex > 0 ? buffer[bufferIndex - 1] : (byte)0;
        var lastBufferLength = bufferIndex;
#endif

        ref byte bufferRef = ref BufferRef();
        bufferIndex += (int)valueLength;
        var indexFromEnd = bufferIndex - buffer.Length;

        ref byte valueRef = ref MemoryMarshal.GetReference(value);
        if (valueLength >= 8)
        {
            if (valueLength > 16)
            {
#if NET8_0_OR_GREATER
                Vector128.LoadUnsafe<byte>(ref valueRef).StoreUnsafe(ref bufferRef);
                Vector128.LoadUnsafe<byte>(ref valueRef, valueLength - 16).StoreUnsafe(ref bufferRef, valueLength - 16);
#else
                Unsafe.WriteUnaligned<ulong>(ref bufferRef, Unsafe.ReadUnaligned<ulong>(ref valueRef));
                Unsafe.WriteUnaligned<ulong>(ref Unsafe.Add(ref bufferRef, 8), Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref valueRef, 8)));
                Unsafe.WriteUnaligned<ulong>(ref Unsafe.Add(ref bufferRef, valueLength - 16), Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref valueRef, valueLength - 16)));
                Unsafe.WriteUnaligned<ulong>(ref Unsafe.Add(ref bufferRef, valueLength - 8), Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref valueRef, valueLength - 8)));
#endif
            }
            else
            {
                Unsafe.WriteUnaligned<ulong>(ref bufferRef, Unsafe.ReadUnaligned<ulong>(ref valueRef));
                Unsafe.WriteUnaligned<ulong>(ref Unsafe.Add(ref bufferRef, valueLength - 8), Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref valueRef, valueLength - 8)));
            }
        }
        else if (valueLength >= 4)
        {
            Unsafe.WriteUnaligned<uint>(ref bufferRef, Unsafe.ReadUnaligned<uint>(ref valueRef));
            Unsafe.WriteUnaligned<uint>(ref Unsafe.Add(ref bufferRef, valueLength - 4), Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref valueRef, valueLength - 4)));
        }
        else if (valueLength >= 2)
        {
            Unsafe.WriteUnaligned<ushort>(ref Unsafe.Add(ref bufferRef, valueLength - 2),
                Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref valueRef, valueLength - 2)));
            bufferRef = valueRef;
        }
        else if (valueLength == 1)
        {
            bufferRef = valueRef;
        }
        else
        {
            Debug.Assert(valueLength == 0);
        }

#if DEBUG
        Debug.Assert(lastBufferChar == (lastBufferLength > 0 ? buffer[lastBufferLength - 1] : 0), $"Corrupted previous data: {lastBufferChar} -> {buffer[lastBufferLength - 1]}");
#endif

        if (indexFromEnd > -MinFreeBufferSpace)
            FlushBuffer();

        AssertInvariants();
    }

    public void Write(ReadOnlySpan<byte> value)
    {
        Debug.Assert(Utf8.IsValid(value), "Invalid UTF-8");
        if (value.Length <= 32)
        {
            WriteSmall(value);
            return;
        }

        var remaining = buffer.Length - bufferIndex;
        if (remaining >= value.Length)
        {
            value.CopyTo(buffer.AsSpan(bufferIndex));
            bufferIndex += value.Length;
            EnsureBufferFreeSpace();
            return;
        }

        if (stream is null)
        {
            EnlargeBuffer(value.Length + MinFreeBufferSpace);
            value.CopyTo(buffer.AsSpan(bufferIndex));
            bufferIndex += value.Length;
            EnsureBufferFreeSpace();
        }
        else
        {
            FlushBuffer();
            stream.Write(value);
        }
    }

    public void Write(ReadOnlySpan<char> value)
    {
        if (value.Length * Utf16CodeMaxLength <= BufferSize)
        {
            WriteAndTranscodeSingleBuffer(value);
        }
        else
        {
            WriteAndTranscodeLarge(value);
        }
        EnsureBufferFreeSpace();
    }
#if NET8_0_OR_GREATER
    public void WriteFormat<T>(T value, ReadOnlySpan<char> formatString, IFormatProvider? provider) where T : IUtf8SpanFormattable
    {
        int written;
        if (value.TryFormat(buffer.AsSpan(bufferIndex), out written, formatString, provider))
        {
            bufferIndex += written;
            EnsureBufferFreeSpace();
            return;
        }

        FlushBuffer();

        if (value.TryFormat(buffer, out written, formatString, provider))
        {
            bufferIndex += written;
            EnsureBufferFreeSpace();
            return;
        }

        WriteFormatVeryBig(value, formatString, provider);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void WriteFormatVeryBig<T>(T value, ReadOnlySpan<char> formatString, IFormatProvider? provider) where T : IUtf8SpanFormattable
    {
        // formatted value is larger than the buffer, let's try allocating a temporary larger one

        var bufferSize = this.buffer.Length * 2;
        while (true)
        {
            if (bufferSize > 2_000_000)
            {
                // this must be a bug
                throw new IndexOutOfRangeException("IUtf8SpanFormattable value produces too large output");
            }

            var tempBufer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                if (value.TryFormat(tempBufer, out var written, formatString, provider))
                {
                    Write(tempBufer.AsSpan(0, written));
                    EnsureBufferFreeSpace();
                    return;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tempBufer);
            }

            bufferSize = bufferSize * 2;
        }
    }
#endif

    // uint PackASCII(ulong utf16)
    // {
    //     // utf16 is 00_char4_00_char3_00_char2_00_char1
    //     var char13 = utf16 & 0x0000_00ff_0000_00ff; // 00_00_00_char3_00_00_00_char1
    //     var char24 = utf16 & 0x00ff_0000_00ff_0000; // 00_char4_00_00_00_char2_00_00
    //     var x = char13 | (char24 >> 8); // 00_00_char4_char3_00_00_char2_char1
    //     return (uint)x | (uint)(x >> 16);
    // }

    // private void WriteCharSmall(ReadOnlySpan<char> value)
    // {
    //     most likely it is ASCII, we will check that and fallback to transcoding if needed
    //     var valueLength = value.Length;
    //     Debug.Assert(valueLength <= 32);
    //     if (valueLength + bufferIndex > buffer.Length)
    //     {
    //         FlushBuffer();
    //     }
    //     ref byte bufferRef = ref BufferRef();
    //     ref char valueRef = ref MemoryMarshal.GetReference(value);
    //     ref byte valueUtf16ByteRef = ref Unsafe.As<char, byte>(ref valueRef);
    //     ulong mask = 0xff80ff80ff80ff80;
    //     while (valueLength >= 16)
    //     {
    //         var a = Unsafe.ReadUnaligned<ulong>(ref valueUtf16ByteRef);
    //         var b = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref valueUtf16ByteRef, 8));
    //         var c = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref valueUtf16ByteRef, 16));
    //         var d = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref valueUtf16ByteRef, 24));
    //         Unsafe.WriteUnaligned<uint>(ref bufferRef, PackASCII(a));
    //         Unsafe.WriteUnaligned<uint>(ref Unsafe.Add(ref bufferRef, 4), PackASCII(b));
    //         Unsafe.WriteUnaligned<uint>(ref Unsafe.Add(ref bufferRef, 8), PackASCII(c));
    //         Unsafe.WriteUnaligned<uint>(ref Unsafe.Add(ref bufferRef, 12), PackASCII(d));
    //         if (((a | b | c | d) & mask) != 0)
    //         {
    //             goto Transcode;
    //         }

    //         bufferRef = ref Unsafe.Add(ref bufferRef, 16);
    //         valueRef = ref Unsafe.Add(ref valueRef, 16);
    //         valueLength = valueLength - 16;
    //     }
    //     if (valueLength >= 8)
    //     {
    //         var a = Unsafe.ReadUnaligned<ulong>(ref valueUtf16ByteRef);
    //         var b = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref valueUtf16ByteRef, 8));
    //         Unsafe.WriteUnaligned<uint>(ref bufferRef, PackASCII(a));
    //         Unsafe.WriteUnaligned<uint>(ref Unsafe.Add(ref bufferRef, 4), PackASCII(b));
    //         if (((a | b) & mask) != 0)
    //         {
    //             goto Transcode;
    //         }
    //         bufferRef = ref Unsafe.Add(ref bufferRef, 8);
    //         valueRef = ref Unsafe.Add(ref valueRef, 8);
    //         valueLength = valueLength - 8;
    //     }
    //     bool mustTranscode = false;
    //     char ch;
    //     switch (valueLength)
    //     {
    //         case 7:
    //             ch = Unsafe.Add(ref valueRef, 6);
    //             Unsafe.Add(ref bufferRef, 6) = (byte)ch;
    //             mustTranscode |= ch >= 0x80;
    //             goto case 6;
    //         case 6:
    //             ch = Unsafe.Add(ref valueRef, 5);
    //             Unsafe.Add(ref bufferRef, 5) = (byte)ch;
    //             mustTranscode |= ch >= 0x80;
    //             goto case 5;
    //         case 5:
    //             ch = Unsafe.Add(ref valueRef, 4);
    //             Unsafe.Add(ref bufferRef, 4) = (byte)ch;
    //             mustTranscode |= ch >= 0x80;
    //             goto case 4;
    //         case 4: {
    //             var quadChar = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref valueUtf16ByteRef, 8));
    //             Unsafe.WriteUnaligned<uint>(ref bufferRef, PackASCII(quadChar));
    //             mustTranscode |= (quadChar & mask) != 0;
    //             break;
    //         }
    //         case 3:
    //             ch = Unsafe.Add(ref valueRef, 2);
    //             Unsafe.Add(ref bufferRef, 2) = (byte)ch;
    //             mustTranscode |= ch >= 0x80;
    //             goto case 2;
    //         case 2:
    //             ch = Unsafe.Add(ref valueRef, 1);
    //             Unsafe.Add(ref bufferRef, 1) = (byte)ch;
    //             Unsafe.Add(ref bufferRef, 3) = (byte)ch;
    //             mustTranscode |= ch >= 0x80;
    //             goto case 1;
    //         case 1:
    //             ch = Unsafe.Add(ref valueRef, 0);
    //             Unsafe.Add(ref bufferRef, 0) = (byte)ch;
    //             mustTranscode |= ch >= 0x80;
    //             break;
    //         case 0:
    //             break;
    //         default:
    //             ThrowUnexpected();
    //             break;
    //     }
    //     if (!mustTranscode)
    //     {
    //         bufferIndex += valueLength;
    //         return;
    //     }
    //     Transcode:
    //     WriteAndTranscodeSingleBuffer(value);
    // }

    private void WriteAndTranscodeSingleBuffer(ReadOnlySpan<char> value)
    {
        // TODO: .NET Framework - https://source.dot.net/#System.Text.Json/System/Text/Json/Writer/JsonWriterHelper.cs,277
        var block1Result = Utf8.FromUtf16(value, BufferRestSpan(), out var block1chars, out var block1bytes, replaceInvalidSequences: true);
        bufferIndex += block1bytes;

        if (block1Result == OperationStatus.Done)
        {
            Debug.Assert(block1chars == value.Length);
            return;
        }

        if (block1Result != OperationStatus.DestinationTooSmall)
            ThrowUnexpected();

        FlushBuffer();
        var block2Result = Utf8.FromUtf16(value.Slice(block1chars), BufferRestSpan(), out var block2chars, out var block2bytes, replaceInvalidSequences: true);
        if (block2Result != OperationStatus.Done)
            ThrowUnexpected();
        bufferIndex += block2bytes;
        Debug.Assert(value.Length == block1chars + block2chars);
    }

    private void WriteAndTranscodeLarge(ReadOnlySpan<char> value)
    {
        OperationStatus result;
        int valueIndex = 0;

        do
        {
            result = Utf8.FromUtf16(value.Slice(valueIndex), BufferRestSpan(), out var charsRead, out var bytesWritten, replaceInvalidSequences: true);
            valueIndex += charsRead;
            bufferIndex += bytesWritten;
            if (result == OperationStatus.DestinationTooSmall)
            {
                FlushBuffer();
            }
        }
        while (result == OperationStatus.DestinationTooSmall);

        if (result != OperationStatus.Done)
            ThrowUnexpected();
    }

    // IBufferWriter<byte> compatibility
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (sizeHint + bufferIndex > buffer.Length)
        {
            FlushBuffer();
            if (sizeHint + bufferIndex > buffer.Length)
            {
                EnlargeBuffer(sizeHint + bufferIndex);
            }
        }
        return buffer.AsMemory(startIndex: bufferIndex);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (sizeHint + bufferIndex <= buffer.Length)
            return BufferRestSpan();
        return GetSpanWithResize(sizeHint);
    }
    private Span<byte> GetSpanWithResize(int sizeHint)
    {
        FlushBuffer();
        if (sizeHint > buffer.Length)
        {
            EnlargeBuffer(sizeHint + MinFreeBufferSpace);
        }
        return buffer.AsSpan(startIndex: bufferIndex);
    }
    public void Advance(int count)
    {
        bufferIndex += count;
        EnsureBufferFreeSpace();
    }


    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]
    static void ThrowUnexpected()
    {
        throw new InvalidOperationException("Internal bug in Utf8StringWriter");
    }

    [Conditional("DEBUG")]
    public void AssertInvariants()
    {
        Debug.Assert(buffer is not null, "buffer is null (disposed or not initialized)");
        Debug.Assert(bufferIndex >= 0 && bufferIndex + MinFreeBufferSpace <= buffer.Length, $"bufferIndex={bufferIndex}, buffer.Length={buffer.Length}, free={buffer.Length - bufferIndex}");

        if (!PendingBytes.IsEmpty)
        {
            // find first byte which is not a UTF-8 continuation
            // see https://en.wikipedia.org/wiki/UTF-8#Byte_map
            var firstCharByte = stream is null ? 0 : PendingBytes.IndexOfAnyExceptInRange((byte)0x80, (byte)0xBF);

            if (firstCharByte < 0)
            {
                Debug.Assert(PendingBytes.Length <= 4, "PendingBytes contains more than 4 bytes, but all of them are continuation bytes");
                return;
            }

            Debug.Assert(firstCharByte <= 3, "First 4 bytes are all continuation bytes:\n" + StringUtils.HexDump(PendingBytes.Slice(0, firstCharByte + 1)));

            if (!Utf8.IsValid(PendingBytes.Slice(firstCharByte)))
            {
                Debug.Assert(false, "PendingBytes is not valid UTF-8:\n" + StringUtils.HexDump(PendingBytes));
            }
        }
    }

    public override string ToString()
    {
        var utf16 = new char[PendingBytes.Length];
        var status = Utf8.ToUtf16(PendingBytes, utf16, out _, out var charsWritten, replaceInvalidSequences: true);

        return $"Utf8Writer({stream?.ToString() ?? "buffer all"}, {new string(utf16, 0, charsWritten)})";
    }

    public void Dispose()
    {
        if (buffer is null)
            return; // already disposed

        FlushBuffer();
        if (!leaveStreamOpen)
        {
            stream?.Dispose();
        }

        ArrayPool<byte>.Shared.Return(buffer);
        this.buffer = null!;
    }
}
