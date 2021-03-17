using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AutoHostClient : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;

    void Start()
    {
        if(!Application.isBatchMode) // Headless build
        {
            Debug.Log($"=== Client connected ===");
            networkManager.StartClient();
        }
        else
        {
            Debug.Log($"=== Server starting ===");
        }
    }

    public void JoinLocal()
    {
        networkManager.networkAddress = "localhost";
        networkManager.StartClient();
    }
}
