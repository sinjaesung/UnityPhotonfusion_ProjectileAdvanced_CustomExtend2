using UnityEngine;

public static class ClientInfo
{
    public static string Username
    {
        get => PlayerPrefs.GetString("C_Username", string.Empty);
        set => PlayerPrefs.SetString("C_Username", value);
    }
    public static int CharId
    {
        get => PlayerPrefs.GetInt("C_CharId", 0);
        set => PlayerPrefs.SetInt("C_CharId", value);
    }
    public static string LobbyName
    {
        get => PlayerPrefs.GetString("C_LastLobbyName", "");
        set => PlayerPrefs.SetString("C_LastLobbyName", value);
    }
}