using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class Player : NetworkBehaviour
{
    [SerializeField]
    private float movementSpeed = 3f;


    private Vector3 movementInput;

    private CharacterController characterController;
    private InputManager inputManager;


    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        inputManager = InputManager.Instance;
    }

    void Update()
    {
        
        if(base.hasAuthority)
            MovePlayer();
    }

    void MovePlayer()
    {
        movementInput = new Vector3(inputManager.GetPlayerMovement().x, 0f, inputManager.GetPlayerMovement().y) * Time.deltaTime * movementSpeed;
        movementInput += Physics.gravity * Time.deltaTime;

        characterController.Move(movementInput);
    }
}
