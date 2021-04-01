using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private InputManager inputManager;

    [SerializeField]
    private float movementSpeed = 3f;
    private Vector3 direction;

    // Camera vars
    private GameObject virtualCamera;
    private GameObject fallingCamera;
    private int layerMask;
    public bool isMovementDisabled;
    // Falling vars
    [SerializeField] private float fallingVelocity = 10f;
    private bool isFalling;
    public bool isInUnderground;
    [SerializeField] private Image img;
    [SerializeField] private float timeToFadeIn = 2f;
    void Awake()
    {
        layerMask = LayerMask.GetMask("TileMovementCollider") | LayerMask.GetMask("Underground");
        this.characterController = GetComponent<CharacterController>();
        this.fallingCamera = FindObjectsOfType<CinemachineVirtualCamera>(true)[1].gameObject;
        this.virtualCamera = FindObjectsOfType<CinemachineVirtualCamera>(true)[0].gameObject;

        this.virtualCamera.SetActive(true);
        this.fallingCamera.SetActive(false);

    }

    private void Start()
    {
        this.inputManager = InputManager.GetInstance();
    }

    void Update()
    {
        if (isMovementDisabled)
            return;

        if (HasFallenInHole())
            FallingMovementUpdate();
        else
            RegularMovement();
    }

   
    private void RegularMovement()
    {
        if (isFalling)
        {
            this.virtualCamera.SetActive(true);
            this.fallingCamera.SetActive(false);
        }

        this.isFalling = false;

        MovePlayer();
    }

    void MovePlayer()
    {
        this.direction = new Vector3(this.inputManager.GetInputMovement().x, 0f, this.inputManager.GetInputMovement().y);
        this.characterController.Move(this.direction * Time.deltaTime * this.movementSpeed);

        if (this.direction != Vector3.zero)
            transform.forward = this.direction;
    }

    private void FallingMovementUpdate()
    {
        if (!isFalling)
        {
            this.fallingCamera.SetActive(true);
            isFalling = true;
        }

        var movementPerUpdate = Vector3.down * Time.deltaTime * this.fallingVelocity;
        if (transform.position.y < -10f)
            this.characterController.Move(movementPerUpdate * 6);
        else
            this.characterController.Move(movementPerUpdate);
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
            closestCollider = hitColliders[0];   

        else if (hitColliders.Length > 1)
            closestCollider = GetClosestCollider(hitColliders);

        if (closestCollider.gameObject.layer == LayerMask.NameToLayer("Underground"))
            isInUnderground = true;
        else
            isInUnderground = false;

        if (this.transform.position.y < -20)
        {
            return closestCollider == null;
        }
        else
        {
            var tile = closestCollider.transform.parent.gameObject.GetComponent<Tile>();
            return tile.tileState == TileState.Respawning;
        }
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
    public void MoveTowards(Vector3 direction, float speed) => this.characterController.Move(direction * Time.deltaTime * speed);

    public IEnumerator ClimbRope(float height,Vector3 surfacePosition)
    {
        StartCoroutine(FadeIn());
        while (this.transform.position.y < -70 + height)
        {
            this.characterController.Move(Vector3.up * Time.deltaTime * this.movementSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        this.transform.position = new Vector3(surfacePosition.x, surfacePosition.y+1f, surfacePosition.z);
        yield return StartCoroutine(FadeOut());
    }

    public IEnumerator FadeIn()
    {
        for (float opacity = 0; opacity <= timeToFadeIn; opacity += Time.deltaTime)
        {
            // set color with i as alpha
            this.img.color = new Color(0, 0, 0, opacity);
            yield return null;
        }
    }

    public IEnumerator FadeOut()
    {
        for (float opacity = timeToFadeIn; opacity >= 0; opacity -= Time.deltaTime)
        {
            // set color with i as alpha
            this.img.color = new Color(0, 0, 0, opacity);
            yield return null;
        }
    }

}