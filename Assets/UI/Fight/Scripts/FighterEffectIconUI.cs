using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class FighterEffectIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image background;
    private Image icon;
    private TextMeshProUGUI abbreviationText;
    private string description;
    private Action<string> onHover;
    private bool isHovered;

    public void Bind(Sprite sprite, string abbreviation, string description, Color color,
        TMP_FontAsset font, Action<string> onHover)
    {
        EnsureVisuals();

        this.description = description;
        this.onHover = onHover;

        GetComponent<Image>().color = color;
        background.color = Color.Lerp(color, Color.black, 0.8f);
        icon.sprite = sprite;
        icon.enabled = sprite != null;
        abbreviationText.text = abbreviation;
        abbreviationText.font = font != null ? font : TMP_Settings.defaultFontAsset;
        abbreviationText.enabled = sprite == null;

        if (isHovered)
        {
            onHover?.Invoke(description);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        onHover?.Invoke(description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ClearHover();
    }

    private void OnDisable()
    {
        ClearHover();
    }

    private void ClearHover()
    {
        if (isHovered)
        {
            isHovered = false;
            onHover?.Invoke(null);
        }
    }

    private void EnsureVisuals()
    {
        if (background != null)
        {
            return;
        }

        GetComponent<RectTransform>().sizeDelta = new Vector2(30f, 30f);
        GetComponent<Image>().raycastTarget = true;

        background = CreateImage("Background", 1.5f);
        icon = CreateImage("Icon", 3f);
        icon.preserveAspect = true;

        GameObject textObject = new GameObject("Abbreviation", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.layer = gameObject.layer;
        textObject.transform.SetParent(transform, false);
        Stretch(textObject.GetComponent<RectTransform>(), 2f);
        abbreviationText = textObject.GetComponent<TextMeshProUGUI>();
        abbreviationText.fontSize = 14f;
        abbreviationText.enableAutoSizing = true;
        abbreviationText.fontSizeMin = 9f;
        abbreviationText.fontSizeMax = 14f;
        abbreviationText.alignment = TextAlignmentOptions.Center;
        abbreviationText.textWrappingMode = TextWrappingModes.NoWrap;
        abbreviationText.color = Color.white;
        abbreviationText.raycastTarget = false;
    }

    private Image CreateImage(string name, float inset)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.layer = gameObject.layer;
        imageObject.transform.SetParent(transform, false);
        Stretch(imageObject.GetComponent<RectTransform>(), inset);
        Image image = imageObject.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * inset;
        rect.offsetMax = Vector2.one * -inset;
    }

    public static Sprite GetMoveIcon(Move move)
    {
        if (move == null)
        {
            return null;
        }

        Sprite sprite = GetFirstFrame(move.GetPreviewAnimationData());
        if (sprite == null)
        {
            sprite = GetFirstFrame(move.GetLoopAnimationData());
        }
        if (sprite == null)
        {
            sprite = GetFirstFrame(move.GetAnimationData());
        }
        return sprite;
    }

    private static Sprite GetFirstFrame(AnimationData animation)
    {
        if (animation == null || animation.GetFrames() == null)
        {
            return null;
        }

        foreach (Sprite frame in animation.GetFrames())
        {
            if (frame != null)
            {
                return frame;
            }
        }
        return null;
    }
}
