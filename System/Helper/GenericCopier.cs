using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace FluentChange.Extensions.System.Helper
{
    public static class GenericCopier<T>
    {
        public static T DeepCopy(object objectToCopy)
        {
            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    //BinaryFormatter binaryFormatter = new BinaryFormatter();
            //    //binaryFormatter.Serialize(memoryStream, objectToCopy);
            //    //memoryStream.Seek(0, SeekOrigin.Begin);
            //    //return (T)binaryFormatter.Deserialize(memoryStream);

                
            //}
            var serialized = JsonSerializer.Serialize(objectToCopy);
            return JsonSerializer.Deserialize<T>(serialized);
        }
    }
}
