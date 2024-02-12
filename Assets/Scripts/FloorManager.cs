using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    private MeshRenderer floorRenderer;

    void Start()
    {
        floorRenderer = GetComponent<MeshRenderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("collided with player");
            // Get the MeshRenderer component of the player object
            MeshRenderer playerRenderer = other.GetComponent<MeshRenderer>();

            // Check if the player's MeshRenderer component is not null
            if (playerRenderer != null)
            {
                // Assign the player's material to the floor's MeshRenderer
                floorRenderer.material = playerRenderer.material;
            }
            else
            {
                Debug.LogWarning("Player object does not have a MeshRenderer component.");
            }
        }
    }
}
