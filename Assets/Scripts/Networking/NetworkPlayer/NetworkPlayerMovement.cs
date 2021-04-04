using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using Mirror;

public class NetworkPlayerMovement : NetworkBehaviour
{
    private CharacterController characterController;
    private InputManager inputManager;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 3f;
    private Vector3 direction;
    public bool IsMovementDisabled;
    [SerializeField] private Animator animator;

    // Camera vars
    private GameObject virtualCamera;
    private GameObject fallingCamera;
    
    private int tileLayerMask;
    private int groundLayerMask;
    
    [Header("Falling")]
    [SerializeField] private float fallingSpeed = 10f;
    private bool isFalling;
    private Image blackoutImage;
    [SerializeField] private float timeToFadeIn = 2f;

    public bool isInUnderground;
    [SerializeField] private float undergroundCheckThreshold = 1.5f;
    [SerializeField] private float heightOffset = 1.3f;

    private GameObject surface;
    private GameObject underground;

    public override void OnStartAuthority()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.groundLayerMask = LayerMask.GetMask("Tile") | LayerMask.GetMask("Underground");
        
        this.characterController = GetComponent<CharacterController>();
        this.fallingCamera = FindObjectsOfType<CinemachineVirtualCamera>(true)[1].gameObject;
        this.virtualCamera = FindObjectsOfType<CinemachineVirtualCamera>(true)[0].gameObject;
        
        this.virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
        this.virtualCamera.GetComponent<CinemachineVirtualCamera>().LookAt = this.transform;

        this.fallingCamera.GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
        this.fallingCamera.GetComponent<CinemachineVirtualCamera>().LookAt = this.transform;

        this.virtualCamera.SetActive(true);
        this.fallingCamera.SetActive(false);
        
        this.surface = GameObject.FindGameObjectWithTag("Surface");
        this.underground = GameObject.FindGameObjectWithTag("Underground");

        var blackoutImageGO = GameObject.FindGameObjectWithTag("BlackoutImage");
        if (blackoutImageGO != null)
            this.blackoutImage = blackoutImageGO.GetComponent<Image>();
        else
            Debug.LogWarning("Please add a Blackout Image (a prefab) to the GUI canvas!");
        
        this.inputManager = InputManager.GetInstance();
    }

    void Update()
    {
        if (base.hasAuthority)
        {
            if (IsMovementDisabled)
                return;

            PlayerMovementUpdate();
        }
    }

    void PlayerMovementUpdate()
    {
        CheckIfUnderground();

        CheckIfFalling();

        if (this.isFalling)
            FallingMovementUpdate();
        else
            RegularMovement();
    }

    private void RegularMovement()
    {
        if (this.fallingCamera.activeSelf)
            this.fallingCamera.SetActive(false);

        this.direction = new Vector3(this.inputManager.GetInputMovement().x, 0f, this.inputManager.GetInputMovement().y);
        this.characterController.Move(this.direction * Time.deltaTime * this.movementSpeed);

        if (Mathf.Abs(characterController.velocity.x) > 0 || Mathf.Abs(characterController.velocity.z) > 0)
            animator.SetBool("isRunning", true);
        else
            animator.SetBool("isRunning", false);

        if (this.direction != Vector3.zero)
            transform.forward = this.direction;
    }

    private void FallingMovementUpdate()
    {
        if (!this.fallingCamera.activeSelf)
            this.fallingCamera.SetActive(true);

        var fallingVelocity = Vector3.down * Time.deltaTime * this.fallingSpeed;
        this.characterController.Move(fallingVelocity);
    }

    private void CheckIfFalling()
    {
        if (this.isInUnderground)
        {
            this.isFalling = false;
            return;
        }

        var tileColliders = Physics.OverlapSphere(transform.position, 0.01f, tileLayerMask);
        Collider closestTileCollider = null;

        if (tileColliders.Length == 0)
        {
            this.isFalling = true;
            return;
        }
        else if (tileColliders.Length == 1)
        {
            closestTileCollider = tileColliders[0];

        }
        else if (tileColliders.Length > 1)
        {
            closestTileCollider = GetClosestCollider(tileColliders);
        }

        var tile = closestTileCollider.transform.parent.gameObject.GetComponent<NetworkTile>();
        this.isFalling = tile.TileInfo.TileState == TileState.Respawning
            || tile.TileInfo.TileState == TileState.Rope;
    }

    private void CheckIfUnderground()
    {
        var distanceToUnderground = transform.position.y - this.underground.transform.position.y;
        distanceToUnderground = Mathf.Abs(distanceToUnderground);
        this.isInUnderground = distanceToUnderground <= this.undergroundCheckThreshold;
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

    public IEnumerator ClimbRope(float height, Vector3 surfacePosition)
    {
        StartCoroutine(FadeIn());
        while (this.transform.position.y < underground.transform.position.y + height)
        {
            this.characterController.Move(Vector3.up * Time.deltaTime * this.movementSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        this.transform.position = surfacePosition + Vector3.up * this.heightOffset;
        yield return StartCoroutine(FadeOut());  
    }

    public IEnumerator FadeIn()
    {
        for (float opacity = 0; opacity <= timeToFadeIn; opacity += Time.deltaTime)
        {
            // set color with i as alpha
            this.blackoutImage.color = new Color(0, 0, 0, opacity);
            yield return null;
        }
    }

    public IEnumerator FadeOut()
    {
        for (float opacity = timeToFadeIn; opacity >= 0; opacity -= Time.deltaTime)
        {
            // set color with i as alpha
            this.blackoutImage.color = new Color(0, 0, 0, opacity);
            yield return null;
        }
    }
}