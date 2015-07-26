using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace DotVVM.Framework.Storage
{
    public class FileSystemReturnedFileStorage : IReturnedFileStorage, IDisposable
    {
        /// <summary>
        /// Array of characters which can be used in generated file ID.
        /// </summary>
        public static readonly char[] AvailableCharacters =
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'
        };

        /// <summary>
        /// Lenght of generated file ID.
        /// </summary>
        public static readonly int FileIdLength = 32;

        /// <summary>
        /// Temp directory where are files stored.
        /// </summary>
        public string TempDirectory { get; private set; }

        /// <summary>
        /// Interval in which will be old files deleted.
        /// </summary>
        public TimeSpan AutoDeleteInterval { get; private set; }

        /// <summary>
        /// Timer for deleting old files.
        /// </summary>
        private readonly Timer _autoDeleteTimer;


        /// <summary>
        /// Initializes new instance of <see cref="FileSystemReturnedFileStorage"/> class with default interval for deleting old files.
        /// </summary>
        /// <param name="directory">Temp directory for storing files.</param>
        public FileSystemReturnedFileStorage(string directory) : this(directory, new TimeSpan(0, 5, 0)) { }

        /// <summary>
        /// Initializes new instance of <see cref="FileSystemReturnedFileStorage"/> class.
        /// </summary>
        /// <param name="directory">Temp directory for storing files.</param>
        /// <param name="autoDeleteInterval">Interval for deleting old files.</param>
        public FileSystemReturnedFileStorage(string directory, TimeSpan autoDeleteInterval)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("Directory can not be null or empty.");
            }

            TempDirectory = directory;
            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }

            AutoDeleteInterval = autoDeleteInterval;

            _autoDeleteTimer = new Timer(state => DeleteOldFiles(DateTime.Now - AutoDeleteInterval), null, AutoDeleteInterval, AutoDeleteInterval);
        }

        public string GenerateFileId()
        {
            var sb = new StringBuilder();
            var randomData = new byte[FileIdLength];

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomData);
            }

            for (var i = 0; i < FileIdLength; ++i)
            {
                var n = randomData[i] % AvailableCharacters.Length;
                sb.Append(AvailableCharacters[n]);
            }

            return sb.ToString();
        }

        public void StoreFile(string id, byte[] bytes, string fileName, string mimeType, IHeaderDictionary additionalHeaders)
        {
            using (FileStream fs = new FileStream(TempDirectory + Path.DirectorySeparatorChar + id + ".data", FileMode.Create),
                    fs2 = new FileStream(TempDirectory + Path.DirectorySeparatorChar + id + ".metadata", FileMode.Create))
            using (var sw2 = new StreamWriter(fs2))
            {
                fs.Write(bytes, 0, bytes.Length);

                var jsonFileName = JsonConvert.SerializeObject(fileName);
                var jsonMimeType = JsonConvert.SerializeObject(mimeType);
                var jsonAdditionalHeaders = JsonConvert.SerializeObject(additionalHeaders);

                sw2.WriteLine(jsonFileName);
                sw2.WriteLine(jsonMimeType);
                sw2.WriteLine(jsonAdditionalHeaders);
            }
        }

        public void StoreFile(string id, Stream stream, string fileName, string mimeType, IHeaderDictionary additionalHeaders)
        {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            StoreFile(id, bytes, fileName, mimeType, additionalHeaders);
        }

        public Stream GetFile(string id, out string fileName, out string mimeType, out IHeaderDictionary additionalHeaders)
        {
            Stream stream = new FileStream(TempDirectory + Path.DirectorySeparatorChar + id + ".data", FileMode.Open);

            using (var fs = new FileStream(TempDirectory + Path.DirectorySeparatorChar + id + ".metadata", FileMode.Open))
            using(var sr = new StreamReader(fs))
            {
                fileName = JsonConvert.DeserializeObject<string>(sr.ReadLine());
                mimeType = JsonConvert.DeserializeObject<string>(sr.ReadLine());
                additionalHeaders = JsonConvert.DeserializeObject<HeaderDictionary>(sr.ReadLine());
            }

            return stream;
        }

        public void DeleteFile(string id)
        {
            File.Delete(TempDirectory + Path.DirectorySeparatorChar + id + ".data");
            File.Delete(TempDirectory + Path.DirectorySeparatorChar + id + ".metadata");
        }

        public void DeleteOldFiles(DateTime maxCreatedDate)
        {
            var files = Directory.GetFiles(TempDirectory).Where(t => File.GetCreationTime(t) < maxCreatedDate);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        public void Dispose()
        {
            _autoDeleteTimer?.Dispose();
        }
    }
}