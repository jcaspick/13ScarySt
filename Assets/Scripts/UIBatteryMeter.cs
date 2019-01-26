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

    private void Start()
    {
        EventManager.AddListener(EventManager.EventType.Flashlight, UpdateDisplay);
    }

    void UpdateDisplay(EventDetails details)
    {
        if (details.flashlightCharge >= 1) Bar1.SetActive(true);
        else Bar1.SetActive(false);

        if (details.flashlightCharge >= 2) Bar2.SetActive(true);
        else Bar2.SetActive(false);

        if (details.flashlightCharge >= 3) Bar3.SetActive(true);
        else Bar3.SetActive(false);

        if (details.flashlightCharge >= 4) Bar4.SetActive(true);
        else Bar4.SetActive(false);
    }
}