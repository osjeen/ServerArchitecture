using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Drawing;
//넷코드를 저수준(udp)부터 다루는 기본코드. 보안수준 낮음

namespace UnityUdpNetCode{

public class UdpClientBasic
{
    static string config_path=Path.Combine(Application.streamingAssetsPath, "config.json");
    public static void SendSocket(byte[] payload)
    {
        string json=File.ReadAllText(config_path);
        var config=JsonConvert.DeserializeObject<Config>(json);

        try
        {
            Debug.Log("Success");
            SendUdpPayload(config.targetIP, config.targetPort, payload);
        }
        catch (Exception ex)
        {
            Debug.Log($"error: {ex.Message}");
        }
    }

    static void SendUdpPayload(string ipAddress, int port, byte[] payload)
    {
        using (UdpClient udpClient = new UdpClient())
        {
            try
            {
                udpClient.Send(payload,payload.Length,ipAddress,port);
            }
            catch (SocketException ex)
            {
                Debug.Log($"socket error: {ex.Message}");
            }
        }

    }
    static void StartUdpClient(int port)
    {
        using (UdpClient udpServer = new UdpClient(port))
        {
            Console.WriteLine($"udp client started");
            Console.WriteLine($"port: {port}");
            Console.WriteLine($"waiting..(Ctrl+C to exit)\n");

            try
            {
                while (true)
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedData = udpServer.Receive(ref remoteEndPoint);

                    DataPackType dp_type=(DataPackType)receivedData[0];

                    ExampleDataTarget target= 
                    Console.WriteLine($"responded");
                    Console.WriteLine($"   sender: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                    Console.WriteLine($"   time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

                    //respond
                    /*
                    if(respond!=null){
                        udpServer.Send(respond, respond.Length, remoteEndPoint);
                        Console.WriteLine($"respond: {respond}\n");
                    }
                    */
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"socket error: {ex.Message}");
            }
        }
    }
}

}