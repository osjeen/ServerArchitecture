using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class UdpServer
{
    static async Task Main(string[] args)
    {
        int port = 5000;

        if (args.Length >= 1)
            int.TryParse(args[0], out port);

        try
        {
            await StartUdpServer(port);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 에러: {ex.Message}");
        }
    }

    static async Task StartUdpServer(int port)
    {
        using (UdpClient udpServer = new UdpClient(port))
        {
            Console.WriteLine($"🚀 UDP 서버 시작됨");
            Console.WriteLine($"   포트: {port}");
            Console.WriteLine($"   대기 중... (Ctrl+C로 종료)\n");

            try
            {
                while (true)
                {
                    // 요청 대기
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedData = udpServer.Receive(ref remoteEndPoint);
                    string receivedMessage = Encoding.UTF8.GetString(receivedData);

                    Console.WriteLine($"📥 요청 수신!");
                    Console.WriteLine($"   발신자: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                    Console.WriteLine($"   메시지: {receivedMessage}");
                    Console.WriteLine($"   시간: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

                    // 응답 전송
                    string responseMessage = $"Echo: {receivedMessage}";
                    byte[] responseData = Encoding.UTF8.GetBytes(responseMessage);
                    udpServer.Send(responseData, responseData.Length, remoteEndPoint);

                    Console.WriteLine($"📤 응답 전송: {responseMessage}\n");
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"❌ 소켓 에러: {ex.Message}");
            }
        }
    }
}
