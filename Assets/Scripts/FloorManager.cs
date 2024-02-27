using UnityEngine;
using Unity.Netcode;

public class FloorManager : NetworkBehaviour
{
    private MeshRenderer floorRenderer;
    private bool active = false;
    public string playerName;
    Color defaultColor;
    void Start()
    {
        floorRenderer = GetComponent<MeshRenderer>();
        defaultColor = floorRenderer.material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && active)
        {
            //Debug.Log("collided with player");
            MeshRenderer playerRenderer = other.GetComponent<MeshRenderer>();
            string player = other.GetComponent<PlayerManager>().GetPlayerName();

            //Debug.Log($"Player {player} has the cube");

            if (playerRenderer != null)
            {
                ColorFloorRpc(playerRenderer.material.color, player);
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

    public void ResetColor()
    {
        floorRenderer.material.color = defaultColor;
        playerName = "";
    }

    [Rpc(SendTo.Everyone)]
    void ColorFloorRpc(Color color, string player)
    {
        //Set the color of floor to the color of the player
        floorRenderer.material.color = color;
        //Set the playerName to the player that touched the floor
        playerName = player;
    }
}
