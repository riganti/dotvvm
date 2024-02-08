using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    internal class LimitLengthStream : Stream
    {
        private readonly Stream innerStream;
        private readonly long maxLength;
        private readonly string comment;
        private long position;

        public LimitLengthStream(Stream innerStream, long maxLength, string errorComment)
        {
            this.innerStream = innerStream;
            this.maxLength = maxLength;
            this.comment = errorComment;
        }

        private void MovePosition(long offset)
        {
            position += offset;
            if (position > maxLength)
                throw new InvalidOperationException($"The stream is limited to {maxLength} bytes: {comment}");
        }

        public long RemainingAllowedLength => maxLength - position;


        public override bool CanRead => innerStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => innerStream.Length;

        public override long Position
        {
            get => innerStream.Position;
            set => throw new NotImplementedException();
        }

        public override void Flush() => innerStream.Flush();
        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = innerStream.Read(buffer, offset, (int)Math.Min(count, RemainingAllowedLength + 1));
            MovePosition(read);
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await innerStream.ReadAsync(buffer, offset, (int)Math.Min(count, RemainingAllowedLength + 1), cancellationToken);
            MovePosition(read);
            return read;
        }
#if DotNetCore
        public override int Read(Span<byte> buffer)
        {
            var read = innerStream.Read(buffer.Slice(0, (int)Math.Min(buffer.Length, RemainingAllowedLength + 1)));
            MovePosition(read);
            return read;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = await innerStream.ReadAsync(buffer.Slice(0, (int)Math.Min(buffer.Length, RemainingAllowedLength + 1)), cancellationToken);
            MovePosition(read);
            return read;
        }
#endif
        public override long Seek(long offset, SeekOrigin origin) => throw new System.NotImplementedException();
        public override void SetLength(long value) => throw new System.NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new System.NotImplementedException();

        public static Stream LimitLength(Stream s, long maxLength, string errorComment)
        {
            if (maxLength < 0 || maxLength == long.MaxValue)
                return s;

            if (s.CanSeek)
            {
                if (s.Length > maxLength)
                    throw new InvalidOperationException($"The stream is limited to {maxLength} bytes: {errorComment}");
                return s;
            }
            else
            {
                return new LimitLengthStream(s, maxLength, errorComment);
            }
        }
    }
}
