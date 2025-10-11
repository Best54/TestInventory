using UnityEngine;
using UnityEngine.EventSystems;

namespace InventorySystem.UI
{
    public class ConsumeItem : MonoBehaviour, IPointerClickHandler
    {
        public Item item;
        public GameObject duplication;
        public static Inventory mainInventory;

        void Start()
        {
            item = GetComponent<ItemOnObject>().item;
            if (GameObject.FindGameObjectWithTag("MainInventory") != null)
                mainInventory = GameObject.FindGameObjectWithTag("MainInventory").GetComponent<Inventory>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Tooltip.Hide();
                Destroy(gameObject);
                return;
            }
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (eventData.clickCount < 2) return; // double click to consume/equip

            var itemOnObject = GetComponent<ItemOnObject>();
            if (itemOnObject == null) return;

            var itemSO = itemOnObject.item;
            if (itemSO == null) return;

            // Try equip via EquipmentSystem if present on the same root
            var eq = GetComponentInParent< /*InventorySystem.Core.*/EquipmentSystem>();
            if (eq != null)
            {   
                eq.Equip(itemSO);
                return;
            }

            // Otherwise consume
            mainInventory.ConsumeItem(itemSO);
            if ( /*controller*/ mainInventory != null)
            {
                Tooltip.Hide();
                mainInventory.DeleteItem(itemSO);
            }
        }
    }
}
