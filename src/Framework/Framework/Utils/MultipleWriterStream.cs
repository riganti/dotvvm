// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;

// namespace DotVVM.Framework.Utils
// {
//     internal class MultipleWriterStream : Stream
//     {
//         private readonly Stream[] innerStreams;
//         private readonly bool parallel;
//         private long position;
//         private Task?[] tasks;

//         public MultipleWriterStream(IEnumerable<Stream> innerStreams, bool parallel)
//         {
//             this.innerStreams = innerStreams.ToArray();
//             if (this.innerStreams.Length > 512)
//                 throw new ArgumentException("Too many output streams.");
//             this.tasks = new Task?[this.innerStreams.Length];
//             this.parallel = parallel;
//             foreach (var stream in this.innerStreams)
//             {
//                 if (!stream.CanWrite)
//                     throw new ArgumentException("All streams must be writable.");
//             }
//         }

//         public override bool CanRead => false;

//         public override bool CanSeek => false;

//         public override bool CanWrite => true;

//         public override long Length => position;

//         public override long Position { get => position; set => throw new NotSupportedException(); }

//         public override void Flush()
//         {
//             foreach (var stream in innerStreams)
//                 stream.Flush();
//         }
//         public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
//         public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
//         public override void SetLength(long value) { }
//         public override void Write(byte[] buffer, int offset, int count)
//         {
//             foreach (var stream in innerStreams)
//                 stream.Write(buffer, offset, count);
//         }

//         public override void Write(ReadOnlySpan<byte> buffer)
//         {
//             foreach (var stream in innerStreams)
//                 stream.Write(buffer);
//             position += buffer.Length;
//         }

//         public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//         {
//             if (parallel)
//             {
//                 bool done = true;
//                 for (int i = 0; i < innerStreams.Length; i++)
//                 {
//                     tasks[i] = innerStreams[i].WriteAsync(buffer, offset, count, cancellationToken);
//                     if (!tasks[i]!.IsCompleted)
//                         done = false;
//                 }
//                 return done ? Task.CompletedTask : Task.WhenAll(tasks);
//             }
//             else
//             {
//                 for (int i = 0; i < innerStreams.Length; i++)
//                 {
//                     var task = innerStreams[i].WriteAsync(buffer, offset, count, cancellationToken);
//                     if (!task.IsCompleted)
//                     {
//                         if (i == innerStreams.Length - 1)
//                             return task;
//                         else
//                             return SerialAsyncCore(task, i + 1, buffer, offset, count, cancellationToken);
//                     }
//                 }
//                 return Task.CompletedTask;
//             }

//             async Task SerialAsyncCore(Task firstResult, int startIndex, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
//             {
//                 await firstResult;
//                 for (int i = startIndex; i < innerStreams.Length; i++)
//                     await innerStreams[i].WriteAsync(buffer, offset, count, cancellationToken);
//                 position += count;
//             }
//         }

//         public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
//         {
//             if (!parallel)
//             {
//                 return SerialCore(buffer, cancellationToken);
//             }
//             else
//             {
//                 bool done = true;
//                 for (int i = 0; i < innerStreams.Length; i++)
//                 {
//                     var task = innerStreams[i].WriteAsync(buffer, cancellationToken);
//                     if (task.IsCompleted)
//                         tasks[i] = Task.CompletedTask;
//                     else
//                     {
//                         tasks[i] = task.AsTask();
//                         done = false;
//                     }
//                 }
//                 return done ? default : new ValueTask(Task.WhenAll(tasks));
//             }


//             async ValueTask SerialCore(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
//             {
//                 foreach (var stream in innerStreams)
//                     await stream.WriteAsync(buffer, cancellationToken);

//                 position += buffer.Length;
//             }
//         }
//     }
// }
