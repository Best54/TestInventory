using UnityEngine;
using System.Collections;

public class RandomLootSystem : MonoBehaviour
{
    public int amountOfLoot = 10;
    static ItemDataBaseList inventoryItemList;
    private Inventory mainInv;

    int counter = 0;

    // Use this for initialization
    void Start()
    {
        inventoryItemList = (ItemDataBaseList)Resources.Load("ItemDatabase");
        mainInv = GameObject.FindWithTag("MainInventory").GetComponent<Inventory>();
    }

    public void SetRandom()
    {
        if (!mainInv) return;
        
        counter = 0;
        
        while (counter < amountOfLoot)
        {
            counter++;

            int randomNumber = Random.Range(1, inventoryItemList.itemList.Count - 1);

            mainInv.addItemToInventory(inventoryItemList.itemList[randomNumber].itemID, 1);
        }
        mainInv.stackableSettings();
        mainInv.updateItemList();
    }
}
