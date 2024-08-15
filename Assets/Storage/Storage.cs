using System.IO;
using UnityEngine;

public class Storage : MonoBehaviour
{
    private string userDir;
    private string saveFilePath;

    public void Initialize()
    {
        userDir = Path.Combine(Application.persistentDataPath, Steam.MySteamID().m_SteamID.ToString());
        Directory.CreateDirectory(userDir);
        
        saveFilePath = Path.Combine(userDir, "savefile.json");
    }

    public void SaveData(UserData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
    }

    public UserData GetUserData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            UserData data = JsonUtility.FromJson<UserData>(json);
            return data;
        }

        return new UserData(0, 0);
    }
}

[System.Serializable]
public class UserData
{
    public int wins;
    public int gamesPlayed;

    public UserData(int wins, int games)
    {
        this.wins = wins;
        this.gamesPlayed = games;
    }
}