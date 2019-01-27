using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDisabledButtonGraphic : MonoBehaviour
{
    Button button;
    Image thisImage;

    private void Start()
    {
        thisImage = GetComponent<Image>();
        button = GetComponentInParent<Button>();
    }

    private void Update()
    {
        if (button.interactable)
        {
            thisImage.color = Color.white;
        } else
        {
            thisImage.color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        }
    }
}
