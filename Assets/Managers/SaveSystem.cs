using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static void SaveGame(SaveData data, int slot)
    {
        string path = GetPath(slot);
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);

        Debug.Log("Game Saved at: " + path);
    }

    public static SaveData LoadGame(int slot)
    {
        string path = GetPath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found!");
            return null;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        return data;
    }

    static string GetPath(int slot)
    {
        return Application.persistentDataPath + "/save_" + slot + ".json";
    }
}