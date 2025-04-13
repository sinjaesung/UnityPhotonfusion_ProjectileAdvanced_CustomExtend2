using UnityEngine;
using UnityEngine.SceneManagement;

public static class ServerInfo
{

    public const int UserCapacity = 8; //the actual hard limit

    public static string LobbyName;
    public static string WorldName => ResourceManager.Instance.worlds[WorldId].worldName;

    public static int WorldId
    {
        get => PlayerPrefs.GetInt("S_WorId", 0);
        set => PlayerPrefs.SetInt("S_WorId", value);
    }

    public static int MaxUsers
    {
        get => PlayerPrefs.GetInt("S_MaxUsers", 4);
        set => PlayerPrefs.SetInt("S_MaxUsers", Mathf.Clamp(value, 1, UserCapacity));
    }
}