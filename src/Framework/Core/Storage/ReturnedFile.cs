using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Core.Storage
{
    public class ReturnedFile
    {

        public Stream Stream { get; }
        public ReturnedFileMetadata Metadata { get; }

        public ReturnedFile(Stream stream, ReturnedFileMetadata metadata)
        {
            Stream = stream;
            Metadata = metadata;
        }

    }
}
