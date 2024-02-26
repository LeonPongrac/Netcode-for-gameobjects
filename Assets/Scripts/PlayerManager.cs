using Unity.Netcode;
using UnityEngine;

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
        playerMeshRenderer = this.GetComponent<MeshRenderer>();

        //If the you are the owner and the host, sets the player to spawnPoint1 and names the player to Host
        if (IsOwner && networkManager.IsHost)
        {
            transform.SetPositionAndRotation(spawnPoint1, new Quaternion());
            playerName = "Host";
        }
        //If the you are the owner and the client, sets the player to spawnPoint2 and names the player to Client
        else if (IsOwner && networkManager.IsClient)
        {
            transform.SetPositionAndRotation(spawnPoint2, new Quaternion());
            playerName = "Client";
        }
        //If the you are the host, names the player to Client
        else if (networkManager.IsHost)
        {
            playerName = "Client";
        }
        //If the you are the client, names the player to Host
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

    public void ResetPosition()
    {
        if (IsOwner && networkManager.IsHost)
        {
            transform.SetPositionAndRotation(spawnPoint1, new Quaternion());
        }
        else if (IsOwner && networkManager.IsClient)
        {
            transform.SetPositionAndRotation(spawnPoint2, new Quaternion());
        }
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
        //Change the color of the player to the given color
        playerMeshRenderer.material.SetColor("_Color", color);
        //Tell gameManager that the player is ready
        gameManager.playerReady();
    }

}
