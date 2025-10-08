using UnityEngine;
using UnityEngine.EventSystems;

public class CloseInventory : MonoBehaviour, IPointerDownHandler
{
    private Inventory inventory;

    private void Awake()
    {
        inventory = GetComponentInParent<Inventory>();
        if (inventory == null)
            Debug.LogError($"{nameof(CloseInventory)}: Inventory component not found in parent!");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        inventory?.closeInventory();
    }
}
