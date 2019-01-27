using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActionIndicator : MonoBehaviour
{
    public enum Indicator
    {
        MoveHumanLit,
        MoveHumanUnlit,
        MoveGhost,
        TurnOffLight
    }

    public SpriteRenderer iconSprite;
    public SpriteRenderer bgSprite;

    public Sprite[] sprites;

    public void SetupIndicator(Indicator type, Color color)
    {
        bgSprite.color = color;
        switch (type)
        {
            case Indicator.MoveGhost:
                iconSprite.sprite = sprites[1];
                break;
            case Indicator.MoveHumanLit:
                iconSprite.sprite = sprites[3];
                break;
            case Indicator.MoveHumanUnlit:
                iconSprite.sprite = sprites[2];
                break;
            case Indicator.TurnOffLight:
                iconSprite.sprite = sprites[0];
                break;
        }
    }
}