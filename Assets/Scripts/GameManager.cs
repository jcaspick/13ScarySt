using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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
    int activeGhosts;

    public List<Room> house;
    public List<Color> playerColors;
    public Color ghostColor;
    public GameObject playerMarker;

    List<Player> players;
    Player activePlayer = null;
    int activePlayerIndex = -1;
    int fearLevel = 0;
    public int maxFear = 13;
    bool firstRound = true;
    bool ghostTurn = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        fearLevel = 0;
        players = new List<Player>();
        activeGhosts = numGhosts;

        for (int i = 0; i < numHumans; i++)
        {
            Player newPlayer = Instantiate(playerMarker).AddComponent<Player>();
            newPlayer.gameObject.SetActive(false);
            newPlayer.isGhost = false;
            newPlayer.playerName = string.Format("Human {0}", i + 1);
            newPlayer.color = playerColors[i];
            players.Add(newPlayer);
        }

        for (int j = 0; j < numGhosts; j++)
        {
            Player newGhost = Instantiate(playerMarker).AddComponent<Player>();
            newGhost.gameObject.SetActive(false);
            newGhost.isGhost = true;
            newGhost.playerName = string.Format("Ghost {0}", j + 1);
            newGhost.color = ghostColor;
            players.Add(newGhost);
        }

        activePlayerIndex = 0;
        activePlayer = players[activePlayerIndex];
        EventManager.AddListener(EventManager.EventType.RoomClicked, ChooseStartLocation);
        UIManager.instance.fearMeter.SetFearLevel(0);
        BeginPlayerTurn();
    }

    void BeginPlayerTurn()
    {
        EventDetails details = new EventDetails();
        details.player = activePlayer;
        EventManager.Invoke(EventManager.EventType.UpdateTurnsUI, details);

        if (!firstRound)
        {
            if (activePlayer.isGhost && activePlayer.currentRoom.isLit && !activePlayer.CanMove())
            {
                activeGhosts--;
                players.Remove(activePlayer);
                activePlayer.gameObject.SetActive(false);
                Debug.Log("GHOST BUSTED");

                if (activeGhosts <= 0)
                {
                    UIManager.ShowAnnouncement("HUMANS WIN");
                }

                EndPlayerTurn();
                return;
            }

            activePlayer.remainingActions = 2;
            if (activePlayer.isGhost && activePlayer.currentRoom.isLit)
                activePlayer.remainingActions--;

            details.player = activePlayer;
            EventManager.Invoke(EventManager.EventType.UpdateActionsUI, details);
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
            } else
            {
                fearLevel++;
                EventDetails details = new EventDetails();
                details.intValue = fearLevel;
                EventManager.Invoke(EventManager.EventType.UpdateFearUI, details);
                if (fearLevel >= maxFear)
                {
                    UIManager.ShowAnnouncement("GHOSTS WIN");
                    return;
                }
            }
        }

        activePlayer = players[activePlayerIndex];
        StartCoroutine(EndOfTurnDelay(firstRound ? 0 : 0.5f));
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
                activePlayer.remainingActions--;
                break;
        }

        EventDetails details = new EventDetails();
        details.player = activePlayer;
        EventManager.Invoke(EventManager.EventType.UpdateActionsUI, details);

        if (activePlayer.remainingActions <= 0)
        {
            EndPlayerTurn();
        }
    }

    public static void PlayerInput(Actions type, Room room = null)
    {
        instance.HandlePlayerAction(type, room);
    }

    void ChooseStartLocation(EventDetails details)
    {
        if (activePlayer.isGhost && details.room.playersInRoom.Count > 0) return;

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

        if (activePlayer.isGhost && !ghostTurn)
        {
            ghostTurn = true;
            foreach (Player player in players)
            {
                if (player.isGhost && !firstRound) player.gameObject.SetActive(true);
            }
        }
        else if (!activePlayer.isGhost && ghostTurn)
        {
            ghostTurn = false;
            foreach (Player player in players)
            {
                if (player.isGhost) player.gameObject.SetActive(false);
            }
        }

        BeginPlayerTurn();
    }
}