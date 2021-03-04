using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController CharacterController;
    private InputManager InputManager;

    [SerializeField] [Range(1,2)]
    private int MovementState = 1;
    [SerializeField]
    private float MovementSpeed = 3f;
    private Vector3 Direction;

    // Camera vars
    private Transform MainCamera;
    private Vector3 CameraForward;
    private Vector3 CameraRight;

    void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        InputManager = InputManager.Instance;
        MainCamera = Camera.main.transform;
    }

    void Update()
    {
        switch (MovementState)
        {
            case 1:
                MovePlayer1();
                break;
            case 2:
                MovePlayer2();
                break;
            default:
                Debug.LogError("Invalide Movement State.");
                break;
        }
    }

    void MovePlayer1()
    {
        Direction = new Vector3(InputManager.GetInputMovement().x, 0f, InputManager.GetInputMovement().y);
        CharacterController.Move(Direction * Time.deltaTime * MovementSpeed);

        if (Direction != Vector3.zero)
            transform.forward = Direction;

    }

    void MovePlayer2()
    {
        CameraForward = MainCamera.forward;
        CameraRight = MainCamera.right;
        CameraRight.y = CameraForward.y = 0f;

        Direction = CameraForward.normalized * InputManager.GetInputMovement().y + CameraRight.normalized * InputManager.GetInputMovement().x;

        CharacterController.Move(Direction * Time.deltaTime * MovementSpeed);

        if (Direction != Vector3.zero)
            transform.forward = Direction;
    }

}