using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class UnityProcess
{
    private const string relay_serverIp = "127.0.0.1";
    private const int port = 5004;

    // 반환 타입을 Task<byte[]>로 변경하여 await 가능하게 함
    public static async Task<byte[]> SendTCPQuery(byte[] data)
    {
        try
        {
            // 1. 비동기 연결 (ConnectAsync)
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(relay_serverIp, port);
                
                using (NetworkStream stream = client.GetStream())
                {
                    // 2. 서버로 데이터 전송 (WriteAsync)
                    await stream.WriteAsync(data, 0, data.Length);

                    // 3. 서버 응답 읽기 (ReadAsync)
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        // 실제 받은 크기만큼만 복사해서 반환
                        byte[] response = new byte[bytesRead];
                        Array.Copy(buffer, 0, response, 0, bytesRead);
                        return response;
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"TCP 통신 에러: {e.Message}");
        }

        return null; // 실패 시 null 반환
    }
}