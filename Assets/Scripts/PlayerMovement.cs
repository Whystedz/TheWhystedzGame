using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private InputManager inputManager;

    [SerializeField]
    private MovementState movementState;
    [SerializeField]
    private float movementSpeed = 3f;
    private Vector3 direction;

    // Camera vars
    private Transform mainCamera;
    private GameObject freeLookCamera;
    private GameObject virtualCamera;
    private Vector3 cameraForward;
    private Vector3 cameraRight;
    private Vector3 initialPosition;
    private int layerMask;
    // Falling vars
    [SerializeField] private float fallingVelocity = 10f;
    private bool isFalling;

    public enum MovementState : int
    {
        CharacterAndCameraIndependent = 1,
        CharacterDependentOnCamera = 2,
        CameraFollowIndependent = 3
    }

    void Awake()
    {
        layerMask = LayerMask.GetMask("TileMovementCollider");
        this.characterController = GetComponent<CharacterController>();
        this.freeLookCamera = FindObjectOfType<CinemachineFreeLook>().gameObject;
        this.virtualCamera = FindObjectOfType<CinemachineVirtualCamera>().gameObject;

        if((int)this.movementState == 1 || (int)this.movementState == 2)
        {
            this.freeLookCamera.SetActive(true);
            this.virtualCamera.SetActive(false);
        }
        else
        {
            this.freeLookCamera.SetActive(false);
            this.virtualCamera.SetActive(true);
        }

    }

    private void Start()
    {
        this.inputManager = InputManager.GetInstance();
        this.mainCamera = Camera.main.transform;
        this.initialPosition = transform.position;
    }

    void Update()
    {
        if (HasFallenInHole())
            FallingMovementUpdate();
        else
            RegularMovement();
    
    }

    private void RegularMovement()
    {
        this.isFalling = false;
        switch ((int)this.movementState)
        {
            case 1:
                MovePlayerCharacterAndCameraIndependent();
                break;
            case 2:
                MovePlayerCharacterDependentOnCamera();
                break;
            case 3:
                MovePlayerCharacterAndCameraIndependent();
                break;
            default:
                Debug.LogError("Invalid Movement State.");
                break;
        }
    }
    void MovePlayerCharacterAndCameraIndependent()
    {
        this.direction = new Vector3(this.inputManager.GetInputMovement().x, 0f, this.inputManager.GetInputMovement().y);
        this.characterController.Move(this.direction * Time.deltaTime * this.movementSpeed);

        if (this.direction != Vector3.zero)
            transform.forward = this.direction;

    }

    void MovePlayerCharacterDependentOnCamera()
    {
        this.cameraForward = this.mainCamera.forward;
        this.cameraRight = this.mainCamera.right;
        this.cameraRight.y = this.cameraForward.y = 0f;

        this.direction = this.cameraForward.normalized * this.inputManager.GetInputMovement().y + this.cameraRight.normalized * this.inputManager.GetInputMovement().x;

        this.characterController.Move(this.direction * Time.deltaTime * this.movementSpeed);

        if (this.direction != Vector3.zero)
            transform.forward = this.direction;
    }

    private void FallingMovementUpdate()
    {
        this.isFalling = true;
        this.characterController.Move(Vector3.down * Time.deltaTime * this.fallingVelocity);

        if (transform.position.y <= -10f)
            Respawn();
    }

    private void Respawn()
    {
        transform.position = this.initialPosition;
    }

    private bool HasFallenInHole()
    {
        var hitColliders = Physics.OverlapSphere(transform.position, 0.01f, layerMask);
        Collider closestCollider = null;

        if (hitColliders.Length == 0)
        {
            //No tile found! If we're still in testing mode, the hole falls down, so we return true here
            return true;
        }
        else if (hitColliders.Length == 1)
        {
            closestCollider = hitColliders[0];
        }
        else if (hitColliders.Length > 1)
        {
            closestCollider = GetClosestCollider(hitColliders);
        }

        var tile = closestCollider.transform.parent.gameObject.GetComponent<Tile>();

        return tile.tileState == TileState.Respawning;
    }

    private Collider GetClosestCollider(Collider[] hitColliders)
    {
        var minimumDistance = Mathf.Infinity;
        Collider closestCollider = null;

        foreach (var collider in hitColliders)
        {
            var distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < minimumDistance)
            {
                closestCollider = collider;
                minimumDistance = distance;
            }
        }

        return closestCollider;
    }

    public bool IsFalling() => this.isFalling;
}