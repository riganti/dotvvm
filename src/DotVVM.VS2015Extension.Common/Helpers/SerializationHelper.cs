using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace DotVVM.VS2015Extension.Common.Helpers
{
    internal static class SerializationHelper
    {
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T ByteArrayToObject<T>(byte[] bytes)
        {
            if (bytes == null)
                return default(T);

            var bf = new BinaryFormatter();
            using (var stream = new MemoryStream(bytes))
            {
                return (T)bf.Deserialize(stream);
            }
        }
    }
}