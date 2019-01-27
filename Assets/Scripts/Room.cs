using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Room> doors;
    public List<Room> sharedWalls;
    List<Player> playersInRoom;
    public bool isLit = false;

    public SpriteRenderer lightOverlay;

    private void Start()
    {
        playersInRoom = new List<Player>();
        SetLight(false);
    }

    public void SetLight(bool on)
    {
        if (on)
        {
            lightOverlay.color = Color.white;
            isLit = true;
        }
        else
        {
            lightOverlay.color = Color.clear;
            isLit = false;
        }
    }

    public void SetHighlight(bool highlighted)
    {
        if (highlighted)
        {
            lightOverlay.color = new Color(0.0f, 1.0f, 0.0f, 0.4f);
        } else
        {
            if (isLit)
            {
                lightOverlay.color = Color.white;
            } else
            {
                lightOverlay.color = Color.clear;
            }
        }
    }

    public void AddPlayer(Player player)
    {
        playersInRoom.Add(player);
        OrganizeRoom();
    }

    public void RemovePlayer(Player player)
    {
        playersInRoom.Remove(player);
        OrganizeRoom();
    }

    public void OrganizeRoom()
    {
        if (playersInRoom.Count == 1) return;

        float offsetMagnitude = 0.2f;
        Vector3[] offsets = new Vector3[playersInRoom.Count];
        for (int i = 0; i < playersInRoom.Count; i++)
        {
            Vector3 offset = Vector3.right * offsetMagnitude;
            Quaternion rot = Quaternion.AngleAxis(i * (360.0f / playersInRoom.Count), Vector3.up);
            offset = rot * offset;
            offsets[i] = offset;

            playersInRoom[i].transform.position += offset;
        }
    }

    private void OnMouseUpAsButton()
    {
        EventDetails details = new EventDetails();
        details.room = this;
        EventManager.Invoke(EventManager.EventType.RoomClicked, details);
    }

    public bool IsOccupied()
    {
        return playersInRoom.Count > 0;
    }
}