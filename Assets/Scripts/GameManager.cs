using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameMode
    {
        Rounds,
        LightsOut
    }
    public GameMode gameMode;

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
    public int numHumanActions = 2;
    public int numGhostActions = 2;
    public int numBatteries = 4;
    public bool caughtGhostPenalty = false;
    public bool darkRoomPenalty = false;
    int activeGhosts;

    public List<Room> house;
    public List<Color> playerColors;
    public Color ghostColor;
    public GameObject playerMarker;

    List<Player> players;
    Player activePlayer = null;
    int activePlayerIndex = -1;
    int round = 1;
    int fearLevel = 0;
    public int maxFear = 13;
    public bool firstRound = true;
    public bool gameEnded = false;
    bool ghostTurn = false;
    bool allowInput = false;

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
            //Player newPlayer = Instantiate(playerMarker).AddComponent<Player>();
            //newPlayer.gameObject.SetActive(false);
            //newPlayer.isGhost = false;
            //newPlayer.playerName = string.Format("Human {0}", i + 1);
            //newPlayer.color = playerColors[i];
            //players.Add(newPlayer);

            Player newPlayer = Instantiate(playerMarker).GetComponent<Player>();
            newPlayer.gameObject.SetActive(false);
            newPlayer.isGhost = false;
            newPlayer.playerName = string.Format("Human {0}", i + 1);
            newPlayer.color = playerColors[i];

            newPlayer.mainSprite.sprite = newPlayer.sprites[4];
            newPlayer.mainSprite.color = newPlayer.color;
            newPlayer.glowSprite.sprite = newPlayer.sprites[5];

            players.Add(newPlayer);
        }

        for (int j = 0; j < numGhosts; j++)
        {
            Player newGhost = Instantiate(playerMarker).GetComponent<Player>();
            newGhost.gameObject.SetActive(false);
            newGhost.isGhost = true;
            newGhost.playerName = string.Format("Ghost {0}", j + 1);
            newGhost.color = ghostColor;

            newGhost.mainSprite.sprite = newGhost.sprites[0];
            newGhost.mainSprite.color = newGhost.color;
            newGhost.glowSprite.sprite = newGhost.sprites[1];

            players.Add(newGhost);
        }

        activePlayerIndex = 0;
        activePlayer = players[activePlayerIndex];
        EventManager.AddListener(EventManager.EventType.RoomClicked, ChooseStartLocation);
        UIManager.instance.fearMeter.SetFearLevel(0);
        UIManager.FirstTurnUI();
        allowInput = true;
        BeginPlayerTurn();
    }

    void BeginPlayerTurn()
    {
        EventDetails details = new EventDetails();
        details.player = activePlayer;
        EventManager.Invoke(EventManager.EventType.UpdateTurnsUI, details);

        if (!firstRound)
        {
            StartCoroutine(HighlightActivePlayer(activePlayer, 1.5f, 0.5f));
            
            activePlayer.remainingActions = NumAvailableActions();

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
                UIManager.ShowUI();
                EventManager.RemoveListener(EventManager.EventType.RoomClicked, ChooseStartLocation);
            } else
            {
                EventDetails details = new EventDetails();
                round++;
                details.intValue = round;
                EventManager.Invoke(EventManager.EventType.UpdateRoundsCountUI, details);

                if(gameMode == GameMode.Rounds)
                {
                    fearLevel++;
                    CheckGhostWinCondition();
                }
            }
        }
        StartCoroutine(HighlightActivePlayer(activePlayer, 1.0f, 0.2f));

        activePlayer = players[activePlayerIndex];
        StartCoroutine(EndOfTurnDelay(firstRound ? 0 : 0.5f));
    }

    IEnumerator HighlightActivePlayer(Player player, float targetScale, float duration)
    {
        float elapsed = 0.0f;
        Vector3 startScale = player.transform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, targetScale);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            player.transform.localScale = Vector3.Lerp(startScale, endScale,
                Mathf.SmoothStep(0, 1, elapsed / duration));
            yield return null;
        }
        player.transform.localScale = endScale;
    }

    IEnumerator HandlePlayerAction(Actions type, Room room = null)
    {
        allowInput = false;

        EventDetails details = new EventDetails();

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
                yield return StartCoroutine(LerpPlayer(activePlayer, room.transform.position, 0.5f));
                activePlayer.currentRoom.RemovePlayer(activePlayer);
                activePlayer.currentRoom = room;
                room.AddPlayer(activePlayer);
                activePlayer.remainingActions--;
                break;
            case Actions.TurnOnLight:
                room.SetLight(true);
                CheckForCaughtGhosts(activePlayer.currentRoom);
                activePlayer.remainingActions--;
                break;
            case Actions.TurnOffLight:
                room.SetLight(false);
                activePlayer.remainingActions--;

                if(gameMode == GameMode.LightsOut)
                {
                    fearLevel++;

                    details.intValue = fearLevel;
                    EventManager.Invoke(EventManager.EventType.UpdateFearUI, details);

                    CheckGhostWinCondition();
                }

                break;
            case Actions.ChargeFlashlight:
                activePlayer.flashLightCharge = numBatteries;
                activePlayer.remainingActions--;
                break;
        }

        yield return null;

        details.player = activePlayer;
        EventManager.Invoke(EventManager.EventType.UpdateActionsUI, details);

        allowInput = true;

        if (!CheckHumanWinCondition() && activePlayer.remainingActions <= 0)
        { 
            EndPlayerTurn();
        }
    }

    public static void PlayerInput(Actions type, Room room = null)
    {
        if (!instance.allowInput) return;
        instance.StartCoroutine(instance.HandlePlayerAction(type, room));
    }

    IEnumerator LerpPlayer(Player player, Vector3 end, float duration)
    {
        float elapsed = 0.0f;
        Vector3 start = player.transform.position;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            player.transform.position = Vector3.Lerp(start, end, 
                Mathf.SmoothStep(0, 1, elapsed / duration));
            yield return null;
        }
        player.transform.position = end;
    }

    void ChooseStartLocation(EventDetails details)
    {
        if (!instance.allowInput) return;
        if (activePlayer.isGhost && details.room.IsOccupied()) return;

        activePlayer.transform.position = details.room.transform.position;
        activePlayer.gameObject.SetActive(true);
        activePlayer.currentRoom = details.room;

        details.room.AddPlayer(activePlayer);

        if (activePlayer.isGhost)
        {
            //activePlayer.GetComponent<Renderer>().material.color = ghostColor;
        } else
        {
            //activePlayer.GetComponent<Renderer>().material.color = playerColors[activePlayerIndex];
            details.room.SetLight(true);
        }

        EndPlayerTurn();
    }

    IEnumerator EndOfTurnDelay(float delay)
    {
        allowInput = false;
        yield return new WaitForSeconds(delay);

        if (activePlayer.isGhost && !ghostTurn)
        {
            ghostTurn = true;

            yield return StartCoroutine(UIManager.instance.TurnPhaseOverlayFadeIn(4.0f, "human"));

            foreach (Player player in players)
            {
                if (player.isGhost && !firstRound) player.gameObject.SetActive(true);
            }
        }
        else if (!activePlayer.isGhost && ghostTurn)
        {
            if(ghostTurn)
            {
                foreach (Player player in players)
                {
                    if (player.isGhost) player.gameObject.SetActive(false);
                }

                yield return StartCoroutine(UIManager.instance.TurnPhaseOverlayFadeIn(4.0f, "ghost"));
                ghostTurn = false;
            }
        }

        allowInput = true;
        BeginPlayerTurn();
    }

    bool CheckHumanWinCondition()
    {
        foreach (Player player in players)
        {
            if (!player.isGhost) continue;
            if (player.currentRoom.isLit && !player.CanMove())
            {
                activeGhosts--;

                players.Remove(player);

                if (activeGhosts <= 0)
                {
                    UIManager.EndGameOverlay("HUMANS WIN");
                    allowInput = false;

                    return true;
                }
            }
        }

        return false;
    }

    void CheckForCaughtGhosts(Room room)
    {
        if (room.ContainsGhost())
        {
            StartCoroutine(CaughtGhost(room));
        }
    }

    bool CheckGhostWinCondition()
    {
        if (fearLevel >= maxFear)
        {
            UIManager.EndGameOverlay("GHOSTS WIN");
            allowInput = false;

            return true;
        }
        return false;
    }

    IEnumerator CaughtGhost(Room room)
    {
        Player ghostIcon = Instantiate(playerMarker).GetComponent<Player>();
        ghostIcon.color = ghostColor;
        ghostIcon.mainSprite.sprite = ghostIcon.sprites[0];
        ghostIcon.mainSprite.color = ghostIcon.color;
        ghostIcon.glowSprite.sprite = ghostIcon.sprites[1];

        ghostIcon.transform.position = room.transform.position + Vector3.up;

        float duration = 0.8f;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ghostIcon.transform.position += Vector3.forward * Time.deltaTime;
            ghostIcon.mainSprite.color = Color.Lerp(Color.white, Color.clear, elapsed / duration);
            ghostIcon.glowSprite.color = Color.Lerp(Color.white, Color.clear, elapsed / duration);
            yield return null;
        }

        Destroy(ghostIcon.gameObject);
    }

    int NumAvailableActions()
    {
        int availableActions = 0;

        if (activePlayer.isGhost) {
            availableActions = numGhostActions;

            if (activePlayer.currentRoom.isLit && caughtGhostPenalty)
                availableActions--;
        }
        else
        {
            availableActions = numHumanActions;

            if (!activePlayer.currentRoom.isLit && darkRoomPenalty)
                availableActions--;
        }

        return availableActions;
    }
}