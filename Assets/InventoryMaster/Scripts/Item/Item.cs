using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Item
{
    public string itemName;
    public int itemID;
    public string itemDesc;
    public Sprite itemIcon;
    public int itemValue = 1;                                   //itemValue is at start 1
    public ItemType itemType;                                   //itemType of the Item
    public int maxStack = 1;
    public int indexItemInList = 999;
    
    public Item getCopy()
    {
        return (Item)this.MemberwiseClone();        
    }
    
    public SaveData Save()
    {
        var data = new SaveData();
        data.id = itemID;
        data.value = itemValue;
        return data;
    }

    [Serializable]
    public class SaveData
    {
        public int id;
        public int value;
    }
}


