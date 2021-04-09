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
            Debug.Log("Digging button pressed (space/E/X/A)");

        if (inputManager.GetInitiateCombo())
            Debug.Log("InitiateCombo button pressed (shift/Q/O/B)");

        if (inputManager.GetLadder())
            Debug.Log("Ladder button pressed (R/square/X)");
    }
}
