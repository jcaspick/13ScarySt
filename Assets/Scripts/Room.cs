using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Room> doors;
    public List<Room> sharedWalls;
    public bool isLit = false;
}