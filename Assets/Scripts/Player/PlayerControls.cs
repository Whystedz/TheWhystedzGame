using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    private InputManager inputManager;

    void Start() => inputManager = InputManager.GetInstance();

}
