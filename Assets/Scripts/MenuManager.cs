using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class MenuManager : MonoBehaviour
{
    NetworkManager networkManager;
    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    public void StartHost()
    {
        Debug.Log("Starting as Host");

        // Start as the host
        networkManager.StartHost();
    }

    public void StartClient()
    {
        Debug.Log("Starting as Client");

        // Start as the client
        networkManager.StartClient();
    }
}