using System.Buffers.Binary;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Net;           // IPAddress.NetworkToHostOrder (엔디안 변환)
using System.Net.Sockets;    // TcpClient, NetworkStream
using System.Threading.Tasks; // Task, await 사용
using UnityEngine;
public class ResponseProtocol
{
    public static T Deserialize<T>(byte[] target,bool useJson=true)
    {
        if(typeof(T) == typeof(int))
        {
            return (T)(object)BinaryPrimitives.ReadInt32LittleEndian(target);
        }else if(useJson)
        {
            int rawLength = BitConverter.ToInt32(target, 0);
            int bodyLength = IPAddress.NetworkToHostOrder(rawLength);
            string json = Encoding.UTF8.GetString(target, 4, bodyLength);

            T data = JsonConvert.DeserializeObject<T>(json);
            return (T)(object)data;
        }
        else
        {
            //단순 string인경우s
            int rawLength = BitConverter.ToInt32(target, 0);
            int bodyLength = IPAddress.NetworkToHostOrder(rawLength);
            string str = Encoding.UTF8.GetString(target, 4, bodyLength);
            return (T)(object)str;
        }
    }
}