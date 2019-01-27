using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject humanActions;
    public GameObject ghostActions;

    public Button idleButton;
    public Button moveButton;
    public Button lightsOnButton;
    public Button chargeButton;

    public Button ghostIdle;
    public Button ghostMove;
    public Button lightsOffButton;

    public Text turnDisplay;
    public Text remainingActions;
    public Text endGameText;

    public GameObject actionIndicator;
    public GameObject actionsCounter;
    public CanvasGroup avertYourEyesOverlay;
    public CanvasGroup endGameOverlay;
    public UIBatteryMeter batteryMeter;
    public UIFearMeter fearMeter;

    public List<GameObject> activeIndicators;

    GameManager.Actions nextAction = GameManager.Actions.None;
    List<Room> validRooms;
    Player activePlayer;
    Color ghostMarkerColor;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        activeIndicators = new List<GameObject>();
        ghostMarkerColor = new Color(59.0f / 255.0f, 59.0f / 255.0f, 33.0f / 255.0f, 1.0f);
    }

    private void Start()
    {
        EventManager.AddListener(EventManager.EventType.UpdateActionsUI, UpdateActions);
        EventManager.AddListener(EventManager.EventType.UpdateFearUI, UpdateFearMeter);
        EventManager.AddListener(EventManager.EventType.UpdateTurnsUI, UpdateTurnsDisplay);

        idleButton.onClick.AddListener(IdleButton);
        moveButton.onClick.AddListener(MoveButton);
        lightsOnButton.onClick.AddListener(LightsOnButton);
        chargeButton.onClick.AddListener(ChargeButton);

        ghostIdle.onClick.AddListener(IdleButton);
        ghostMove.onClick.AddListener(MoveButton);
        lightsOffButton.onClick.AddListener(LightsOffButton);
        
        validRooms = new List<Room>();
    }

    void UpdateActions(EventDetails details)
    {
        activePlayer = details.player;
        remainingActions.text = activePlayer.remainingActions.ToString();
        batteryMeter.UpdateDisplay(activePlayer.flashLightCharge);

        if (activePlayer.remainingActions == 0)
        {
            idleButton.interactable = false;
            moveButton.interactable = false;
            lightsOnButton.interactable = false;
            chargeButton.interactable = false;
            lightsOffButton.interactable = false;
            ghostIdle.interactable = false;
            ghostMove.interactable = false;
        } else
        {
            if (activePlayer.isGhost)
            {
                batteryMeter.gameObject.SetActive(false);
                ghostActions.SetActive(true);
                humanActions.SetActive(false);
                ghostIdle.interactable = !activePlayer.currentRoom.isLit;
                ghostMove.interactable = activePlayer.CanMove();
                lightsOffButton.interactable = !activePlayer.currentRoom.isLit && CanTurnOffLights();
            }
            else
            {
                batteryMeter.gameObject.SetActive(true);
                ghostActions.SetActive(false);
                humanActions.SetActive(true);
                idleButton.interactable = true;
                moveButton.interactable = activePlayer.CanMove();
                lightsOnButton.interactable = !activePlayer.currentRoom.isLit;
                chargeButton.interactable = activePlayer.flashLightCharge < 4;
            }
        }
    }

    void UpdateFearMeter(EventDetails details)
    {
        fearMeter.SetFearLevel(details.intValue);
    }

    void UpdateTurnsDisplay(EventDetails details)
    {
        //turnDisplay.text = details.player.playerName;
        turnDisplay.text = string.Format("<color=#{0}ff>{1}</color> TURN",
            ColorUtility.ToHtmlStringRGB(details.player.color),
            details.player.playerName.ToUpper());
    }

    void IdleButton()
    {
        CancelExistingActions();
        GameManager.PlayerInput(GameManager.Actions.Idle);
    }

    void MoveButton()
    {
        CancelExistingActions();
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
            //room.SetHighlight(true);
            UIActionIndicator indicator = Instantiate(actionIndicator).GetComponent<UIActionIndicator>();
            indicator.transform.position = room.transform.position + Vector3.up * 3.0f;
            
            if (activePlayer.isGhost)
            {
                indicator.SetupIndicator(UIActionIndicator.Indicator.MoveGhost, ghostMarkerColor);
            } else
            {
                if (room.isLit)
                {
                    indicator.SetupIndicator(UIActionIndicator.Indicator.MoveHumanLit, activePlayer.color);
                } else
                {
                    indicator.SetupIndicator(UIActionIndicator.Indicator.MoveHumanUnlit, activePlayer.color);
                }
            }

            activeIndicators.Add(indicator.gameObject);
        }

        EventManager.AddListener(EventManager.EventType.RoomClicked, SelectRoom);
    }

    void LightsOnButton()
    {
        CancelExistingActions();
        GameManager.PlayerInput(GameManager.Actions.TurnOnLight, activePlayer.currentRoom);
    }

    void LightsOffButton()
    {
        CancelExistingActions();
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
            UIActionIndicator indicator = Instantiate(actionIndicator).GetComponent<UIActionIndicator>();
            indicator.transform.position = room.transform.position + Vector3.up * 3.0f;
            indicator.SetupIndicator(UIActionIndicator.Indicator.TurnOffLight, ghostMarkerColor);
            activeIndicators.Add(indicator.gameObject);
        }

        EventManager.AddListener(EventManager.EventType.RoomClicked, SelectRoom);
    }

    void ChargeButton()
    {
        CancelExistingActions();
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

            //foreach (Room room in validRooms)
            //{
            //    room.SetHighlight(false);
            //}
            for (int i = activeIndicators.Count - 1; i >= 0; i--)
            {
                GameObject toDelete = activeIndicators[i];
                activeIndicators.Remove(toDelete);
                Destroy(toDelete);
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

    public static void EndGameOverlay(string text)
    {
        instance.StartCoroutine(instance.EndGameFadeIn(0.5f, text));
    }

    IEnumerator EndGameFadeIn(float duration, string text)
    {
        endGameText.text = text;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            endGameOverlay.alpha = elapsed / duration;
            yield return null;
        }
    }

    public static void AvertYourEyesOverlay(float duration)
    {
        instance.StartCoroutine(instance.AvertYourEyesFadeIn(1.0f));
    }

    public IEnumerator AvertYourEyesFadeIn(float duration)
    {
        avertYourEyesOverlay.gameObject.SetActive(true);
        float fadeDuration = 0.5f;
        float elapsed = 0.0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            avertYourEyesOverlay.alpha = elapsed / fadeDuration;
            yield return null;
        }

        elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (Input.GetMouseButton(0))
            {
                elapsed = duration;
            }
            yield return null;
        }

        elapsed = 0.0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            avertYourEyesOverlay.alpha = 1.0f - (elapsed / fadeDuration);
            yield return null;
        }

        avertYourEyesOverlay.alpha = 0.0f;
        avertYourEyesOverlay.gameObject.SetActive(false);
    }

    public static void FirstTurnUI()
    {
        instance.humanActions.gameObject.SetActive(false);
        instance.ghostActions.gameObject.SetActive(false);
        instance.fearMeter.gameObject.SetActive(false);
        instance.actionsCounter.SetActive(false);
        instance.batteryMeter.gameObject.SetActive(false);
    }

    public static void ShowUI()
    {
        instance.fearMeter.gameObject.SetActive(true);
        instance.actionsCounter.SetActive(true);
        instance.batteryMeter.gameObject.SetActive(true);
        instance.humanActions.gameObject.SetActive(true);
    }

    void CancelExistingActions()
    {
        if (nextAction != GameManager.Actions.None)
        {
            nextAction = GameManager.Actions.None;
            for (int i = activeIndicators.Count - 1; i >= 0; i--)
            {
                GameObject toDelete = activeIndicators[i];
                activeIndicators.Remove(toDelete);
                Destroy(toDelete);
            }
            EventManager.RemoveListener(EventManager.EventType.RoomClicked, SelectRoom);
        }
    }
}