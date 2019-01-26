using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button idleButton;
    public Button moveButton;
    public Button lightsOnButton;
    public Button lightsOffButton;
    public Button chargeButton;

    public Text remainingActions;

    GameManager.Actions nextAction = GameManager.Actions.None;
    List<Room> validRooms;
    Player activePlayer;

    private void Start()
    {
        EventManager.AddListener(EventManager.EventType.TurnStart, HandleTurnStart);

        idleButton.onClick.AddListener(IdleButton);
        moveButton.onClick.AddListener(MoveButton);
        lightsOnButton.onClick.AddListener(LightsOnButton);
        lightsOffButton.onClick.AddListener(LightsOffButton);
        chargeButton.onClick.AddListener(ChargeButton);
        validRooms = new List<Room>();
    }

    void HandleTurnStart(EventDetails details)
    {
        activePlayer = details.player;
        remainingActions.text = string.Format("Remaining actions: {0}", 
            activePlayer.remainingActions.ToString());

        if (activePlayer.isGhost)
        {
            EventManager.Invoke(EventManager.EventType.Flashlight,
                new EventDetails() { flashlightCharge = 0 });
            idleButton.interactable = !activePlayer.currentRoom.isLit;
            moveButton.interactable = true;
            lightsOnButton.interactable = false;
            lightsOffButton.interactable = !activePlayer.currentRoom.isLit;
            chargeButton.interactable = false;
        } else
        {
            EventManager.Invoke(EventManager.EventType.Flashlight,
                new EventDetails() { flashlightCharge = activePlayer.flashLightCharge });
            idleButton.interactable = true;
            moveButton.interactable = true;
            lightsOnButton.interactable = !activePlayer.currentRoom.isLit;
            lightsOffButton.interactable = false;
            chargeButton.interactable = activePlayer.flashLightCharge < 4;
        }
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
}