using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoad
{
    static bool Local = true;

    public static void SaveInventory(List<Item.SaveData> lst, string inv)
    {
        BinaryFormatter _bin = new BinaryFormatter();
        MemoryStream _mem = new MemoryStream();
        _bin.Serialize(_mem, lst);
        string save = Convert.ToBase64String(_mem.GetBuffer());
        
        PlayerPrefs.SetString(inv, save);
        PlayerPrefs.Save();
    }
    
    public static List<Item.SaveData> LoadInventory(string inv)
    {
        List<Item.SaveData> it = new List<Item.SaveData>();
        
        if (PlayerPrefs.HasKey(inv))
        {
            string st = PlayerPrefs.GetString(inv);

            if (st != "")
            { 
                BinaryFormatter _bin = new BinaryFormatter();
                try
                {
                    MemoryStream _mem = new MemoryStream(Convert.FromBase64String(st));
                    it.AddRange(_bin.Deserialize(_mem) as List<Item.SaveData>);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            else
            {
                throw new Exception("No have load " + inv);
            }
        }

        return it;
    }
}
