using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    // Анимация перемещения
    [SerializeField]
    private float duration = 1.5f; // длительность анимации (секунд)
    [SerializeField]
    private float elapsed = 0f;

    //Prefabs
    [SerializeField]
    private GameObject prefabCanvasWithPanel;
    [SerializeField]
    private GameObject prefabSlot;
    [SerializeField]
    private GameObject prefabSlotContainer;
    [SerializeField]
    private GameObject prefabItem;
    [SerializeField]
    private GameObject prefabDraggingItemContainer;
    [SerializeField]
    private GameObject prefabPanel;

    //Itemdatabase
    [SerializeField]
    private ItemDataBaseList itemDatabase;

    //GameObjects which are alive
    [SerializeField]
    private string inventoryTitle;
    [SerializeField]
    private RectTransform PanelRectTransform;
    [SerializeField]
    private Image PanelImage;
    [SerializeField] 
    private GameObject SlotContainer;
    [SerializeField]
    private GameObject DraggingItemContainer;
    [SerializeField]
    private RectTransform SlotContainerRectTransform;
    [SerializeField]
    private GridLayoutGroup SlotGridLayout;
    [SerializeField]
    private RectTransform SlotGridRectTransform;

    //Inventory Settings
    [SerializeField]
    public bool mainInventory;
    [SerializeField]
    private List<Item> ItemsInInventory = new List<Item>();
   [SerializeField]
    public int height;
    [SerializeField]
    public int width;
    [SerializeField]
    public bool stackable;
    [SerializeField]
    public static bool inventoryOpen;

    //GUI Settings
    [SerializeField]
    public int slotSize;
    [SerializeField]
    public int iconSize;

    InputManager inputManagerDatabase;

    //event delegates for consuming, gearing
    public delegate void ItemDelegate(Item item);
    public static event ItemDelegate ItemConsumed;
    public static event ItemDelegate ItemEquip;
    public static event ItemDelegate UnEquipItem;

    public delegate void InventoryOpened();
    public static event InventoryOpened InventoryOpen;
    public static event InventoryOpened AllInventoriesClosed;
    
    public List<Item.SaveData> SaveCurrentItems()
    {
        return ItemsInInventory.Select(x => x.Save()).ToList();
    }

    void Start()
    {
        updateItemList();

        inputManagerDatabase = (InputManager)Resources.Load("InputManager");
    }

    public void SortItems()
    {
        updateItemList();
        StartCoroutine(ApplySortingToUI());
    }
    
    public void SortByName()
    {
        updateItemList();
        ItemsInInventory.Sort((a, b) => string.Compare(a.itemName, b.itemName, System.StringComparison.OrdinalIgnoreCase));
        StartCoroutine(ApplySortingToUI());
    }

    public void SortByType()
    {
        updateItemList();
        //sort type -> name
        ItemsInInventory.Sort((a, b) =>
        {
            int typeComparison = a.itemType.CompareTo(b.itemType);
            if (typeComparison == 0)
                return string.Compare(a.itemName, b.itemName, System.StringComparison.OrdinalIgnoreCase);
            return typeComparison;
        });
        
        StartCoroutine(ApplySortingToUI());
    }

    IEnumerator ApplySortingToUI()
    {
        //Save tek item
        List<GameObject> itemObjects = new List<GameObject>();
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
            if (SlotContainer.transform.GetChild(i).childCount > 0)
                itemObjects.Add(SlotContainer.transform.GetChild(i).GetChild(0).gameObject);

        //Save position
        List<Vector3> startPositions = new List<Vector3>();
        foreach (GameObject go in itemObjects)
            startPositions.Add(go.transform.position);

        //New position
        List<Transform> slots = new List<Transform>();
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
            slots.Add(SlotContainer.transform.GetChild(i));

        //New slot
        for (int i = 0; i < itemObjects.Count && i < ItemsInInventory.Count; i++)
            itemObjects[i].GetComponent<ItemOnObject>().item = ItemsInInventory[i];

        //Target position
        List<Vector3> targetPositions = new List<Vector3>();
        for (int i = 0; i < itemObjects.Count && i < slots.Count; i++)
            targetPositions.Add(slots[i].position);

        float tekElapsed = elapsed;
        //Lerp
        while (tekElapsed < duration)
        {
            tekElapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, tekElapsed / duration);

            for (int i = 0; i < itemObjects.Count && i < targetPositions.Count; i++)
                if (itemObjects[i] != null)
                    itemObjects[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
        
            yield return null;
        }

        //Set item to slot
        for (int i = 0; i < itemObjects.Count && i < slots.Count; i++)
            if (itemObjects[i] != null)
            {
                itemObjects[i].transform.SetParent(slots[i]);
                itemObjects[i].GetComponent<RectTransform>().localPosition = Vector3.zero;
            }

        stackableSettings();
        updateItemList();
        yield return new WaitForEndOfFrame();
    }

    void Update()
    {
        updateItemIndex();
    }

    public void OnUpdateItemList()
    {
        updateItemList();
    }

    public void closeInventory()
    {
        this.gameObject.SetActive(false);
        checkIfAllInventoryClosed();
    }

    public void openInventory()
    {
        this.gameObject.SetActive(true);
        if (InventoryOpen != null)
            InventoryOpen();
    }

    public void checkIfAllInventoryClosed()
    {
        GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");

        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            GameObject child = canvas.transform.GetChild(i).gameObject;
            if (!child.activeSelf && (child.tag == "EquipmentSystem" || child.tag == "Panel" || child.tag == "MainInventory" || child.tag == "CraftSystem"))
            {
                if (AllInventoriesClosed != null && i == canvas.transform.childCount - 1)
                    AllInventoriesClosed();
            }
            else if (child.activeSelf && (child.tag == "EquipmentSystem" || child.tag == "Panel" || child.tag == "MainInventory" || child.tag == "CraftSystem"))
                break;

            else if (i == canvas.transform.childCount - 1)
            {
                if (AllInventoriesClosed != null)
                    AllInventoriesClosed();
            }
        }
    }

    public void ConsumeItem(Item item)
    {
        if (ItemConsumed != null)
            ItemConsumed(item);
    }

    public void EquiptItem(Item item)
    {
        if (ItemEquip != null)
            ItemEquip(item);
    }

    public void UnEquipItem1(Item item)
    {
        if (UnEquipItem != null)
            UnEquipItem(item);
    }
    
    public bool characterSystem()
    {
        if (GetComponent<EquipmentSystem>() != null)
            return true;
        else
            return false;
    }

    public void setImportantVariables()
    {
        PanelRectTransform = GetComponent<RectTransform>();
        SlotContainer = transform.GetChild(1).gameObject;
        SlotGridLayout = SlotContainer.GetComponent<GridLayoutGroup>();
        SlotGridRectTransform = SlotContainer.GetComponent<RectTransform>();
    }

    public void updateItemList()
    {
        Clear();
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
        {
            Transform trans = SlotContainer.transform.GetChild(i);
            if (trans.childCount != 0)
            {
                ItemsInInventory.Add(trans.GetChild(0).GetComponent<ItemOnObject>().item);
            }
        }
    }

    public void Clear()
    {
        ItemsInInventory.Clear();
    }
    
    private void updSlotAll()
    {
        if (prefabSlot == null)
            prefabSlot = Resources.Load("Prefabs/Slot - Inventory") as GameObject;

        if (SlotContainer == null)
        {
            SlotContainer = (GameObject)Instantiate(prefabSlotContainer);
            SlotContainer.transform.SetParent(PanelRectTransform.transform);
            SlotContainerRectTransform = SlotContainer.GetComponent<RectTransform>();
            SlotGridRectTransform = SlotContainer.GetComponent<RectTransform>();
            SlotGridLayout = SlotContainer.GetComponent<GridLayoutGroup>();
        }

        if (SlotContainerRectTransform == null)
            SlotContainerRectTransform = SlotContainer.GetComponent<RectTransform>();

        SlotContainerRectTransform.localPosition = Vector3.zero;
    }

    void updateItemSize()
    {
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
        {
            if (SlotContainer.transform.GetChild(i).childCount > 0)
            {
                SlotContainer.transform.GetChild(i).GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(slotSize, slotSize);
                SlotContainer.transform.GetChild(i).GetChild(0).GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(slotSize, slotSize);
            }
        }
    }

    public void addAllItemsToInventory()
    {
        for (int k = 0; k < ItemsInInventory.Count; k++)
        {
            for (int i = 0; i < SlotContainer.transform.childCount; i++)
            {
                if (SlotContainer.transform.GetChild(i).childCount == 0)
                {
                    GameObject item = (GameObject)Instantiate(prefabItem);
                    item.GetComponent<ItemOnObject>().item = ItemsInInventory[k];
                    item.transform.SetParent(SlotContainer.transform.GetChild(i));
                    item.GetComponent<RectTransform>().localPosition = Vector3.zero;
                    item.transform.GetChild(0).GetComponent<Image>().sprite = ItemsInInventory[k].itemIcon;
                    updateItemSize();
                    break;
                }
            }
        }
        stackableSettings();
    }

    public bool checkIfItemAllreadyExist(int itemID, int itemValue)
    {
        updateItemList();
        int stack;
        for (int i = 0; i < ItemsInInventory.Count; i++)
        {
            if (ItemsInInventory[i].itemID == itemID)
            {
                stack = ItemsInInventory[i].itemValue + itemValue;
                if (stack <= ItemsInInventory[i].maxStack)
                {
                    ItemsInInventory[i].itemValue = stack;
                    GameObject temp = getItemGameObject(ItemsInInventory[i]);
                    if (temp != null && temp.GetComponent<ConsumeItem>().duplication != null)
                        temp.GetComponent<ConsumeItem>().duplication.GetComponent<ItemOnObject>().item.itemValue = stack;
                    return true;
                }
            }
        }
        return false;
    }

    public void addItemToInventory(int id)
    {
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
        {
            if (SlotContainer.transform.GetChild(i).childCount == 0)
            {
                GameObject item = (GameObject)Instantiate(prefabItem);
                item.GetComponent<ItemOnObject>().item = itemDatabase.getItemByID(id);
                item.transform.SetParent(SlotContainer.transform.GetChild(i));
                item.GetComponent<RectTransform>().localPosition = Vector3.zero;
                item.transform.GetChild(0).GetComponent<Image>().sprite = item.GetComponent<ItemOnObject>().item.itemIcon;
                item.GetComponent<ItemOnObject>().item.indexItemInList = ItemsInInventory.Count - 1;
                break;
            }
        }
        
        stackableSettings();
        updateItemList();
    }

    public GameObject addItemToInventory(int id, int value)
    {
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
        {
            if (SlotContainer.transform.GetChild(i).childCount == 0)
            {
                if (!characterSystem() || GetComponent<EquipmentSystem>().getTypeOfSlots(i) ==
                    itemDatabase.getItemByID(id).itemType)
                {
                    GameObject item = (GameObject) Instantiate(prefabItem);
                    ItemOnObject itemOnObject = item.GetComponent<ItemOnObject>();
                    itemOnObject.item = itemDatabase.getItemByID(id);
                    if (itemOnObject.item.itemValue <= itemOnObject.item.maxStack &&
                        value <= itemOnObject.item.maxStack)
                        itemOnObject.item.itemValue = value;
                    else
                        itemOnObject.item.itemValue = 1;
                    item.transform.SetParent(SlotContainer.transform.GetChild(i));
                    item.GetComponent<RectTransform>().localPosition = Vector3.zero;
                    item.transform.GetChild(0).GetComponent<Image>().sprite = itemOnObject.item.itemIcon;
                    itemOnObject.item.indexItemInList = ItemsInInventory.Count - 1;
                    if (inputManagerDatabase == null)
                        inputManagerDatabase = (InputManager) Resources.Load("InputManager");
                    return item;
                }
            }
        }

        stackableSettings();
        updateItemList();
        return null;
    }

    public void stackableSettings(bool stackable, Vector3 posi)
    {
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
        {
            if (SlotContainer.transform.GetChild(i).childCount > 0)
            {
                ItemOnObject item = SlotContainer.transform.GetChild(i).GetChild(0).GetComponent<ItemOnObject>();
                if (item.item.maxStack > 1)
                {
                    RectTransform textRectTransform = SlotContainer.transform.GetChild(i).GetChild(0).GetChild(1).GetComponent<RectTransform>();
                    Text text = SlotContainer.transform.GetChild(i).GetChild(0).GetChild(1).GetComponent<Text>();
                    text.text = "" + item.item.itemValue;
                    text.enabled = stackable;
                    textRectTransform.localPosition = posi;
                }
            }
        }
    }

    public void deleteAllItems()
    {
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
        {
            if (SlotContainer.transform.GetChild(i).childCount != 0)
            {
                Destroy(SlotContainer.transform.GetChild(i).GetChild(0).gameObject);
            }
        }
    }

    public List<Item> getItemList()
    {
        List<Item> theList = new List<Item>();
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
        {
            if (SlotContainer.transform.GetChild(i).childCount != 0)
                theList.Add(SlotContainer.transform.GetChild(i).GetChild(0).GetComponent<ItemOnObject>().item);
        }
        return theList;
    }

    public void stackableSettings()
    {
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
        {
            if (SlotContainer.transform.GetChild(i).childCount > 0)
            {
                ItemOnObject item = SlotContainer.transform.GetChild(i).GetChild(0).GetComponent<ItemOnObject>();
                if (item.item.maxStack > 1)
                {
                    RectTransform textRectTransform = SlotContainer.transform.GetChild(i).GetChild(0).GetChild(1).GetComponent<RectTransform>();
                    Text text = SlotContainer.transform.GetChild(i).GetChild(0).GetChild(1).GetComponent<Text>();
                    text.text = "" + item.item.itemValue;
                    text.enabled = stackable;
                    //textRectTransform.localPosition = new Vector3(positionNumberX, positionNumberY, 0);
                }
                else
                {
                    Text text = SlotContainer.transform.GetChild(i).GetChild(0).GetChild(1).GetComponent<Text>();
                    text.enabled = false;
                }
            }
        }
    }

    public GameObject getItemGameObjectByName(Item item)
    {
        for (int k = 0; k < SlotContainer.transform.childCount; k++)
        {
            if (SlotContainer.transform.GetChild(k).childCount != 0)
            {
                GameObject itemGameObject = SlotContainer.transform.GetChild(k).GetChild(0).gameObject;
                Item itemObject = itemGameObject.GetComponent<ItemOnObject>().item;
                if (itemObject.itemName.Equals(item.itemName))
                {
                    return itemGameObject;
                }
            }
        }
        return null;
    }

    public GameObject getItemGameObject(Item item)
    {
        for (int k = 0; k < SlotContainer.transform.childCount; k++)
        {
            if (SlotContainer.transform.GetChild(k).childCount != 0)
            {
                GameObject itemGameObject = SlotContainer.transform.GetChild(k).GetChild(0).gameObject;
                Item itemObject = itemGameObject.GetComponent<ItemOnObject>().item;
                if (itemObject.Equals(item))
                {
                    return itemGameObject;
                }
            }
        }
        return null;
    }

    public void deleteItem(Item item)
    {
        for (int i = 0; i < ItemsInInventory.Count; i++)
        {
            if (item.Equals(ItemsInInventory[i]))
                ItemsInInventory.RemoveAt(item.indexItemInList);
        }
    }

    public void deleteItemFromInventory(Item item)
    {
        for (int i = 0; i < ItemsInInventory.Count; i++)
        {
            if (item.Equals(ItemsInInventory[i]))
                ItemsInInventory.RemoveAt(i);
        }
    }

    public void deleteItemFromInventoryWithGameObject(Item item)
    {
        for (int i = 0; i < ItemsInInventory.Count; i++)
        {
            if (item.Equals(ItemsInInventory[i]))
            {
                ItemsInInventory.RemoveAt(i);
            }
        }

        for (int k = 0; k < SlotContainer.transform.childCount; k++)
        {
            if (SlotContainer.transform.GetChild(k).childCount != 0)
            {
                GameObject itemGameObject = SlotContainer.transform.GetChild(k).GetChild(0).gameObject;
                Item itemObject = itemGameObject.GetComponent<ItemOnObject>().item;
                if (itemObject.Equals(item))
                {
                    Destroy(itemGameObject);
                    break;
                }
            }
        }
    }

    public int getPositionOfItem(Item item)
    {
        for (int i = 0; i < SlotContainer.transform.childCount; i++)
        {
            if (SlotContainer.transform.GetChild(i).childCount != 0)
            {
                Item item2 = SlotContainer.transform.GetChild(i).GetChild(0).GetComponent<ItemOnObject>().item;
                if (item.Equals(item2))
                    return i;
            }
        }
        return -1;
    }

    public void updateItemIndex()
    {
        for (int i = 0; i < ItemsInInventory.Count; i++)
        {
            ItemsInInventory[i].indexItemInList = i;
        }
    }

    public GameObject GetSlotContainer()
    {
        return SlotContainer;
    }
}
