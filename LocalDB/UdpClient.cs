using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class UdpClient
{
    static async Task Main(string[] args)
    {
        // 설정
        string targetIP = "JOINTSKILui-MacBookAir.local";
        int targetPort = 5000;
        string message = "Hello UDP Server";
        int timeoutMs = 5000;  // 5초 타임아웃

        if (args.Length >= 1)
            targetIP = args[0];
        if (args.Length >= 2)
            int.TryParse(args[1], out targetPort);
        if (args.Length >= 3)
            message = args[2];

        try
        {
            // 응답 받기 포함
            await SendAndReceiveUdpMessage(targetIP, targetPort, message, timeoutMs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 에러: {ex.Message}");
        }
    }

    static async Task SendAndReceiveUdpMessage(string ipAddress, int port, string message, int timeout)
    {
        using (UdpClient udpClient = new UdpClient())
        {
            try
            {
                // 타임아웃 설정
                udpClient.Client.ReceiveTimeout = timeout;

                // 메시지 전송
                byte[] data = Encoding.UTF8.GetBytes(message);
                Console.WriteLine($"📤 메시지 전송 중...");
                Console.WriteLine($"   IP: {ipAddress}, Port: {port}");
                Console.WriteLine($"   내용: {message}");
                
                udpClient.Send(data, data.Length, ipAddress, port);
                Console.WriteLine($"✅ 메시지 전송 완료");

                // 응답 대기
                Console.WriteLine($"\n⏳ 응답 대기 중... ({timeout}ms)");
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                
                try
                {
                    byte[] receiveData = udpClient.Receive(ref remoteEndPoint);
                    string responseMessage = Encoding.UTF8.GetString(receiveData);
                    
                    Console.WriteLine($"\n✅ 응답 수신 완료!");
                    Console.WriteLine($"   발신자: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                    Console.WriteLine($"   응답: {responseMessage}");
                }
                catch (SocketException)
                {
                    Console.WriteLine($"⚠️  응답 시간 초과 (응답 없음)");
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"❌ 소켓 에러: {ex.Message}");
            }
        }
    }
}