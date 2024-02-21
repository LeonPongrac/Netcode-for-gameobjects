using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FloorManager : NetworkBehaviour
{
    private MeshRenderer floorRenderer;
    private bool active = false;
    public string playerName;

    void Start()
    {
        floorRenderer = GetComponent<MeshRenderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && active)
        {
            //Debug.Log("collided with player");
            MeshRenderer playerRenderer = other.GetComponent<MeshRenderer>();
            string player = other.GetComponent<PlayerManager>().playerName;

            Debug.Log($"Player {player} has the cube");

            if (playerRenderer != null)
            {
                ColorFloorRpc(playerRenderer.material.color, player);
            }
            else
            {
                Debug.LogWarning("Player object does not have a MeshRenderer component.");
            }
        }
    }

    public void ActivateFloor()
    {
        active = true;
    }

    public void DeActivateFloor()
    {
        active = false;
    }

    [Rpc(SendTo.Everyone)]
    void ColorFloorRpc(Color color, string player)
    {
        floorRenderer.material.color = color;
        playerName = player;
    }
}
