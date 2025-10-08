using UnityEngine;
using UnityEngine.EventSystems;

namespace InventorySystem.UI
{
    public class ConsumeItem2 : MonoBehaviour, IPointerClickHandler
    {
        public Item item;

        private static Tooltip tooltip;

        //public ItemType[] itemTypeOfSlot;
        public static EquipmentSystem eS;
        public GameObject duplication;
        public static Inventory mainInventory;

        void Start()
        {
            item = GetComponent<ItemOnObject>().item;
            if (GameObject.FindGameObjectWithTag("Tooltip") != null)
                tooltip = GameObject.FindGameObjectWithTag("Tooltip").GetComponent<Tooltip>();
            if (GameObject.FindGameObjectWithTag("MainInventory") != null)
                mainInventory = GameObject.FindGameObjectWithTag("MainInventory").GetComponent<Inventory>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (eventData.clickCount < 2) return; // double click to consume/equip

            var itemOnObject = GetComponent<ItemOnObject>();
            if (itemOnObject == null) return;

            var itemSO = itemOnObject.item;
            if (itemSO == null) return;

            // Try equip via EquipmentSystem if present on the same root
            var eq = GetComponentInParent< /*InventorySystem.Core.*/EquipmentSystem>();
            if (eq != null)
            {   // Simple behavior: raise event to request equip; consumer of event decides.
                //    InventoryEvents.RaiseItemEquipped(itemSO);
                eq.Equip(itemSO);
                return;
            }

            // Otherwise consume
            //InventoryEvents.RaiseItemConsumed(itemSO);
            mainInventory.ConsumeItem(itemSO);
            // Controller could reduce count via listening to event; for convenience try removing one via controller if assigned
            if ( /*controller*/ mainInventory != null)
            {
                //controller.RemoveItemBySO(itemSO, 1);
                mainInventory.deleteItemFromInventoryWithGameObject(itemSO);
            }
        }

