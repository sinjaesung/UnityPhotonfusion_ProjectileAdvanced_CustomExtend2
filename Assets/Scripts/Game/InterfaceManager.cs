using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FusionExamples.Utility;

public class InterfaceManager : MonoBehaviour
{
    [SerializeField] private ProfileSetupUI profileSetup;

    public UIScreen mainMenu;
    public UIScreen pauseMenu;
    public UIScreen lobbyMenu;

    public static InterfaceManager Instance => Singleton<InterfaceManager>.Instance;

    private void Start()
    {
        profileSetup.AssertProfileSetup();
    }

    public void OpenPauseMenu()
    {
        //open pause menu only if the kart can drive and the menu isn't open already
        if(UIScreen.activeScreen != pauseMenu)
        {
            UIScreen.Focus(pauseMenu);
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}


