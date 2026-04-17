using System.Buffers.Binary;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
public class ResponseProtocol
{
    public static T Deserialize<T>(byte[] target)
    {
        if(typeof(T) == typeof(int))
        {
            return (T)(object)BinaryPrimitives.ReadInt32BigEndian(target);
        }else
        {
            string jsonString = Encoding.UTF8.GetString(target);
        
            T result = JsonConvert.DeserializeObject<T>(jsonString);
            return result;
        }
    }
}