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
        None,
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
        // TODO replace this with events
        turnDisplay.text = activePlayer.playerName;

        if (!firstRound)
        {
            activePlayer.remainingActions = 2;
            if (activePlayer.isGhost && activePlayer.currentRoom.isLit)
                activePlayer.remainingActions--;

            EventDetails details = new EventDetails();
            details.player = activePlayer;
            EventManager.Invoke(EventManager.EventType.TurnStart, details);
        }
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
        StartCoroutine(EndOfTurnDelay(firstRound ? 0 : 1));
    }

    void HandlePlayerAction(Actions type, Room room = null)
    {
        switch (type)
        {
            case Actions.Idle:
                activePlayer.remainingActions--;
                break;
            case Actions.Move:
                if (!activePlayer.isGhost && !room.isLit)
                {
                    activePlayer.flashLightCharge--;
                    EventManager.Invoke(EventManager.EventType.Flashlight,
                        new EventDetails() { flashlightCharge = activePlayer.flashLightCharge });
                }
                activePlayer.transform.position = room.transform.position;
                activePlayer.currentRoom.playersInRoom.Remove(activePlayer);
                activePlayer.currentRoom = room;
                room.playersInRoom.Add(activePlayer);
                activePlayer.remainingActions--;
                break;
            case Actions.TurnOnLight:
                room.SetLight(true);
                activePlayer.remainingActions--;
                break;
            case Actions.TurnOffLight:
                room.SetLight(false);
                activePlayer.remainingActions--;
                break;
            case Actions.ChargeFlashlight:
                activePlayer.flashLightCharge = 4;
                EventManager.Invoke(EventManager.EventType.Flashlight,
                    new EventDetails() { flashlightCharge = activePlayer.flashLightCharge });
                activePlayer.remainingActions--;
                break;
        }

        if (activePlayer.remainingActions <= 0)
        {
            EndPlayerTurn();
        } else
        {
            EventDetails details = new EventDetails();
            details.player = activePlayer;
            EventManager.Invoke(EventManager.EventType.TurnStart, details);
        }
    }

    public static void PlayerInput(Actions type, Room room = null)
    {
        instance.HandlePlayerAction(type, room);
    }

    void ChooseStartLocation(EventDetails details)
    {
        activePlayer.transform.position = details.room.transform.position;
        activePlayer.gameObject.SetActive(true);
        activePlayer.currentRoom = details.room;
        details.room.playersInRoom.Add(activePlayer);

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

    IEnumerator EndOfTurnDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        BeginPlayerTurn();
    }
}