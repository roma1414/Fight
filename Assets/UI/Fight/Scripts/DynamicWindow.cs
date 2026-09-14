using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Linq;

public class DynamicWindow : MonoBehaviour
{
    [SerializeField] private Fight          Fight;
    [SerializeField] private Image          FightBackground;
    [SerializeField] private Image          Attacker;

    public void ConfigureFightWindowForAttackers(MoveEvent moveEvent)
    {
        int team = moveEvent.GetFighters()[0].GetTeam();
        if (team == 1)
        {
            FightBackground.sprite = Fight.GetMapArt().GetBackground_1();
        }
        else if (team == 2)
        {
            FightBackground.sprite = Fight.GetMapArt().GetBackground_2();
        }
        else
        {
            FightBackground.sprite = Fight.GetMapArt().GetBackground_3();
        }

        Vector2 position = FightBackground.rectTransform.anchoredPosition;
        position.x = 0f;
        FightBackground.rectTransform.anchoredPosition = position;

        Attacker.sprite = null;
        Sprite attackerSprite = moveEvent.GetFighters()[0].GetArt().GetBody();
        if (attackerSprite != null)
        {
            Attacker.sprite = attackerSprite;
        }
        else
        {
            Debug.LogError($"Error! Fighter {moveEvent.GetFighters()[0].GetName()} has no body sprite!");
        }
    }

    public IEnumerator DisplayMovePreview(MoveEvent moveEvent)
    {
        ConfigureFightWindowForAttackers(moveEvent);
        yield return new WaitForSeconds(1f);
    }
}
