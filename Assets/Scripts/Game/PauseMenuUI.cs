using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    private InputManager inputManager;
    private bool isPaused;

    [SerializeField] private GameObject pauseUI;
    
    private void Start()
    {
        this.isPaused = false;
        this.inputManager = InputManager.GetInstance();
    }

    void Update()
    {
        if (this.inputManager.GetMainMenu())
        {
            if(this.isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        this.pauseUI.SetActive(true);
        this.isPaused = true;
    }

    public void Resume()
    {
        this.pauseUI.SetActive(false);
        this.isPaused = false;
    }
}
