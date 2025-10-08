using UnityEngine;

public class VariablesDatabase : MonoBehaviour
{
    void Awake()
    {
        if (!PlayerPrefs.HasKey("InventoryData"))
        {
            PlayerPrefs.SetString("InventoryData", "");
        }
        if (!PlayerPrefs.HasKey("EquipData"))
        {
            PlayerPrefs.SetString("EquipData", "");
        }
    }
}
