using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string playerName;
    public bool isGhost = false;
    public Room currentRoom = null;
    public int flashLightCharge = 4;
    public int remainingActions = 0;
    public Color color;

    public void BeginTurn()
    {
        remainingActions = 2;
    }

    public bool CanMove()
    {
        if (isGhost)
        {
            foreach (Room room in currentRoom.doors)
            {
                if (!room.isLit) return true;
            }
            foreach (Room room in currentRoom.sharedWalls)
            {
                if (!room.isLit) return true;
            }
        } else
        {
            foreach (Room room in currentRoom.doors)
            {
                if (flashLightCharge > 0 || room.isLit) return true;
            }
        }
        return false;
    }
}