using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBatteryMeter : MonoBehaviour
{
    public GameObject Bar1;
    public GameObject Bar2;
    public GameObject Bar3;
    public GameObject Bar4;

    public void UpdateDisplay(int value)
    {
        if (value >= 1) Bar1.SetActive(true);
        else Bar1.SetActive(false);

        if (value >= 2) Bar2.SetActive(true);
        else Bar2.SetActive(false);

        if (value >= 3) Bar3.SetActive(true);
        else Bar3.SetActive(false);

        if (value >= 4) Bar4.SetActive(true);
        else Bar4.SetActive(false);
    }
}