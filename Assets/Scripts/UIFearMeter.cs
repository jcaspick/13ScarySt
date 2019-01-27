using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFearMeter : MonoBehaviour
{
    public Sprite[] fearLevels;
    public Image fearDisplay;

    public void SetFearLevel(int fear)
    {
        if (fear <= 0)
        {
            fearDisplay.gameObject.SetActive(false);
        } else
        {
            fearDisplay.gameObject.SetActive(true);
            int level = Mathf.Min(12, fear - 1);
            fearDisplay.sprite = fearLevels[level];
        }
    }
}
