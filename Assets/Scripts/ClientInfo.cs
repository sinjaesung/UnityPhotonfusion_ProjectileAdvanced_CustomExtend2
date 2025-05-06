using UnityEngine;

public static class ClientInfo
{
    public static int CharId
    {
        get => PlayerPrefs.GetInt("C_CharId", 0);
        set => PlayerPrefs.SetInt("C_CharId", value);
    }
    public static string Name
    {
        get => PlayerPrefs.GetString("C_Name", "");
        set => PlayerPrefs.SetString("C_Name", value);
    }
}