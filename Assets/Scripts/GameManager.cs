using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;

public class GameManager : NetworkBehaviour
{
    public const float TIMER_DURATION = 30.0f;
    public const float COUNTDOWN_DURATION = 3.0f;

    NetworkManager networkManager;
    int playersReady;
    GameObject gameSetup;
    GameObject gameCountdown;
    GameObject gameOverlay;
    GameObject waitingForPlayer;
    GameObject endScrean;
    TMP_Text gameStartCountdown;
    TMP_Text gameTimer;
    TMP_Text resultText;
    TMP_Text scoreText;
    private bool gameCountdownState;
    private bool gameRunningState;
    float countdownTimeRemaining;
    float timerTimeRemaining;
    private Dictionary<string, int> playerScores = new Dictionary<string, int>();

    private void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        playersReady = 0;
        gameSetup = GameObject.Find("GameSetup");
        gameCountdown = GameObject.Find("GameCountDown");
        gameOverlay = GameObject.Find("GameOverlay");
        waitingForPlayer = GameObject.Find("WaitingForPlayer");
        endScrean = GameObject.Find("EndScrean");
        gameStartCountdown = GameObject.Find("GameStartCountdown").GetComponent<TMP_Text>();
        gameTimer = GameObject.Find("GameCountdown").GetComponent<TMP_Text>();
        resultText = GameObject.Find("ResultText").GetComponent<TMP_Text>();
        scoreText = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
        gameCountdown.SetActive(false);
        gameOverlay.SetActive(false);
        waitingForPlayer.SetActive(false);
        endScrean.SetActive(false);
        gameCountdownState = false;
        gameRunningState = false;
        countdownTimeRemaining = COUNTDOWN_DURATION;
        timerTimeRemaining = TIMER_DURATION;
        if (networkManager.IsHost)
        {
            networkManager.OnClientConnectedCallback += OnPlayerConnected;
        }
        
    }

    private void OnPlayerConnected(ulong obj)
    {
        Debug.Log("Player connected: " + obj);

    }

    private void Update()
    {
        if (gameCountdownState)
        {
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
                playerScores.Add(playerManager.playerName, 0);
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
            else
            {
                Debug.LogWarning("FloorManager component not found on GameObject with tag 'floor'");
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
            else
            {
                Debug.LogWarning("FloorManager component not found on GameObject with tag 'floor'");
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

    [Rpc(SendTo.Everyone)]
    void StartSetupRpc()
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


    [Rpc(SendTo.Everyone)]
    void StartCountdownRpc()
    {
        gameSetup.SetActive(false);
        gameCountdown.SetActive(true);
        gameCountdownState = true;
    }

    [Rpc(SendTo.Everyone)]
    void StartGameRpc()
    {
        gameCountdown.SetActive(false);
        gameOverlay.SetActive(true);
        activateFloor();
        enablePlayerMovement();
        gameRunningState = true;
    }

    [Rpc(SendTo.Everyone)]
    void GameEndRpc()
    {
        gameOverlay.SetActive(false);
        deactivateFloor();
        DisablePlayerMovement();
    }
}
