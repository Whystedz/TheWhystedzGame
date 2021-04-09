using System.Collections;
using UnityEngine;
using Cinemachine;
using System.Linq;
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
    private bool isRunning;

    // Camera vars
    private GameObject virtualCamera;
    
    [Header("Falling")]
    [SerializeField] private float fallingSpeed = 10f;
    private bool isFalling;
    private Image blackoutImage;
    [SerializeField] private float timeToFadeIn = 0.25f;
    [SerializeField] private float timeToFadeOut = 2f;

    public bool IsClimbing { get; private set; }
    public bool IsInUnderground { get; set; }
    [SerializeField] private float undergroundCheckThreshold = 2f;
    [SerializeField] private float heightOffset = 1f;

    private GameObject surface;
    private GameObject underground;

    [SerializeField] private float spawningCollisionRadiusToCheck = 1.2f;
    private NetworkLoseCrystals loseCrystals;

    private float disabledMovementCooldown;

    private NetworkTile tileCurrentlyOn;
    private bool tileCurrentlyOnUpdatedThisFrame;

    private PlayerAudio playerAudio;

    public override void OnStartAuthority()
    {
        this.characterController = GetComponent<CharacterController>();

        SetCamera();

        this.surface = GameObject.FindGameObjectWithTag("Surface");
        this.underground = GameObject.FindGameObjectWithTag("Underground");

        IsInUnderground = false;

        this.loseCrystals = GetComponent<NetworkLoseCrystals>();

        var blackoutImageGO = GameObject.FindGameObjectWithTag("BlackoutImage");
        if (blackoutImageGO != null)
            this.blackoutImage = blackoutImageGO.GetComponent<Image>();
        else
            Debug.LogWarning("Please add a Blackout Image (a prefab) to the GUI canvas!");

        this.playerAudio = GetComponent<PlayerAudio>();
    }

    public void SetCamera()
    {
        this.virtualCamera = FindObjectsOfType<CinemachineVirtualCamera>(true)[0].gameObject;
        
        this.virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
        this.virtualCamera.GetComponent<CinemachineVirtualCamera>().LookAt = this.transform;

        this.virtualCamera.SetActive(true);
    }

    private void Start() => RefreshTileCurrentlyOn();

    void Update()
    {
        if (base.hasAuthority)
        {
            RefreshTileCurrentlyOn();

            if (IsMovementDisabled)
            {
                if (this.disabledMovementCooldown == -1) return; // infinite cooldown

                this.disabledMovementCooldown -= Time.deltaTime;

                if (disabledMovementCooldown <= 0)
                    EnableMovement();

                return;
            }

            PlayerMovementUpdate();

            this.tileCurrentlyOnUpdatedThisFrame = false;
        }
    }

    void PlayerMovementUpdate()
    {
        UpdateUndergroundSoundFX();

        CheckIfFalling();

        if (this.isFalling)
            RegularMovement();
    }

    private void RegularMovement()
    {
        this.direction = new Vector3(InputManager.Instance.GetInputMovement().x, 0f, InputManager.Instance.GetInputMovement().y);
        this.characterController.Move(this.direction * Time.deltaTime * this.movementSpeed);

        this.isRunning = (Mathf.Abs(this.characterController.velocity.x) > 0 || Mathf.Abs(this.characterController.velocity.z) > 0) ? true : false;
        this.animator.SetBool("isRunning", isRunning);

        if (this.direction != Vector3.zero)
            transform.forward = this.direction;
    }

    private void CheckIfFalling()
    {
        if (this.IsInUnderground || this.isFalling)
            return;

        this.isFalling = tileCurrentlyOn is null
            || tileCurrentlyOn.TileInfo.TileState == TileState.Respawning
            || tileCurrentlyOn.TileInfo.TileState == TileState.Rope;

        if (this.isFalling)
            StartFalling();
    }

    private void StartFalling() => StartCoroutine(Fall());

    private void UpdateUndergroundSoundFX()
    {
        if(IsInUnderground)
            AudioManager.PlayUndergroundFX();
        else
            AudioManager.StopUndergroundFX();
    }

    public bool IsFalling() => this.isFalling;
    public void MoveTowards(Vector3 direction, float speed) => this.characterController.Move(direction * Time.deltaTime * speed);

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


    public void StartClimbing(GameObject rope, float height)
    {
        StartCoroutine(ClimbRope(rope, height));
    }

    public IEnumerator ClimbRope(GameObject rope, float height)
    {
        IsMovementDisabled = true;
        IsClimbing = true;
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

        IsClimbing = false;
        IsMovementDisabled = false;
        IsInUnderground = false;
    }


    public IEnumerator TransitionToTop(float height, Vector3 surfacePosition)
    {
        this.animator.SetBool("isClimbing", true);
        StartCoroutine(FadeOut(2f));
        while (this.transform.position.y < underground.transform.position.y + height)
        {
            this.characterController.Move(Vector3.up * Time.deltaTime * this.movementSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        this.transform.position = surfacePosition + Vector3.up * this.heightOffset;
        this.animator.SetBool("isClimbing", false);
        yield return StartCoroutine(FadeIn(2f));
        this.animator.SetTrigger("Reset");
    }

    public IEnumerator FadeIn(float timeToFadeOut)
    {
        for (float opacity = timeToFadeIn; opacity >= 0; opacity -= Time.deltaTime)
        {
            this.blackoutImage.color = new Color(0, 0, 0, opacity);
            yield return null;
        }
        this.blackoutImage.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator FadeOut(float timeToFadeIn)
    {
        for (float opacity = 0; opacity <= timeToFadeOut; opacity += Time.deltaTime)
        {
            this.blackoutImage.color = new Color(0, 0, 0, opacity);
            yield return null;
        }
        this.blackoutImage.color = new Color(0, 0, 0, 1);
    }

    public IEnumerator Fall()
    {
        var initialPosition = this.transform.position;

        yield return StartCoroutine(FadeOut(this.timeToFadeOut));

        var offset = initialPosition.y - this.surface.transform.position.y;
        var fallenPosition = FindAFallingPosition(initialPosition, offset);
        
        this.characterController.enabled = false;
        this.transform.position = fallenPosition;
        this.characterController.enabled = true;

        this.isFalling = false;
        IsInUnderground = true;

        this.virtualCamera.SetActive(false);
        this.virtualCamera.SetActive(true);

        this.loseCrystals.LoseCrystal();
        yield return StartCoroutine(FadeIn(this.timeToFadeIn));
    }

    internal void DisableMovementFor(float seconds)
    {
        DisableMovement();
        this.disabledMovementCooldown = seconds;
    }

    internal void EnableMovement()
    {
        this.disabledMovementCooldown = -1; // set to infinite
        IsMovementDisabled = false;
    }

    internal void DisableMovement()
    {
        this.disabledMovementCooldown = -1; // set to infinite
        IsMovementDisabled = true;
    }

    [Command(ignoreAuthority = true)]
    public void CmdDisableMovement()
    {
        this.disabledMovementCooldown = -1; // set to infinite
        IsMovementDisabled = true;
    }

    public void RefreshTileCurrentlyOn()
    {
        this.tileCurrentlyOn = NetworkTile.FindTileAtPosition(transform.position);
        this.tileCurrentlyOnUpdatedThisFrame = true;
    }

    public NetworkTile TileCurrentlyOn() 
    {
        if (!this.tileCurrentlyOnUpdatedThisFrame)
            RefreshTileCurrentlyOn();

        return this.tileCurrentlyOn;
    }

    private Vector3 FindAFallingPosition(Vector3 initialPosition, float offset)
    {
        int attempts = 0;
        var fallenPosition = new Vector3(
                    initialPosition.x,
                    this.underground.transform.position.y + offset,
                    initialPosition.z);

        var obstacleColliders = new Collider[50];
        int numColliders = GetNumberOfColliders(fallenPosition, obstacleColliders);

        if (numColliders > 0)
            //Debug.LogWarning($"Fell, but path was obstructed. Will try to find a new position to land at. Initial Position was {initialPosition}");

        while (numColliders >= 1 && attempts++ < 10)
            TryAnotherPosition(initialPosition, offset, out fallenPosition, obstacleColliders, out numColliders);

        if (attempts >= 10)
        {
            //Debug.LogWarning($"Was unable to find a solid position to land at. Initial Position was {initialPosition}");
            fallenPosition = new Vector3(0, this.underground.transform.position.y + offset, 0);
        }

        return fallenPosition;
    }

    private void TryAnotherPosition(Vector3 initialPosition, float offset, out Vector3 fallenPosition, Collider[] obstacleColliders, out int numColliders)
    {
        float zOffset = UnityEngine.Random.Range(1, 6);
        float xOffset = UnityEngine.Random.Range(1, 6);

        fallenPosition = new Vector3(
            initialPosition.x + xOffset,
            this.underground.transform.position.y + offset,
            initialPosition.z + zOffset);

        numColliders = GetNumberOfColliders(fallenPosition, obstacleColliders);
    }

    private int GetNumberOfColliders(Vector3 fallenPosition, Collider[] obstacleColliders)
    {
        return Physics.OverlapSphereNonAlloc(
            fallenPosition,
            this.spawningCollisionRadiusToCheck,
            obstacleColliders,
            1 << LayerMask.NameToLayer("Default"));
    }
}
