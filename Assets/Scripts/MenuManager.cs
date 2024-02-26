using UnityEngine;
using Unity.Netcode;

public class MenuManager : MonoBehaviour
{
    NetworkManager networkManager;
    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    public void StartHost()
    {
        //Debug.Log("Starting as Host");
        //Start host through networkManager
        networkManager.StartHost();
    }

    public void StartClient()
    {
        //Debug.Log("Starting as Client");
        //Start client through networkManager
        networkManager.StartClient();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
