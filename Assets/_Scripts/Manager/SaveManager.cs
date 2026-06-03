using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System.Xml;

public class SaveManager : MonoBehaviour
{
    public void SetGameData(Save save)
    {
        // Rule 2: Defensive checks
        if (GameManager.instance == null || GameManager.instance.player == null)
        {
            Debug.LogWarning("SaveManager: Player or GameManager is missing.");
            return;
        }

        // Apply weapon level safely
        if (GameManager.instance.weapon == null)
        {
            Debug.LogWarning("SaveManager: Weapon is not bound to GameManager yet.");
        }
        else
        {
            GameManager.instance.weapon.SetWeaponLevel(save.WeaponLevel);
        }

        // Apply local player progression stats safely
        GameManager.instance.pesos = save.pesos;
        GameManager.instance.experience = save.experience;
        GameManager.instance.player.rage = (float)save.rage;

        Debug.Log("SaveManager: Player save data synchronized successfully.");
    }

    public void SaveGame()
    {
        // Rule 2: Defensive checks
        if (GameManager.instance == null || GameManager.instance.player == null || GameManager.instance.weapon == null)
        {
            Debug.LogWarning("SaveManager: Cannot save. Components are not fully loaded.");
            return;
        }

        Save save = new Save
        {
            pesos = GameManager.instance.pesos,
            experience = GameManager.instance.experience,
            WeaponLevel = GameManager.instance.weapon.weaponLevel,
            rage = (int)GameManager.instance.player.rage
        };

        string path = Application.dataPath + "/SaveData.json";
        string jsonStr = JsonMapper.ToJson(save);

        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.Write(jsonStr);
        }

        Debug.Log("SaveManager: Save completed successfully.");
    }

    public void SaveGame(Save save)
    {
        string path = Application.dataPath + "/SaveData.json";
        string jsonStr = JsonMapper.ToJson(save);

        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.Write(jsonStr);
        }

        Debug.Log("SaveManager: Save override completed successfully.");
    }

    public void LoadGame()
    {
        string path = Application.dataPath + "/SaveData.json";

        if (File.Exists(path))
        {
            string jsonStr;
            using (StreamReader sr = new StreamReader(path))
            {
                jsonStr = sr.ReadToEnd();
            }

            Save save = JsonMapper.ToObject<Save>(jsonStr);
            SetGameData(save);

            Debug.Log("SaveManager: Game loaded successfully.");
        }
        else
        {
            NewGame();
            LoadGame();
        }
    }

    public void NewGame()
    {
        Save save = new Save
        {
            pesos = 0,
            experience = 0,
            WeaponLevel = 0,
            rage = 0
        };
        SaveGame(save);
    }
}