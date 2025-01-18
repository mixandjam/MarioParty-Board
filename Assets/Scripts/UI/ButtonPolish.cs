using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class ButtonPolish : MonoBehaviour, ISelectHandler, IDeselectHandler
{

    private RectTransform rectTransform;
    private Vector2 originalRectPos;
    [SerializeField] private Vector2 offset;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Color textSelectionColor;
    private Color originalSelectionColor;

    public void OnDeselect(BaseEventData eventData)
    {
        rectTransform.DOAnchorPos(originalRectPos, .3f).SetEase(Ease.OutBack);
        if (buttonText != null)
        {
            buttonText.color = originalSelectionColor;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        transform.DOComplete();
        transform.DOShakeScale(.2f, .2f, 10, 90, true);

        rectTransform.DOAnchorPos(originalRectPos + offset, .3f).SetEase(Ease.OutBack);
        if (buttonText != null)
        {
            buttonText.color = textSelectionColor;
        }
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalRectPos = rectTransform.anchoredPosition;
        if (buttonText != null)
        {
            originalSelectionColor = buttonText.color;
        }
    }

}
