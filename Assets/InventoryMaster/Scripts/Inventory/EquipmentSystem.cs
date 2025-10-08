using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class EquipmentSystem : MonoBehaviour
{
    [SerializeField]
    private ItemType[] itemTypeOfSlots = new ItemType[999];
    
    private Inventory _inventory;
    private int _slotCount;

    private void Awake()
    {
        _inventory = GetComponent<Inventory>();
    }

    void Start()
    {
        ConsumeItem.eS = this;
        _slotCount = _inventory.GetSlotContainer().transform.childCount;
    }

    public int getSlotsInTotal()
    {
        if (_inventory == null)
        {
            Debug.LogError($"{nameof(EquipmentSystem)}: Inventory not found!");
            return -1;
        }

        return _slotCount;
    }

    public ItemType getTypeOfSlots(int slot)
    {
        return itemTypeOfSlots[slot];
    }
}

