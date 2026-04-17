using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClient:MonoBehaviour
{
    public void StartClient()
    {
        StartCoroutine(Cor_StartClient());
    }
    IEnumerator Cor_StartClient()
    {
        Debug.Log("Starting Client..");

        var task = UnityTCPQuery.AssignClient("Ch1/R1");

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
        }
    }
}