using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class WheelItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Space]
    [Header("Selection & Details")]
    public Button Button;
    public TextMeshProUGUI AmountText;
    [Space]
    [Header("Icon Settings")]
    public Image Image;
    [Space]
    [Header("Item Type")]
    public ItemType ItemType;
    [Space]
    [Header("Button Events")]
    [Space]
    public UnityEvent<PointerEventData> OnHoverEnter;
    [Space]
    public UnityEvent<PointerEventData> OnHoverExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEnter.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit.Invoke(eventData);
    }
}
