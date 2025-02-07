﻿using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting
{
    public interface IHttpResponse
    {
        IHeaderCollection Headers { get; }
        IHttpContext Context { get; }
        int StatusCode { get; set; }
        string? ContentType { get; set; }
        Stream Body { get; set; }
        void Write(string text);
        void Write(ReadOnlyMemory<char> text);
        void Write(ReadOnlyMemory<byte> data);
        Task WriteAsync(string text, CancellationToken token = default);
        Task WriteAsync(ReadOnlyMemory<char> text, CancellationToken token = default);
        Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken token = default);
    }
}
