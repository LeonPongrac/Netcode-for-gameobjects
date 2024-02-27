using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public const float TIMER_DURATION = 30.0f;
    public const float COUNTDOWN_DURATION = 3.0f;

    NetworkManager networkManager;
    int playersReady;
    GameObject networkSelect;
    GameObject waitingForPlayer;
    GameObject colorSelectActive;
    GameObject colorSelectInactive;
    GameObject gameCountdown;
    GameObject gameOverlay;
    GameObject endScrean;
    Button redButton;
    Button blueButton;
    Button greenButton;
    Button yelowButton;
    Button magentaButton;
    Button cyanButton;
    Button resetButton;
    TMP_Text gameStartCountdown;
    TMP_Text gameTimer;
    TMP_Text resultText;
    TMP_Text scoreText;
    private bool gameCountdownState;
    private bool gameRunningState;
    float countdownTimeRemaining;
    float timerTimeRemaining;
    private Dictionary<string, int> playerScores = new Dictionary<string, int>();
    public List<Color> playerColors = new List<Color>();

    private void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        playersReady = 0;
        networkSelect = GameObject.Find("NetworkSelect");
        waitingForPlayer = GameObject.Find("WaitingForPlayer");
        colorSelectActive = GameObject.Find("ColorSelectActive");
        colorSelectInactive = GameObject.Find("ColorSelectInactive");
        gameCountdown = GameObject.Find("GameCountDown");
        gameOverlay = GameObject.Find("GameOverlay");
        endScrean = GameObject.Find("EndScrean");
        redButton = GameObject.Find("RedButton").GetComponent<Button>();
        blueButton = GameObject.Find("BlueButton").GetComponent<Button>();
        greenButton = GameObject.Find("GreenButton").GetComponent<Button>();
        yelowButton = GameObject.Find("YelowButton").GetComponent<Button>();
        magentaButton = GameObject.Find("MagentaButton").GetComponent<Button>();
        cyanButton = GameObject.Find("CyanButton").GetComponent<Button>();
        resetButton = GameObject.Find("RestartButton").GetComponent<Button>();
        redButton.onClick.AddListener(delegate { setColor(Color.red); });
        blueButton.onClick.AddListener(delegate { setColor(Color.blue); });
        greenButton.onClick.AddListener(delegate { setColor(Color.green); });
        yelowButton.onClick.AddListener(delegate { setColor(Color.yellow); });
        magentaButton.onClick.AddListener(delegate { setColor(Color.magenta); });
        cyanButton.onClick.AddListener(delegate { setColor(Color.cyan); });
        resetButton.onClick.AddListener(delegate { ResetGameRpc(); });
        gameStartCountdown = GameObject.Find("GameStartCountdown").GetComponent<TMP_Text>();
        gameTimer = GameObject.Find("GameCountdown").GetComponent<TMP_Text>();
        resultText = GameObject.Find("ResultText").GetComponent<TMP_Text>();
        scoreText = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
        waitingForPlayer.SetActive(false);
        colorSelectActive.SetActive(false);
        colorSelectInactive.SetActive(false);
        gameCountdown.SetActive(false);
        gameOverlay.SetActive(false);
        endScrean.SetActive(false);
        gameCountdownState = false;
        gameRunningState = false;
        countdownTimeRemaining = COUNTDOWN_DURATION;
        timerTimeRemaining = TIMER_DURATION;
        networkManager.OnClientConnectedCallback += OnPlayerConnected;       
    }

    private void OnPlayerConnected(ulong obj)
    {
        Debug.Log("Player connected: " + obj);

        if (waitingForPlayer)
        {
            networkSelect.SetActive(false);
            waitingForPlayer.SetActive(true);
        }
        if (obj == 1 && networkManager.IsHost)
        {
            StartSetupRpc();
        }

    }

    public void setColor(Color color)
    {
        CheckForColorRpc(color, networkManager.LocalClientId);
    }

    private void Update()
    {
        if (gameCountdownState)
        {
            colorSelectInactive.SetActive(false);
            countdownTimeRemaining -= Time.deltaTime;
            if (countdownTimeRemaining <= 0f)
            {
                gameCountdownState = false;
                if (networkManager.IsHost)
                {
                    StartGameRpc();
                }
            }
            UpdateText("{0}", countdownTimeRemaining, gameStartCountdown);
        }
        if (gameRunningState)
        {
            timerTimeRemaining -= Time.deltaTime;
            if (timerTimeRemaining <= 0f)
            {
                gameRunningState = false;
                if (networkManager.IsHost)
                {
                    GameEndRpc();
                }
                GetResult();
                endScrean.SetActive(true);
            }
            UpdateText("{0}", timerTimeRemaining, gameTimer);
        }
    }

    void GetResult()
    {
        string winner = "";
        int highestScore = -1;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                playerScores.Add(playerManager.GetPlayerName(), 0);
            }
        }

        GameObject[] floors = GameObject.FindGameObjectsWithTag("Floor");

        foreach (GameObject floor in floors)
        {
            FloorManager floorManager = floor.GetComponent<FloorManager>();

            if (floorManager != null)
            {
                string playerName = floorManager.playerName;

                if (playerScores.ContainsKey(playerName))
                {
                    playerScores[playerName]++;
                }
            }
        }

        foreach (var entry in playerScores)
        {
            if (entry.Value > highestScore)
            {
                highestScore = entry.Value;
                winner = entry.Key;
            }
        }

        if (winner.Equals("Host"))
        {
            if (networkManager.IsHost)
            {
                resultText.text = "You win";
            }
            else
            {
                resultText.text = "You lose";
            }
        }
        else
        {
            if (networkManager.IsHost)
            {
                resultText.text = "You lose";
            }
            else
            {
                resultText.text = "You win";
            }
        }

        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        string scoreString = "Player Scores:\n";

        foreach (var entry in playerScores)
        {
            scoreString += entry.Key + ": " + entry.Value + "\n";
        }

        scoreText.text = scoreString;
    }

    void UpdateText(string format, float time, TMP_Text text)
    {
        text.text = string.Format(format, Mathf.CeilToInt(time));
    }

    public void playerReady()
    {
        Debug.Log($"Player ready");
        playersReady++;
        if (networkManager.IsHost && playersReady == 2)
        {
            Debug.Log($"Starting game");
            StartCountdownRpc();
        }
    }

    void activateFloor()
    {
        GameObject[] floors = GameObject.FindGameObjectsWithTag("Floor");

        foreach (GameObject floor in floors)
        {
            FloorManager floorManager = floor.GetComponent<FloorManager>();

            if (floorManager != null)
            {
                floorManager.ActivateFloor();
            }
        }
    }

    void deactivateFloor()
    {
        GameObject[] floors = GameObject.FindGameObjectsWithTag("Floor");

        foreach (GameObject floor in floors)
        {
            FloorManager floorManager = floor.GetComponent<FloorManager>();

            if (floorManager != null)
            {
                floorManager.DeActivateFloor();
            }
        }
    }

    void enablePlayerMovement()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                playerManager.EnableMovemant();
            }
        }
    }

    void DisablePlayerMovement()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                playerManager.DisableMovemant();
            }
        }
    }

    void ResetPlayerPosition()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                playerManager.ResetPosition();
            }
        }
    }

    void ResetFloor()
    {
        GameObject[] floors = GameObject.FindGameObjectsWithTag("Floor");

        foreach (GameObject floor in floors)
        {
            FloorManager floorManager = floor.GetComponent<FloorManager>();

            if (floorManager != null)
            {
                floorManager.ResetColor();
            }
        }
    }

    //All entities should activate this function
    [Rpc(SendTo.Everyone)]
    void StartSetupRpc()
    {
        //deactivate waitingForPlayer GameObject
        waitingForPlayer.SetActive(false);
        //activate colorSelectActive GameObject
        colorSelectActive.SetActive(true);
    }

    //Only the Server can run this function
    [Rpc(SendTo.Server)]
    void CheckForColorRpc(Color color, ulong ClientId)
    {
        //activate if the playerColors does not contain the selected color
        if (!playerColors.Contains(color))
        {
            //add the color to the playerColors
            playerColors.Add(color);
            //tell the client that called the function that the selected color is confirmed
            ColorConfirmRpc(color, RpcTarget.Single(ClientId, RpcTargetUse.Temp));
        }
    }

    //Only the entity specified in params can run this function
    [Rpc(SendTo.SpecifiedInParams)]
    void ColorConfirmRpc(Color color, RpcParams rpcParams = default)
    {
        //find the player that is owned by the client and set its color to the selected color
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().IsOwner)
            {
                player.GetComponent<PlayerManager>().SetColor(color);
            }
        }

        //deactivate colorSelectActive GameObject
        colorSelectActive.SetActive(false);
        //activate colorSelectInactive GameObject
        colorSelectInactive.SetActive(true);
    }

    //All entities should activate this function
    [Rpc(SendTo.Everyone)]
    void StartCountdownRpc()
    {
        //deactivate colorSelectInactive GameObject
        colorSelectInactive.SetActive(false);
        //activate gameCountdown GameObject
        gameCountdown.SetActive(true);
        //activate the gameCountdownState trigger
        gameCountdownState = true;
    }

    //All entities should activate this function
    [Rpc(SendTo.Everyone)]
    void StartGameRpc()
    {
        //deactivate gameCountdown GameObject
        gameCountdown.SetActive(false);
        //activate gameOverlay GameObject
        gameOverlay.SetActive(true);
        //activate on enter triggers for the floor
        activateFloor();
        //enable players to move
        enablePlayerMovement();
        //activate the gameRunningState trigger
        gameRunningState = true;
    }

    //All entities should activate this function
    [Rpc(SendTo.Everyone)]
    void GameEndRpc()
    {
        //deactivate gameCountdown GameObject
        gameOverlay.SetActive(false);
        //deactivate on enter triggers for the floor
        deactivateFloor();
        //disable players to move
        DisablePlayerMovement();
    }

    //All entities should activate this function
    [Rpc(SendTo.Everyone)]
    void ResetGameRpc()
    {
        //deactivate gameCountdown GameObject
        endScrean.SetActive(false);
        //reset player position
        ResetPlayerPosition();
        //reset the floor 
        ResetFloor();
        //reset countdownTimeRemaining
        countdownTimeRemaining = COUNTDOWN_DURATION;
        //reset timerTimeRemaining
        timerTimeRemaining = TIMER_DURATION;
        //clear playerScores
        playerScores.Clear();
        //if this is the host start countdown rpc
        if (networkManager.IsHost)
        {
            Debug.Log($"Starting game");
            StartCountdownRpc();
        }
    }
}
