using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IDragHandler, IPointerDownHandler, IEndDragHandler
{
    private Vector2 pointerOffset;
    private RectTransform rectTransform;
    private RectTransform rectTransformSlot;
    private CanvasGroup canvasGroup;
    private GameObject oldSlot;
    private Inventory inventory;
    private Transform draggedItemBox;

    public delegate void ItemDelegate();
    public static event ItemDelegate updateInventoryList;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransformSlot = GameObject.FindGameObjectWithTag("DraggingItem").GetComponent<RectTransform>();
        inventory = transform.parent.parent.parent.GetComponent<Inventory>();
        draggedItemBox = GameObject.FindGameObjectWithTag("DraggingItem").transform;
    }
    
    public void OnDrag(PointerEventData data)
    {
        if (rectTransform == null)
            return;

        if (data.button == PointerEventData.InputButton.Left)
        {
            rectTransform.SetAsLastSibling();
            transform.SetParent(draggedItemBox);
            Vector2 localPointerPosition;
            canvasGroup.blocksRaycasts = false;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransformSlot, Input.mousePosition, data.pressEventCamera, out localPointerPosition))
            {
                rectTransform.localPosition = localPointerPosition - pointerOffset;
                if (transform.GetComponent<ConsumeItem>().duplication != null)
                    Destroy(transform.GetComponent<ConsumeItem>().duplication);
            }
        }
        inventory.OnUpdateItemList();
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out pointerOffset);
            oldSlot = transform.parent.gameObject;
        }

        if (data.button == PointerEventData.InputButton.Right)
        {
            gameObject.GetComponent<ShowTooltip>().tooltip.deactivateTooltip();
            Destroy(this.gameObject);
        }
        
        if (updateInventoryList != null)
            updateInventoryList();
    }

    public void createDuplication(GameObject Item)
    {
        Item item = Item.GetComponent<ItemOnObject>().item;
        GameObject duplication = GameObject.FindGameObjectWithTag("MainInventory").GetComponent<Inventory>().addItemToInventory(item.itemID, item.itemValue);
        duplication.transform.parent.parent.parent.GetComponent<Inventory>().stackableSettings();
        Item.GetComponent<ConsumeItem>().duplication = duplication;
        duplication.GetComponent<ConsumeItem>().duplication = Item;
    }

