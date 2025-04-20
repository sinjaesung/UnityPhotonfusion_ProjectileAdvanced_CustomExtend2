using UnityEngine;

public static class ClientInfo
{
    public static int CharId
    {
        get => PlayerPrefs.GetInt("C_CharId", 0);
        set => PlayerPrefs.SetInt("C_CharId", value);
    }
}