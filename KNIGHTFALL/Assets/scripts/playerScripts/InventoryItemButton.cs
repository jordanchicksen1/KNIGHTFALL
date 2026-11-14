
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("Item Description")]
    [SerializeField] private string itemName;
    [TextArea(3, 6)]
    [SerializeField] private string itemDescription;

    [Header("Description Panel")]
    [SerializeField] private TMP_Text descriptionTitle;
    [SerializeField] private TMP_Text descriptionText;

    public void OnSelect(BaseEventData eventData)
    {
        UpdateDescription();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateDescription();
    }

    private void UpdateDescription()
    {
        descriptionTitle.text = itemName;
        descriptionText.text = itemDescription;
    }
}
