using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    public float moveSpeed = 5f;
    NetworkManager networkManager;
    GameManager gameManager;
    Vector3 spawnPoint1 = new Vector3(11f, 1.5f, 0f);
    Vector3 spawnPoint2 = new Vector3(0f, 1.5f, 11f);
    private bool canMove = false;
    MeshRenderer playerMeshRenderer;
    public string playerName;

    void Update()
    {
        if (IsOwner && canMove)
        {
            Movement();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkManager = FindObjectOfType<NetworkManager>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {

        if (IsOwner)
        {
            playerMeshRenderer = this.GetComponent<MeshRenderer>();
        }
        else
        {
            playerMeshRenderer = this.GetComponent<MeshRenderer>();
        }

        if (IsOwner && networkManager.IsHost)
        {
            transform.SetPositionAndRotation(spawnPoint1, new Quaternion());
            playerName = "Host";
        }
        else if (IsOwner && networkManager.IsClient)
        {
            transform.SetPositionAndRotation(spawnPoint2, new Quaternion());
            playerName = "Client";
        }
        else if (networkManager.IsHost)
        {
            playerName = "Client";
        }
        else if (networkManager.IsClient)
        {
            playerName = "Host";
        }

    }

    void Movement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        MovePlayer(moveDirection);
    }

    void MovePlayer(Vector3 moveDirection)
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void EnableMovemant()
    {
        canMove = true;
    }

    public void DisableMovemant()
    {
        canMove = false;
    }

    public void SetColor(Color color)
    {
        ChangeCollorRpc(color);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ChangeCollorRpc(Color color)
    {
        playerMeshRenderer.material.SetColor("_Color", color);
        gameManager.playerReady();
    }

}
