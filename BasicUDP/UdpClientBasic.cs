using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
//넷코드를 저수준(udp)부터 다루는 기본코드. 보안수준 낮음

namespace UnityUdpNetCode{

public class UdpClientBasic
{
    static void Main()
    {
        /*
        // 설정
        string targetIP = "JOINTSKILui-MacBookAir.local";      // 대상 IP 주소
        int targetPort = 5000;                  // 대상 포트
        string message = "Hello UDP Server";    // 보낼 메시지

        // 명령줄 인자 처리
        if (args.Length >= 1)
            targetIP = args[0];
        if (args.Length >= 2)
            int.TryParse(args[1], out targetPort);
        if (args.Length >= 3)
            message = args[2];

        try
        {
            SendUdpMessage(targetIP, targetPort, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"에러 발생: {ex.Message}");
        }*/
    }
    static string config_path=Path.Combine(AppContext.BaseDirectory, "config.json");
    static void SendSocket(byte payload)
    {
        string json=File.ReadAllText(config_path);
        var config=JsonConvert.DeserializeObject<Config>(json);

        try
        {
            SendUdpPayload(config.targetIP, config.targetPort, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"error: {ex.Message}");
        }
    }

    static void SendUdpPayload(string ipAddress, int port, byte payload)
    {
        
        using (UdpClient udpClient = new UdpClient())
        {
            try
            {
                udpClient.Send(payload, ipAddress, port);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"socket error: {ex.Message}");
            }
        }

    }
}

}