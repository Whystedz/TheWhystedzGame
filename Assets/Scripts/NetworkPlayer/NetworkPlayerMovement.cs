using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;

public class NetworkPlayerMovement : NetworkBehaviour
{
    private CharacterController characterController;
    private InputManager inputManager;

    [SerializeField]
    private float movementSpeed = 3f;
    [SerializeField]
    private float rotationSpeed = 10f;
    private Vector3 direction;

    private Vector3 initialPosition;
    private int layerMask;
    // Falling vars
    [SerializeField] private float fallingVelocity = 10f;
    private bool isFalling;

    // Camera
    [SerializeField] private Vector2 maxFollowOffset = new Vector2(-5f, 6f);
    [SerializeField] private Vector2 cameraVelocity = new Vector2(4f, 0.25f);
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;
    private CinemachineTransposer transposer;

    public override void OnStartAuthority()
    {
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        virtualCamera.gameObject.SetActive(true);

        layerMask = LayerMask.GetMask("TileMovementCollider");
        this.characterController = GetComponent<CharacterController>();
        this.inputManager = InputManager.GetInstance();
        this.initialPosition = transform.position;

        inputManager.GetPlayerInput().PlayerControls.Camera.performed += ctx => Look(ctx.ReadValue<Vector2>());
    }

    void Update()
    {
        if (base.hasAuthority)
        {
            if (HasFallenInHole())
                FallingMovementUpdate();
            else
                Move();
        }
    }

    private void Move()
    {
        this.isFalling = false;
        /*
        Vector3 right = this.characterController.transform.right;
        Vector3 forward = this.characterController.transform.forward;
        right.y = 0f;
        forward.y = 0f;

        Vector3 movement = right.normalized * this.inputManager.GetInputMovement().x + forward.normalized * this.inputManager.GetInputMovement().y;
        */

        float forward = this.inputManager.GetInputMovement().y;
        //float rotation = this.inputManager.GetInputMovement().x;
        float right = this.inputManager.GetInputMovement().x;

        Vector3 next = new Vector3(right, 0f, forward) * Time.deltaTime * movementSpeed;
        //next += Physics.gravity * Time.deltaTime;

        //transform.Rotate(new Vector3(0f, rotation * Time.deltaTime * rotationSpeed, 0f));
        this.characterController.Move(transform.TransformDirection(next));
    }

    private void Look(Vector2 lookAxis)
    {
        float deltaTime = Time.deltaTime;

        float followOffset = Mathf.Clamp(
            transposer.m_FollowOffset.y - (lookAxis.y * cameraVelocity.y * deltaTime),
            maxFollowOffset.x,
            maxFollowOffset.y);

        transposer.m_FollowOffset.y = followOffset;

        transform.Rotate(0f, lookAxis.x * cameraVelocity.x * deltaTime, 0f);
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