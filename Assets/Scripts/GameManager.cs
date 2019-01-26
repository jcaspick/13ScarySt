using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<Color> playerColors;
    public Color ghostColor;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
    }

    public enum Actions
    {
        Idle,
        Move,
        TurnOnLight,
        TurnOffLight,
        ChargeFlashlight,
        Scare,
        ChooseStart
    }

    public Text turnDisplay;

    public int numHumans = 2;
    public int numGhosts = 1;

    public List<Room> house;

    public GameObject playerMarker;
    List<Player> players;
    Player activePlayer = null;
    int activePlayerIndex = -1;

    bool firstRound = true;

    private void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        players = new List<Player>();

        for (int i = 0; i < numHumans; i++)
        {
            Player newPlayer = Instantiate(playerMarker).AddComponent<Player>();
            newPlayer.gameObject.SetActive(false);
            newPlayer.isGhost = false;
            newPlayer.playerName = string.Format("Human {0}", i + 1);
            players.Add(newPlayer);
        }

        for (int j = 0; j < numGhosts; j++)
        {
            Player newGhost = Instantiate(playerMarker).AddComponent<Player>();
            newGhost.gameObject.SetActive(false);
            newGhost.isGhost = true;
            newGhost.playerName = string.Format("Ghost {0}", j + 1);
            players.Add(newGhost);
        }

        activePlayerIndex = 0;
        activePlayer = players[activePlayerIndex];
        EventManager.AddListener(EventManager.EventType.RoomClicked, ChooseStartLocation);
        BeginPlayerTurn();
    }

    void BeginPlayerTurn()
    {
        turnDisplay.text = activePlayer.playerName;
    }

    void EndPlayerTurn()
    {
        activePlayerIndex++;
        if (activePlayerIndex >= players.Count)
        {
            activePlayerIndex = 0;
            if (firstRound)
            {
                firstRound = false;
                EventManager.RemoveListener(EventManager.EventType.RoomClicked, ChooseStartLocation);
            }
        }

        activePlayer = players[activePlayerIndex];

        BeginPlayerTurn();
    }

    void ChooseStartLocation(EventDetails details)
    {
        activePlayer.transform.position = details.room.transform.position;
        activePlayer.gameObject.SetActive(true);

        if (activePlayer.isGhost)
        {
            activePlayer.GetComponent<Renderer>().material.color = ghostColor;
        } else
        {
            activePlayer.GetComponent<Renderer>().material.color = playerColors[activePlayerIndex];
            details.room.SetLight(true);
        }

        EndPlayerTurn();
    }
}