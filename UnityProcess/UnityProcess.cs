using System;
using System.Net.Sockets;
using System.Text;

class UnityProcess
{
    static void Main()
    {
        string serverIp = "127.0.0.1";
        int port = 5004;

        try
        {
            // 1. 중계기(5004)에 연결 시도
            using (TcpClient client = new TcpClient(serverIp, port))
            {
                Console.WriteLine($"중계기({port})에 연결되었습니다.");

                // 2. 스트림 가져오기 (데이터 읽기/쓰기용)
                NetworkStream stream = client.GetStream();

                // 3. 서버(중계기)로 메시지 전송
                string message = "내부 앱에서 보내는 요청입니다!";
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Console.WriteLine("데이터 전송 완료.");

                // 4. 서버(중계기)로부터 답변 읽기
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"서버 답변: {response}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"에러 발생: {e.Message}");
        }
    }
}