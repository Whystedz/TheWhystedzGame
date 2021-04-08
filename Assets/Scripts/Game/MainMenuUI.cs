using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    private void Start() => AudioManager.PlayLobbyMusic();

    public void ExitGame() => Application.Quit();
}
