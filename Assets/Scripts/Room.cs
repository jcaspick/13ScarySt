using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Room> doors;
    public List<Room> sharedWalls;
    public List<Player> playersInRoom;
    public bool isLit = false;

    public void SetLight(bool on)
    {
        if (on)
        {
            GetComponent<Renderer>().material.color = new Color(0.6f, 0.6f, 0.6f, 1.0f);
            isLit = true;
        }
        else
        {
            GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            isLit = false;
        }
    }

    public void SetHighlight(bool highlighted)
    {
        if (highlighted)
        {
            GetComponent<Renderer>().material.color = new Color(0.2f, 0.7f, 0.3f, 1.0f);
        } else
        {
            if (isLit)
            {
                GetComponent<Renderer>().material.color = new Color(0.6f, 0.6f, 0.6f, 1.0f);
            } else
            {
                GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            }
        }
    }

    private void OnMouseUpAsButton()
    {
        EventDetails details = new EventDetails();
        details.room = this;
        EventManager.Invoke(EventManager.EventType.RoomClicked, details);
    }
}