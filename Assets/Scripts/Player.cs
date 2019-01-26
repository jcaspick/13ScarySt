using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string playerName;
    public bool isGhost = false;
    public Room currentRoom = null;

    public int remainingActions = 0;

    public void BeginTurn()
    {
        remainingActions = 2;
    }
}