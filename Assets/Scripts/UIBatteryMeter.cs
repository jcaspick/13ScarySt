using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBatteryMeter : MonoBehaviour
{
    public Image Battery1Full;
    public Image Battery1Empty;
    public Image Battery2Full;
    public Image Battery2Empty;
    public Image Battery3Full;
    public Image Battery3Empty;
    public Image Battery4Full;
    public Image Battery4Empty;

    public Image BatteryDepleted;
    public Image BatteryFull;

    public void UpdateDisplay(int value)
    {
        if (value <= 0)
        {
            BatteryDepleted.gameObject.SetActive(true);
            BatteryFull.gameObject.SetActive(false);
        } else
        {
            BatteryDepleted.gameObject.SetActive(false);
            BatteryFull.gameObject.SetActive(true);
        }

        if (value >= 1)
        {
            Battery1Full.gameObject.SetActive(true);
            Battery1Empty.gameObject.SetActive(false);
        } else
        {
            Battery1Full.gameObject.SetActive(false);
            Battery1Empty.gameObject.SetActive(true);
        }

        if (value >= 2)
        {
            Battery2Full.gameObject.SetActive(true);
            Battery2Empty.gameObject.SetActive(false);
        }
        else
        {
            Battery2Full.gameObject.SetActive(false);
            Battery2Empty.gameObject.SetActive(true);
        }

        if (value >= 3)
        {
            Battery3Full.gameObject.SetActive(true);
            Battery3Empty.gameObject.SetActive(false);
        }
        else
        {
            Battery3Full.gameObject.SetActive(false);
            Battery3Empty.gameObject.SetActive(true);
        }

        if (value >= 4)
        {
            Battery4Full.gameObject.SetActive(true);
            Battery4Empty.gameObject.SetActive(false);
        }
        else
        {
            Battery4Full.gameObject.SetActive(false);
            Battery4Empty.gameObject.SetActive(true);
        }
    }
}