public void OnEndDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            canvasGroup.blocksRaycasts = true;
            Transform newSlot = null;
            if (data.pointerEnter != null)
                newSlot = data.pointerEnter.transform;

            if (newSlot != null)
            {
                //getting the items from the slots, GameObjects and RectTransform
                GameObject firstItemGameObject = this.gameObject;
                GameObject secondItemGameObject = newSlot.parent.gameObject;
                RectTransform firstItemRectTransform = this.gameObject.GetComponent<RectTransform>();
                RectTransform secondItemRectTransform = newSlot.parent.GetComponent<RectTransform>();
                Item firstItem = rectTransform.GetComponent<ItemOnObject>().item;
                Item secondItem = new Item();
                if (newSlot.parent.GetComponent<ItemOnObject>() != null)
                    secondItem = newSlot.parent.GetComponent<ItemOnObject>().item;

                //get some informations about the two items
                bool sameItem = firstItem.itemName == secondItem.itemName;
                bool sameItemRerferenced = firstItem.Equals(secondItem);
                bool secondItemStack = false;
                bool firstItemStack = false;
                if (sameItem)
                {
                    firstItemStack = firstItem.itemValue < firstItem.maxStack;
                    secondItemStack = secondItem.itemValue < secondItem.maxStack;
                }

                GameObject Inventory = secondItemRectTransform.parent.gameObject;
                if (Inventory.tag == "Slot")
                    Inventory = secondItemRectTransform.parent.parent.parent.gameObject;

                if (Inventory.tag.Equals("Slot"))
                    Inventory = Inventory.transform.parent.parent.gameObject;

                EquipmentSystem eqSys = Inventory.GetComponent<EquipmentSystem>();

                //dragging in an Inventory      
                if (eqSys == null)
                {
                    {
                        int newSlotChildCount = newSlot.transform.parent.childCount;
                        bool isOnSlot = newSlot.transform.parent.GetChild(0).tag == "ItemIcon";
                        //dragging on a slot where allready is an item on
                        if (newSlotChildCount != 0 && isOnSlot)
                        {
                            //check if the items fits into the other item
                            bool fitsIntoStack = false;
                            if (sameItem)
                                fitsIntoStack = (firstItem.itemValue + secondItem.itemValue) <= firstItem.maxStack;
                            //if the item is stackable checking if the firstitemstack and seconditemstack is not full and check if they are the same items

                            if (inventory.stackable && sameItem && firstItemStack && secondItemStack)
                            {
                                //if the item does not fit into the other item
                                if (fitsIntoStack && !sameItemRerferenced)
                                {
                                    secondItem.itemValue = firstItem.itemValue + secondItem.itemValue;
                                    secondItemGameObject.transform.SetParent(newSlot.parent.parent);
                                    Destroy(firstItemGameObject);
                                    secondItemRectTransform.localPosition = Vector3.zero;
                                    if (secondItemGameObject.GetComponent<ConsumeItem>().duplication != null)
                                    {
                                        GameObject dup = secondItemGameObject.GetComponent<ConsumeItem>().duplication;
                                        dup.GetComponent<ItemOnObject>().item.itemValue = secondItem.itemValue;
                                        dup.GetComponent<SplitItem>().inv.stackableSettings();
                                        dup.transform.parent.parent.parent.GetComponent<Inventory>().updateItemList();
                                    }
                                }
                                else
                                {
                                    //creates the rest of the item
                                    int rest = (firstItem.itemValue + secondItem.itemValue) % firstItem.maxStack;

                                    //fill up the other stack and adds the rest to the other stack 
                                    if (!fitsIntoStack && rest > 0)
                                    {
                                        firstItem.itemValue = firstItem.maxStack;
                                        secondItem.itemValue = rest;

                                        firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                        secondItemGameObject.transform.SetParent(oldSlot.transform);

                                        firstItemRectTransform.localPosition = Vector3.zero;
                                        secondItemRectTransform.localPosition = Vector3.zero;
                                    }
                                }

                            }
                            //if does not fit
                            else
                            {
                                //creates the rest of the item
                                int rest = 0;
                                if (sameItem)
                                    rest = (firstItem.itemValue + secondItem.itemValue) % firstItem.maxStack;

                                //fill up the other stack and adds the rest to the other stack 
                                if (!fitsIntoStack && rest > 0)
                                {
                                    secondItem.itemValue = firstItem.maxStack;
                                    firstItem.itemValue = rest;

                                    firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                    secondItemGameObject.transform.SetParent(oldSlot.transform);

                                    firstItemRectTransform.localPosition = Vector3.zero;
                                    secondItemRectTransform.localPosition = Vector3.zero;
                                }
                                //if they are different items or the stack is full, they get swapped
                                else if (!fitsIntoStack && rest == 0)
                                {
                                    //if you are dragging an item from equipmentsystem to the inventory and try to swap it with the same itemtype
                                    if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null && firstItem.itemType == secondItem.itemType)
                                    {
                                        newSlot.transform.parent.parent.parent.parent.GetComponent<Inventory>().UnEquipItem1(firstItem);
                                        oldSlot.transform.parent.parent.GetComponent<Inventory>().EquiptItem(secondItem);

                                        firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                        secondItemGameObject.transform.SetParent(oldSlot.transform);
                                        secondItemRectTransform.localPosition = Vector3.zero;
                                        firstItemRectTransform.localPosition = Vector3.zero;

                                        if (secondItemGameObject.GetComponent<ConsumeItem>().duplication != null)
                                            Destroy(secondItemGameObject.GetComponent<ConsumeItem>().duplication);

                                    }
                                    //if you are dragging an item from the equipmentsystem to the inventory and they are not from the same itemtype they do not get swapped.                                    
                                    else if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null && firstItem.itemType != secondItem.itemType)
                                    {
                                        firstItemGameObject.transform.SetParent(oldSlot.transform);
                                        firstItemRectTransform.localPosition = Vector3.zero;
                                    }
                                    //swapping for the rest of the inventorys
                                    else if (oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() == null)
                                    {
                                        firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                                        secondItemGameObject.transform.SetParent(oldSlot.transform);
                                        secondItemRectTransform.localPosition = Vector3.zero;
                                        firstItemRectTransform.localPosition = Vector3.zero;
                                    }
                                }
                            }
                        }
                        //empty slot
                        else
                        {
                            if (newSlot.tag != "Slot" && newSlot.tag != "ItemIcon")
                            {
                                firstItemGameObject.transform.SetParent(oldSlot.transform);
                                firstItemRectTransform.localPosition = Vector3.zero;
                            }
                            else
                            {                                
                                firstItemGameObject.transform.SetParent(newSlot.transform);
                                firstItemRectTransform.localPosition = Vector3.zero;

                                if (newSlot.transform.parent.parent.GetComponent<EquipmentSystem>() == null && oldSlot.transform.parent.parent.GetComponent<EquipmentSystem>() != null)
                                    oldSlot.transform.parent.parent.GetComponent<Inventory>().UnEquipItem1(firstItem);
                            }
                        }
                    }
                }

                //dragging into a equipmentsystem/charactersystem
                if (eqSys != null)
                {
                    //ItemType[] itemTypeOfSlots = GameObject.FindGameObjectWithTag("EquipmentSystem").GetComponent<EquipmentSystem>().itemTypeOfSlots;
                    int newSlotChildCount = newSlot.transform.parent.childCount;
                    bool isOnSlot = newSlot.transform.parent.GetChild(0).tag == "ItemIcon";
                    bool sameItemType = firstItem.itemType == secondItem.itemType;

                    //dragging on a slot where allready is an item on
                    if (newSlotChildCount != 0 && isOnSlot)
                    {
                        //items getting swapped if they are the same itemtype
                        if (sameItemType && !sameItemRerferenced) //
                        {
                            Transform temp1 = secondItemGameObject.transform.parent.parent.parent;
                            Transform temp2 = oldSlot.transform.parent.parent;                            

                            firstItemGameObject.transform.SetParent(secondItemGameObject.transform.parent);
                            secondItemGameObject.transform.SetParent(oldSlot.transform);
                            secondItemRectTransform.localPosition = Vector3.zero;
                            firstItemRectTransform.localPosition = Vector3.zero;

                            if (!temp1.Equals(temp2))
                            {
                                    Inventory.GetComponent<Inventory>().EquiptItem(firstItem);
                            }
                        }
                        //if they are not from the same Itemtype the dragged one getting placed back
                        else
                        {
                            firstItemGameObject.transform.SetParent(oldSlot.transform);
                            firstItemRectTransform.localPosition = Vector3.zero;
                        }

                    }
                    //if the slot is empty
                    else
                    {
                        for (int i = 0; i < newSlot.parent.childCount; i++)
                        {
                            if (newSlot.Equals(newSlot.parent.GetChild(i)))
                            {
                                //checking if it is the right slot for the item
                                if (eqSys.getTypeOfSlots(i).Equals(transform.GetComponent<ItemOnObject>().item.itemType))
                                {
                                    transform.SetParent(newSlot);
                                    rectTransform.localPosition = Vector3.zero;

                                    if (!oldSlot.transform.parent.parent.Equals(newSlot.transform.parent.parent))
                                        Inventory.GetComponent<Inventory>().EquiptItem(firstItem);

                                }
                                //else it get back to the old slot
                                else
                                {
                                    transform.SetParent(oldSlot.transform);
                                    rectTransform.localPosition = Vector3.zero;
                                }
                            }
                        }
                    }

                }
            }
            //Delete Item
            else
            {
                inventory.OnUpdateItemList();
                Destroy(this.gameObject);

            }
        }
        inventory.OnUpdateItemList();
    }
}
