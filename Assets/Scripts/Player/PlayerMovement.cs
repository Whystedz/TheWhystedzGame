using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using System;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private InputManager inputManager;

    [Header("Movement")]
    [SerializeField]
    private float movementSpeed = 3f;
    private Vector3 direction;
    public bool IsMovementDisabled;

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

    private LoseCrystals loseCrystals;

    private float disabledMovementCooldown;

    private Tile tileCurrentlyOn;
    private bool tileCurrentlyOnUpdatedThisFrame;

    void Awake()
    {        
        this.characterController = GetComponent<CharacterController>();
        this.virtualCamera = FindObjectsOfType<CinemachineVirtualCamera>(true)[0].gameObject;

        this.virtualCamera.SetActive(true);

        this.surface = GameObject.FindGameObjectWithTag("Surface");
        this.underground = GameObject.FindGameObjectWithTag("Underground");

        this.loseCrystals = GetComponent<LoseCrystals>();

        var blackoutImageGO = GameObject.FindGameObjectWithTag("BlackoutImage");
        if (blackoutImageGO != null)
            this.blackoutImage = blackoutImageGO.GetComponent<Image>();
        else
            Debug.LogWarning("Please add a Blackout Image (a prefab) to the GUI canvas!");

        IsInUnderground = false;
    }

    private void Start()
    {
        this.inputManager = InputManager.GetInstance();
        RefreshTileCurrentlyOn();
    }

    void Update()
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

    private void RegularMovement()
    {
        this.direction = new Vector3(this.inputManager.GetInputMovement().x, 0f, this.inputManager.GetInputMovement().y);
        this.characterController.Move(this.direction * Time.deltaTime * this.movementSpeed);

        if (this.direction != Vector3.zero)
            transform.forward = this.direction;
    }

    void PlayerMovementUpdate()
    {
        UpdateUndergroundSoundFX();

        CheckIfFalling();

        if (!this.isFalling)
            RegularMovement();
    }

    private void CheckIfFalling()
    {
        if (this.IsInUnderground || this.isFalling)
            return;

        this.isFalling = tileCurrentlyOn is null
            || tileCurrentlyOn.tileState == TileState.Respawning
            || tileCurrentlyOn.tileState == TileState.Rope;

        if (this.isFalling)
            StartFalling();
    }

    private void StartFalling()
    {
        StartCoroutine(Fall());
    }

    private void UpdateUndergroundSoundFX()
    {
        if(IsInUnderground)
            AudioManager.PlayUndergroundFX();
        else
            AudioManager.StopUndergroundFX();
    }

    public bool IsFalling() => this.isFalling;
    public void MoveTowards(Vector3 direction, float speed) => this.characterController.Move(direction * Time.deltaTime * speed);

    public IEnumerator ClimbRope(float height,Vector3 surfacePosition)
    {
        IsClimbing = true;

        StartCoroutine(FadeOut(2f));
        while (this.transform.position.y < underground.transform.position.y + height)
        {
            this.characterController.Move(Vector3.up * Time.deltaTime * this.movementSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        this.transform.position = surfacePosition + Vector3.up * this.heightOffset;
        yield return StartCoroutine(FadeIn(2f));

        this.tileCurrentlyOn.tileState = TileState.Normal;
        IsClimbing = false;
        this.IsMovementDisabled = false;
        this.IsInUnderground = false;
    }

    public IEnumerator FadeOut(float timeToFadeOut)
    {
        for (float opacity = 0; opacity <= timeToFadeOut; opacity += Time.deltaTime)
        {
            this.blackoutImage.color = new Color(0, 0, 0, opacity);
            yield return null;
        }
        this.blackoutImage.color = new Color(0, 0, 0, 1);
    }

    public IEnumerator FadeIn(float timeToFadeIn)
    {
        for (float opacity = timeToFadeIn; opacity >= 0; opacity -= Time.deltaTime)
        {
            this.blackoutImage.color = new Color(0, 0, 0, opacity);
            yield return null;
        }
        this.blackoutImage.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator Fall()
    {
        var initialPosition = this.transform.position;

        yield return StartCoroutine(FadeOut(this.timeToFadeOut));

        var offset = initialPosition.y - this.surface.transform.position.y;
        var fallenPosition = new Vector3(
            initialPosition.x,
            this.underground.transform.position.y + offset,
            initialPosition.z);

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

    public void DisableMovementFor(float seconds)
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

    public void RefreshTileCurrentlyOn()
    {
        this.tileCurrentlyOn = Tile.FindTileAtPosition(transform.position);

        this.tileCurrentlyOnUpdatedThisFrame = true;
    }

    public Tile TileCurrentlyOn() {
        if (!this.tileCurrentlyOnUpdatedThisFrame)
            RefreshTileCurrentlyOn();

        return this.tileCurrentlyOn;
    }
}