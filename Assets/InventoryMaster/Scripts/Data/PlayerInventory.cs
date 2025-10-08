using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal.Internal;

public class PlayerInventory : MonoBehaviour
{
    public GameObject inventory;
    public GameObject characterSystem;
    private Inventory mainInventory;
    private Inventory characterSystemInventory;
    private Tooltip toolTip;

    private InputManager inputManagerDatabase;

    public TMP_Text hpText;
    public TMP_Text manaText;
    float maxHealth = 100;
    float maxMana = 100;
    float currentHealth = 10;
    float currentMana = 10;

    public void OnEnable()
    {
        Inventory.ItemConsumed += OnConsumeItem;
        Inventory.ItemEquip += EquipWeapon;
        Inventory.UnEquipItem += UnEquipWeapon;
    }

    public void OnDisable()
    {
        Inventory.ItemConsumed -= OnConsumeItem;
        Inventory.UnEquipItem -= UnEquipWeapon;
        Inventory.ItemEquip -= EquipWeapon;
    }

    void Start()
    {
        UpdateHPBar();
        UpdateManaBar();

        if (inputManagerDatabase == null)
            inputManagerDatabase = (InputManager)Resources.Load("InputManager");

        if (GameObject.FindGameObjectWithTag("Tooltip") != null)
            toolTip = GameObject.FindGameObjectWithTag("Tooltip").GetComponent<Tooltip>();
        if (inventory != null)
            mainInventory = inventory.GetComponent<Inventory>();
        if (characterSystem != null)
            characterSystemInventory = characterSystem.GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        //PanelCharacter
        if (Input.GetKeyDown(inputManagerDatabase.CharacterSystemKeyCode))
        {
            if (!characterSystem.activeSelf)
            {
                characterSystemInventory.openInventory();
            }
            else
            {
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                characterSystemInventory.closeInventory();
            }
        }

        //PanelInventory
        if (Input.GetKeyDown(inputManagerDatabase.InventoryKeyCode))
        {
            if (!inventory.activeSelf)
            {
                mainInventory.openInventory();
            }
            else
            {
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                mainInventory.closeInventory();
            }
        }
    }

    void UpdateHPBar()
    {
        hpText.text = "HP:" + (currentHealth + "/" + maxHealth);
    }

    void UpdateManaBar()
    {
        manaText.text = "MP:" + (currentMana + "/" + maxMana);
    }
    
    void EquipWeapon(Item item)
    {
        if (item.itemType == ItemType.Weapon)
        {
            //add the weapon if you unequip the weapon
        }
    }

    void UnEquipWeapon(Item item)
    {
        if (item.itemType == ItemType.Weapon)
        {
            //delete the weapon if you unequip the weapon
        }
    }

    void OnConsumeItem(Item item)
    {
        if (item.itemType == ItemType.Consumable)
        {
            switch (item.itemID)
            {
                case 10: AddHP(10);
                    break;
                case 11: AddMana(10);
                    break;
                case 12: AddMana(20);
                    break;
                case 15: AddHP(20);
                    break;
                case 16: AddHP(10);
                    break;
                case 17: AddHP(20);
                    AddMana(20);
                    break;
                default:
                    break;
            }
        }
    }

    void AddHP(float hp)
    {
        currentHealth += hp;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        else if (currentHealth < 0) currentHealth = 0;
        UpdateHPBar();
    }

    void AddMana(float mp)
    {
        currentMana += mp;
        if (currentMana > maxMana) currentMana = maxMana;
        else if (currentMana < 0) currentMana = 0;
        UpdateManaBar();
    }

    public void SaveInventory(string inv)
    {
        if (inv == "EquipData")
        {
            characterSystemInventory.updateItemList();
            SaveLoad.SaveInventory(characterSystemInventory.SaveCurrentItems(), inv);
        }

        if (inv == "InventoryData")
        {
            mainInventory.updateItemList();
            SaveLoad.SaveInventory(mainInventory.SaveCurrentItems(), inv);
        }
    }
    
    public void LoadInventory(string inv)
    {
        List<Item.SaveData> curInv = SaveLoad.LoadInventory(inv);
        if (curInv == null) return;
        if (curInv.Count < 1) return;

        Inventory tmpInv = new Inventory();
        if (inv == "EquipData")
            tmpInv = characterSystemInventory;
        if (inv == "InventoryData")
            tmpInv = mainInventory;
        
        tmpInv.deleteAllItems();
        tmpInv.Clear();
        
        foreach (var eq in curInv)
        {
            tmpInv.addItemToInventory(eq.id, eq.value);
        }
    }
}
