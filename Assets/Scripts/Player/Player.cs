using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private float movementSpeed = 3f;
    private Vector3 rawInputMovement;

    private CharacterController characterController = null;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if(base.hasAuthority)
            UpdatePlayerMovement();
    }

    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();

        rawInputMovement = new Vector3(inputMovement.x * Time.deltaTime * movementSpeed, 0f, inputMovement.y * Time.deltaTime * movementSpeed);
        rawInputMovement += Physics.gravity * Time.deltaTime;
    }

    void UpdatePlayerMovement()
    {
        characterController.Move(rawInputMovement);
    }
}
