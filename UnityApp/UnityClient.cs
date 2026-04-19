using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LLNetCode
{

public class UnityNetClient
{
    private const string mainServerIP = "127.0.0.1";
    private const int tcp_port = 5001;

    /*TCP SOCKET*/

    public static async Task<byte[]> SendTCPQuery(byte[] data)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(mainServerIP, tcp_port);
                
                using (NetworkStream stream = client.GetStream())
                {
                    await stream.WriteAsync(data, 0, data.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        byte[] response = new byte[bytesRead];
                        Array.Copy(buffer, 0, response, 0, bytesRead);
                        return response;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"TCP 통신 에러: {e.Message}");
        }
        return null; 
    }

    //division path->client id
    public static async Task<int> AssignClient(string division_path)
    {
        ClientAssign_PacketData data = new ClientAssign_PacketData();
        data.path = division_path;

        byte[] response = await SendTCPQuery(QueryProtocol.Serialize(data));

        if (response == null || response.Length < 4)
        {
            Debug.LogError("서버로부터 유효하지 않은 응답을 받았습니다.");
            return -1; 
        }

        return ResponseProtocol.Deserialize<int>(response);
    }

    //division path->client datas
    public static async Task<ClientMap> GetDivisionClients(string path)
    {
        
        DivisionClients_PacketData data = new DivisionClients_PacketData();
        data.path = path;

        // 2. 직렬화 및 TCP 요청 (UnityProcess는 비동기로 구현되어 있어야 함)
        byte[] response = await SendTCPQuery(QueryProtocol.Serialize(data));

        // 3. 예외 처리: 데이터가 비어있거나 부족할 경우
        if (response == null || response.Length < 4)
        {
            UnityEngine.Debug.LogError("서버로부터 유효하지 않은 응답을 받았습니다.");
            return null; 
        }

        return ResponseProtocol.Deserialize<ClientMap>(response);
    }
}

}