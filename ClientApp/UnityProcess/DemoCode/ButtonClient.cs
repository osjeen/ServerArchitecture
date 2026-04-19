using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNetCode;

public class ButtonClient:MonoBehaviour
{
    public void StartClient()
    {
        StartCoroutine(Cor_StartClient());
    }
    IEnumerator Cor_StartClient()
    {
        Debug.Log("Starting Client..");

        var task = UnityNetClient.AssignClient("Ch1/R1");

        while (!task.IsCompleted)
        {
            yield return null; 
        }

        if (task.IsFaulted) 
        {
            Debug.LogError($"에러: {task.Exception.InnerException.Message}");
        }
        else
        {
            int clientId = task.Result; 
            Debug.Log($"id:{clientId}");
            PlayerPrefs.SetInt("clientID",clientId);//테스트용
        }
    }


    public void GetClients()
    {
        StartCoroutine(Cor_GetClients());
    }

    IEnumerator Cor_GetClients()
    {
        Debug.Log("GetClient..");

        var task = UnityNetClient.GetDivisionClients("00");//테스트용. 실제론 이 클라의 path를 파라미터로 넣어주ㅜ야됨

        while (!task.IsCompleted)
        {
            yield return null; 
        }

        if (task.IsFaulted) 
        {
            Debug.LogError($"에러: {task.Exception.InnerException.Message}");
        }
        else
        {
            ClientMap res = task.Result; 
            Debug.Log(res.client_structures.Count);
            int id=PlayerPrefs.GetInt("clientID");
            foreach (var item in res.client_structures)
            {
                if(item.uuid==id)
                Debug.Log("found me");
            }
        }
    }
    UnityNetClient unityNetClient;
    void Start()
    {
        unityNetClient=new UnityNetClient();
        input_field.onSubmit.AddListener(SendMsg);
    }
    [SerializeField]TMPro.TMP_InputField input_field;
    void SendMsg(string msg)
    {
        UnityNetClient.SendMsg(msg);
    }

}