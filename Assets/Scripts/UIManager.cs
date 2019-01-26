using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public Button idleButton;
    public Button moveButton;
    public Button lightsOnButton;
    public Button lightsOffButton;
    public Button chargeButton;

    public Text turnDisplay;
    public Text remainingActions;
    public Text fearMeter;
    public Text announcement;

    public CanvasGroup screenFader;
    public UIBatteryMeter batteryMeter;

    GameManager.Actions nextAction = GameManager.Actions.None;
    List<Room> validRooms;
    Player activePlayer;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        EventManager.AddListener(EventManager.EventType.UpdateActionsUI, UpdateActions);
        EventManager.AddListener(EventManager.EventType.UpdateFearUI, UpdateFearMeter);
        EventManager.AddListener(EventManager.EventType.UpdateTurnsUI, UpdateTurnsDisplay);

        idleButton.onClick.AddListener(IdleButton);
        moveButton.onClick.AddListener(MoveButton);
        lightsOnButton.onClick.AddListener(LightsOnButton);
        lightsOffButton.onClick.AddListener(LightsOffButton);
        chargeButton.onClick.AddListener(ChargeButton);
        validRooms = new List<Room>();
    }

    void UpdateActions(EventDetails details)
    {
        activePlayer = details.player;
        remainingActions.text = string.Format("Remaining actions: {0}", 
            activePlayer.remainingActions.ToString());
        batteryMeter.UpdateDisplay(activePlayer.flashLightCharge);

        if (activePlayer.remainingActions == 0)
        {
            idleButton.interactable = false;
            moveButton.interactable = false;
            lightsOnButton.interactable = false;
            lightsOffButton.interactable = false;
            chargeButton.interactable = false;
        } else
        {
            if (activePlayer.isGhost)
            {
                idleButton.interactable = !activePlayer.currentRoom.isLit;
                moveButton.interactable = activePlayer.CanMove();
                lightsOnButton.interactable = false;
                lightsOffButton.interactable = !activePlayer.currentRoom.isLit && CanTurnOffLights();
                chargeButton.interactable = false;
            }
            else
            {
                idleButton.interactable = true;
                moveButton.interactable = activePlayer.CanMove();
                lightsOnButton.interactable = !activePlayer.currentRoom.isLit;
                lightsOffButton.interactable = false;
                chargeButton.interactable = activePlayer.flashLightCharge < 4;
            }
        }
    }

    void UpdateFearMeter(EventDetails details)
    {
        fearMeter.text = string.Format("Fear: {0}", details.intValue);
    }

    void UpdateTurnsDisplay(EventDetails details)
    {
        turnDisplay.text = details.player.playerName;
    }

    void IdleButton()
    {
        GameManager.PlayerInput(GameManager.Actions.Idle);
    }

    void MoveButton()
    {
        nextAction = GameManager.Actions.Move;
        validRooms.Clear();

        if (activePlayer.isGhost)
        {
            foreach (Room room in activePlayer.currentRoom.doors)
            {
                if (!room.isLit) validRooms.Add(room);
            }
            foreach (Room room in activePlayer.currentRoom.sharedWalls)
            {
                if (!room.isLit) validRooms.Add(room);
            }
        } else
        {
            foreach (Room room in activePlayer.currentRoom.doors)
            {
                if (room.isLit || activePlayer.flashLightCharge > 0) validRooms.Add(room);
            }
        }

        foreach (Room room in validRooms)
        {
            room.SetHighlight(true);
        }

        EventManager.AddListener(EventManager.EventType.RoomClicked, SelectRoom);
    }

    void LightsOnButton()
    {
        GameManager.PlayerInput(GameManager.Actions.TurnOnLight, activePlayer.currentRoom);
    }

    void LightsOffButton()
    {
        nextAction = GameManager.Actions.TurnOffLight;
        validRooms.Clear();

        foreach (Room room in activePlayer.currentRoom.doors)
        {
            if (room.isLit) validRooms.Add(room);
        }
        foreach (Room room in activePlayer.currentRoom.sharedWalls)
        {
            if (room.isLit) validRooms.Add(room);
        }

        foreach (Room room in validRooms)
        {
            room.SetHighlight(true);
        }

        EventManager.AddListener(EventManager.EventType.RoomClicked, SelectRoom);
    }

    void ChargeButton()
    {
        GameManager.PlayerInput(GameManager.Actions.ChargeFlashlight);
    }

    void SelectRoom(EventDetails details)
    {
        if (validRooms.Contains(details.room))
        {
            switch (nextAction)
            {
                case GameManager.Actions.Move:
                    GameManager.PlayerInput(GameManager.Actions.Move, details.room);
                    break;
                case GameManager.Actions.TurnOffLight:
                    GameManager.PlayerInput(GameManager.Actions.TurnOffLight, details.room);
                    break;
            }

            foreach (Room room in validRooms)
            {
                room.SetHighlight(false);
            }
            EventManager.RemoveListener(EventManager.EventType.RoomClicked, SelectRoom);
        }
    }

    bool CanTurnOffLights()
    {
        foreach (Room room in activePlayer.currentRoom.doors)
        {
            if (room.isLit) return true;
        }
        foreach (Room room in activePlayer.currentRoom.sharedWalls)
        {
            if (room.isLit) return true;
        }
        return false;
    }

    public static void ShowAnnouncement(string text)
    {
        instance.StartCoroutine(instance.AnnouncementFadeIn(0.5f, text));
    }

    IEnumerator AnnouncementFadeIn(float duration, string text)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            screenFader.alpha = 0.5f * (elapsed / duration);
            yield return null;
        }
        announcement.text = text;
        announcement.gameObject.SetActive(true);
    }
}