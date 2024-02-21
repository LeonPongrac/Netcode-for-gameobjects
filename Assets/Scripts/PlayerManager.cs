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
    GameObject colorSelectActive;
    GameObject colorSelectWait;
    GameObject waitingForPlayer;
    MeshRenderer playerMeshRenderer;
    Button redButton;
    Button blueButton;
    Button greenButton;
    Button yelowButton;
    Button magentaButton;
    Button cyanButton;
    public Color playerColor;
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
        colorSelectActive = GameObject.Find("ColorSelectActive");
        colorSelectWait = GameObject.Find("ColorSelectInactive");
        waitingForPlayer = GameObject.Find("WaitingForPlayer");
        redButton = GameObject.Find("RedButton").GetComponent<Button>();
        blueButton = GameObject.Find("BlueButton").GetComponent<Button>();
        greenButton = GameObject.Find("GreenButton").GetComponent<Button>();
        yelowButton = GameObject.Find("YelowButton").GetComponent<Button>();
        magentaButton = GameObject.Find("MagentaButton").GetComponent<Button>();
        cyanButton = GameObject.Find("CyanButton").GetComponent<Button>();
        redButton.onClick.AddListener(delegate { setColor(Color.red); });
        blueButton.onClick.AddListener(delegate { setColor(Color.blue); });
        greenButton.onClick.AddListener(delegate { setColor(Color.green); });
        yelowButton.onClick.AddListener(delegate { setColor(Color.yellow); });
        magentaButton.onClick.AddListener(delegate { setColor(Color.magenta); });
        cyanButton.onClick.AddListener(delegate { setColor(Color.cyan); });

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
            colorSelectActive.SetActive(true);
            colorSelectWait.SetActive(false);
            playerName = "Host";
        }
        else if (IsOwner && networkManager.IsClient)
        {
            transform.SetPositionAndRotation(spawnPoint2, new Quaternion());
            colorSelectActive.SetActive(false);
            colorSelectWait.SetActive(true);
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

    public void setColor(Color color)
    {
        if (IsOwner)
        {
            playerColor = color;
            ChangeCollorRpc(color);
            colorSelectActive.SetActive(false);
            colorSelectWait.SetActive(true);
        }
        else
        {
            ChangeClientCollorRpc(color);
        }
        
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ChangeClientCollorRpc(Color color)
    {
        Debug.Log($"Change color on client on player {playerName}");
        if (IsOwner && networkManager.IsClient)
        {
            Debug.Log($"I am changing the color on player {playerName}");
            colorSelectActive.SetActive(true);
            colorSelectWait.SetActive(false);
        }
    }

    [Rpc(SendTo.Everyone)]
    void ChangeCollorRpc(Color color)
    {
        playerMeshRenderer.material.SetColor("_Color", color);
        gameManager.playerReady();
    }

}
