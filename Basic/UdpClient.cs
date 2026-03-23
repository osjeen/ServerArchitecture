using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UdpClientProgram
{
    static void Main(string[] args)
    {
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
        }
    }

    static void SendUdpMessage(string ipAddress, int port, string message)
    {
        using (UdpClient udpClient = new UdpClient())
        {
            try
            {
                // UDP 메시지 전송
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, ipAddress, port);
                
                Console.WriteLine($"✅ UDP 메시지 전송 성공!");
                Console.WriteLine($"   대상 IP: {ipAddress}");
                Console.WriteLine($"   포트: {port}");
                Console.WriteLine($"   메시지: {message}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"❌ 소켓 에러: {ex.Message}");
            }
        }
    }
}
