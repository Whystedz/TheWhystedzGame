using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using Mirror;

public class NetworkPlayerMovement : NetworkBehaviour
{
    private CharacterController characterController;

    [Header("Movement")]
    [SerializeField] private Animator animator;
    [SerializeField] private float movementSpeed = 3f;
    private Vector3 direction;
    public bool IsMovementDisabled;
    private bool isRunning = false;

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

    public bool IsInUnderground { get; private set; }
    [SerializeField] private float undergroundCheckThreshold = 1.5f;
    [SerializeField] private float heightOffset = 1.3f;

    private GameObject surface;
    private GameObject underground;

    private PlayerAudio playerAudio;

    public override void OnStartAuthority()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.groundLayerMask = LayerMask.GetMask("Tile") | LayerMask.GetMask("Underground");
        
        this.characterController = GetComponent<CharacterController>();

        SetCamera();

        this.surface = GameObject.FindGameObjectWithTag("Surface");
        this.underground = GameObject.FindGameObjectWithTag("Underground");

        var blackoutImageGO = GameObject.FindGameObjectWithTag("BlackoutImage");
        if (blackoutImageGO != null)
            this.blackoutImage = blackoutImageGO.GetComponent<Image>();
        else
            Debug.LogWarning("Please add a Blackout Image (a prefab) to the GUI canvas!");

        this.playerAudio = GetComponent<PlayerAudio>();
    }

    public void SetCamera()
    {
        this.fallingCamera = FindObjectsOfType<CinemachineVirtualCamera>(true)[1].gameObject;
        this.virtualCamera = FindObjectsOfType<CinemachineVirtualCamera>(true)[0].gameObject;
        
        this.virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
        this.virtualCamera.GetComponent<CinemachineVirtualCamera>().LookAt = this.transform;

        this.fallingCamera.GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
        this.fallingCamera.GetComponent<CinemachineVirtualCamera>().LookAt = this.transform;

        this.virtualCamera.SetActive(true);
        this.fallingCamera.SetActive(false);
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

        this.direction = new Vector3(InputManager.Instance.GetInputMovement().x, 0f, InputManager.Instance.GetInputMovement().y);
        this.characterController.Move(this.direction * Time.deltaTime * this.movementSpeed);

        this.isRunning = (Mathf.Abs(this.characterController.velocity.x) > 0 || Mathf.Abs(this.characterController.velocity.z) > 0) ? true : false;
        this.animator.SetBool("isRunning", isRunning);

        if (this.direction != Vector3.zero)
            transform.forward = this.direction;
    }

    private void FallingMovementUpdate()
    {
        this.animator.SetTrigger("Fall");

        if (!this.fallingCamera.activeSelf)
            this.fallingCamera.SetActive(true);

        var fallingVelocity = Vector3.down * Time.deltaTime * this.fallingSpeed;
        this.characterController.Move(fallingVelocity);
    }

    private void CheckIfFalling()
    {
        if (this.IsInUnderground)
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

        if(IsInUnderground && distanceToUnderground > this.undergroundCheckThreshold)
            AudioManager.StopUndergroundFX();
        else if(!IsInUnderground && distanceToUnderground <= this.undergroundCheckThreshold)
            AudioManager.PlayUndergroundFX();

        this.IsInUnderground = distanceToUnderground <= this.undergroundCheckThreshold;
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

    public void StartClimbing(GameObject rope, float height)
    {
        StartCoroutine(ClimbRope(rope, height));
    }

    public IEnumerator ClimbRope(GameObject rope, float height)
    {
        IsMovementDisabled = true;
        var directionToRope = (rope.transform.position - this.transform.position).normalized;
        directionToRope = new Vector3(directionToRope.x, 0,directionToRope.z);
        var ropePositionWithoutY = new Vector3(rope.transform.position.x, this.transform.position.y, rope.transform.position.z);
        this.transform.LookAt(ropePositionWithoutY);

        while (Vector3.Distance(this.transform.position, ropePositionWithoutY) > 1.0f)
        {
            MoveTowards(directionToRope, 120f);
            yield return null;
        }
        this.transform.LookAt(ropePositionWithoutY);

        yield return StartCoroutine(TransitionToTop(height, rope.transform.position));
        rope.GetComponent<NetworkRope>().SetRopeState(RopeState.Saved);
        // In reality this would be the animation delay
        yield return new WaitForSecondsRealtime(0.5f);
        IsMovementDisabled = false;
    }

    public IEnumerator TransitionToTop(float height, Vector3 surfacePosition)
    {
        this.animator.SetBool("isClimbing", true);
        StartCoroutine(FadeIn());
        while (this.transform.position.y < underground.transform.position.y + height)
        {
            this.characterController.Move(Vector3.up * Time.deltaTime * this.movementSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        this.transform.position = surfacePosition + Vector3.up * this.heightOffset;
        this.animator.SetBool("isClimbing", false);
        yield return StartCoroutine(FadeOut());
        this.animator.SetTrigger("Reset");
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
