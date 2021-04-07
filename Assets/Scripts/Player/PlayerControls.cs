using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    private InputManager inputManager;

    void Start() => inputManager = InputManager.GetInstance();

    // Update is called once per frame
    void Update()
    {
        if (inputManager.GetDigging())
            Debug.Log("Digging button pressed (space/X)");

        if (inputManager.GetInitiateCombo())
            Debug.Log("InitiateCombo button pressed (C/circle)");

        if (inputManager.GetMuteSelf())
            Debug.Log("MuteSelf button pressed (M/squre)");

        if (inputManager.GetRope())
            Debug.Log("Rope button pressed (R/triangle)");

        if (inputManager.GetMainMenu())
            Debug.Log("MainMenu button pressed (X/start/option)");
    }
}