        /*public void OnPointerClick(PointerEventData data)
        {
            if (this.gameObject.transform.parent.parent.parent.GetComponent<EquipmentSystem>() == null)
            {
                bool gearable = false;
                Inventory inventory = transform.parent.parent.parent.GetComponent<Inventory>();
                
                if (data.clickCount >= 2 && data.button == PointerEventData.InputButton.Left)
                {
                    {
                        bool stop = false;
                        if (eS != null)
                        {
                            for (int i = 0; i < eS.getSlotsInTotal(); i++)
                            {
                                if (eS.getTypeOfSlots(i).Equals(item.itemType))
                                {
                                    if (eS.transform.GetChild(1).GetChild(i).childCount == 0)
                                    {
                                        stop = true;
                                        if (eS.transform.GetChild(1).GetChild(i).parent.parent.GetComponent<EquipmentSystem>() != null && this.gameObject.transform.parent.parent.parent.GetComponent<EquipmentSystem>() != null) { }
                                        else                                    
                                            inventory.EquiptItem(item);
                                        
                                        this.gameObject.transform.SetParent(eS.transform.GetChild(1).GetChild(i));
                                        this.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                                        eS.gameObject.GetComponent<Inventory>().updateItemList();
                                        inventory.updateItemList();
                                        gearable = true;
                                        if (duplication != null)
                                            Destroy(duplication.gameObject);
                                        break;
                                    }
                                }
                            }
                            if (!stop)
                            {
                                for (int i = 0; i < eS.getSlotsInTotal(); i++)
                                {
                                    if (eS.getTypeOfSlots(i).Equals(item.itemType))
                                    {
                                        if (eS.transform.GetChild(1).GetChild(i).childCount != 0)
                                        {
                                            GameObject otherItemFromCharacterSystem = eS.transform.GetChild(1).GetChild(i).GetChild(0).gameObject;
                                            Item otherSlotItem = otherItemFromCharacterSystem.GetComponent<ItemOnObject>().item;
                                            inventory.EquiptItem(item);
                                                otherItemFromCharacterSystem.transform.SetParent(this.transform.parent);
                                                otherItemFromCharacterSystem.GetComponent<RectTransform>().localPosition = Vector3.zero;
                                                
                                                this.gameObject.transform.SetParent(eS.transform.GetChild(1).GetChild(i));
                                                this.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
    
                                            gearable = true;                                        
                                            if (duplication != null)
                                                Destroy(duplication.gameObject);
                                            eS.gameObject.GetComponent<Inventory>().updateItemList();
                                            inventory.OnUpdateItemList();
                                            break;
                                        }
                                    }
                                }
                            }
    
                        }
                    }
                    if (!gearable)
                    {
    
                        Item itemFromDup = null;
                        if (duplication != null)
                            itemFromDup = duplication.GetComponent<ItemOnObject>().item;
    
                        inventory.ConsumeItem(item);
    
                        item.itemValue--;
                        if (itemFromDup != null)
                        {
                            duplication.GetComponent<ItemOnObject>().item.itemValue--;
                            if (itemFromDup.itemValue <= 0)
                            {
                                if (tooltip != null)
                                    tooltip.deactivateTooltip();
                                inventory.deleteItemFromInventory(item);
                                Destroy(duplication.gameObject); 
                            }
                        }
                        if (item.itemValue <= 0)
                        {
                            if (tooltip != null)
                                tooltip.deactivateTooltip();
                            inventory.deleteItemFromInventory(item);
                            Destroy(this.gameObject);                        
                        }
                    }
                }
            }
        }
    
        public void consumeIt()
        {
            Inventory inventory = transform.parent.parent.parent.GetComponent<Inventory>();
    
            bool gearable = false;
    
            if (GameObject.FindGameObjectWithTag("EquipmentSystem") != null)
                eS = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>().characterSystem.GetComponent<EquipmentSystem>();
    
            Item itemFromDup = null;
            if (duplication != null)
                itemFromDup = duplication.GetComponent<ItemOnObject>().item;       
    
            bool stop = false;
            if (eS != null)
            {
                
                for (int i = 0; i < eS.getSlotsInTotal(); i++)
                {
                    if (eS.getTypeOfSlots(i).Equals(item.itemType))
                    {
                        if (eS.transform.GetChild(1).GetChild(i).childCount == 0)
                        {
                            stop = true;
                            this.gameObject.transform.SetParent(eS.transform.GetChild(1).GetChild(i));
                            this.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                            eS.gameObject.GetComponent<Inventory>().updateItemList();
                            inventory.updateItemList();
                            inventory.EquiptItem(item);
                            gearable = true;
                            if (duplication != null)
                                Destroy(duplication.gameObject);
                            break;
                        }
                    }
                }
    
                if (!stop)
                {
                    for (int i = 0; i < eS.getSlotsInTotal(); i++)
                    {
                        if (eS.getTypeOfSlots(i).Equals(item.itemType))
                        {
                            if (eS.transform.GetChild(1).GetChild(i).childCount != 0)
                            {
                                GameObject otherItemFromCharacterSystem = eS.transform.GetChild(1).GetChild(i).GetChild(0).gameObject;
                                Item otherSlotItem = otherItemFromCharacterSystem.GetComponent<ItemOnObject>().item;
                                inventory.EquiptItem(item);
                                    otherItemFromCharacterSystem.transform.SetParent(this.transform.parent);
                                    otherItemFromCharacterSystem.GetComponent<RectTransform>().localPosition = Vector3.zero;
                                    
                                    this.gameObject.transform.SetParent(eS.transform.GetChild(1).GetChild(i));
                                    this.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                                gearable = true;
                                if (duplication != null)
                                    Destroy(duplication.gameObject);
                                eS.gameObject.GetComponent<Inventory>().updateItemList();
                                inventory.OnUpdateItemList();
                                break;                           
                            }
                        }
                    }
                }
    
    
            }
            if (!gearable)
            {
    
                if (duplication != null)
                    itemFromDup = duplication.GetComponent<ItemOnObject>().item;
    
                inventory.ConsumeItem(item);
    
    
                item.itemValue--;
                if (itemFromDup != null)
                {
                    duplication.GetComponent<ItemOnObject>().item.itemValue--;
                    if (itemFromDup.itemValue <= 0)
                    {
                        if (tooltip != null)
                            tooltip.deactivateTooltip();
                        inventory.deleteItemFromInventory(item);
                        Destroy(duplication.gameObject);
    
                    }
                }
                if (item.itemValue <= 0)
                {
                    if (tooltip != null)
                        tooltip.deactivateTooltip();
                    inventory.deleteItemFromInventory(item);
                    Destroy(this.gameObject); 
                }
    
            }        
        }*/



        public void createDuplication(GameObject Item)
        {
            Item item = Item.GetComponent<ItemOnObject>().item;
            GameObject dup = mainInventory.GetComponent<Inventory>().addItemToInventory(item.itemID, item.itemValue);
            Item.GetComponent<ConsumeItem>().duplication = dup;
            dup.GetComponent<ConsumeItem>().duplication = Item;
        }
    }
}
