using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RopeInteractions : MonoBehaviour
{
    private InputManager inputManager;
    private PlayerMovement playerMovement;
    [SerializeField] private float heightToClimb = 8f;
    private Teammate team;
    internal RopeState ropeState;

    [SerializeField] private Rope rope;
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private float speedTowardsRope = 6.0f;

    [Header("Ropping")]
    private float distanceFromTileToHoldRope = 1.8f;
    [SerializeField] private float maxDistanceToRopeTile = 2.5f;
    [SerializeField] private float afterRopePause = 0.5f;

    private Tile lastTileHighlighted;
    private int tileLayerMask;

    private PlayerAudio playerAudio;

    public Tile RopeTile { get; private set; }

    public bool IsHoldingRope { get; private set; }

    [SerializeField] private bool isClientPlayer;

    void Awake()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.playerAudio = this.GetComponent<PlayerAudio>();
        this.playerMovement = this.GetComponent<PlayerMovement>();

        ropeState = RopeState.Normal;
        this.team = this.transform.parent.GetComponent<Teammate>();
    }

    void Start() => inputManager = InputManager.GetInstance();

    void Update()
    {
        if (!isClientPlayer)
            return;

        ResetRopePreviewHighlighting();

        var tileCurrentlyOn = this.playerMovement.TileCurrentlyOn();

        if (playerMovement.IsFalling())
        {
            this.rope.gameObject.SetActive(false);

            if (tileCurrentlyOn != null)
                tileCurrentlyOn.tileState = TileState.Respawning;

            return;
        }

        DetermineRopeTile();

        if (RopeTile != null 
            && !inputManager.GetInitiateCombo())// TODO Rope input
        {
            PreviewRope();
            return;
        }

        if (inputManager.GetInitiateCombo())
            DetermineRopeAction();

        //if (this.inputManager.GetInitiateCombo() && ropeState != RopeState.InUse && this.inZone) //TOD rope input
        //{
        //    StartCoroutine(ClimbRope());
        //}
    }

    private void PreviewRope()
    {
        RopeTile.HighlightTileRopePreview();
        lastTileHighlighted = RopeTile;
        return;
    }

    private void ResetRopePreviewHighlighting()
    {
        if (this.lastTileHighlighted != null)
            this.lastTileHighlighted.ResetHighlighting();

        this.lastTileHighlighted = null;
    }

    private void DetermineRopeTile()
    {
        if (this.IsHoldingRope)
            return; // it'll be the same tile

        var closestTileDirectlyInFront = GetClosestTileDirectlyInFront();
        if (closestTileDirectlyInFront != null)
        {
            RopeTile = closestTileDirectlyInFront;
            return;
        }

        RopeTile = GetClosestTileInSphere();
    }

    private Tile GetClosestTileInSphere()
    {
        var colliders = Physics.OverlapSphere(transform.position,
            this.maxDistanceToRopeTile,
            tileLayerMask);

        return colliders
            .Select(collider => collider.GetComponentInParent<Tile>())
            .Where(tile => tile.tileState == TileState.Respawning)
            .OrderBy(tile => Vector3.Distance(transform.position, tile.transform.position))
            .FirstOrDefault();
    }

    private Tile GetClosestTileDirectlyInFront()
    {
        var hits = Physics.RaycastAll(transform.position,
            transform.TransformDirection(Vector3.forward),
            this.maxDistanceToRopeTile,
            tileLayerMask
        );

        return hits
            .Select(hit => hit.collider.GetComponentInParent<Tile>())
            .Where(tile => tile.tileState == TileState.Respawning)
            .OrderBy(tile => Vector3.Distance(transform.position, tile.transform.position))
            .FirstOrDefault();
    }

    public void DetermineRopeAction() // rename
    {
        if (RopeTile is null)
            return;

        if (RopeTile.tileState == TileState.Normal
            || RopeTile.tileState == TileState.Unbreakable
            || RopeTile.tileState == TileState.Unstable)
            return;

        if (rope.ropeState == RopeState.Normal 
            && this.rope.gameObject.activeSelf 
            && RopeTile.tileState == TileState.Rope)
        {
            HaulUpRope();
            return;
        }

        if (rope.ropeState == RopeState.Normal &&
                 !this.rope.gameObject.activeSelf 
                 && RopeTile.tileState == TileState.Respawning)
        {
            playerMovement.DisableMovement();

            RopeTile.tileState = TileState.Rope;

            this.IsHoldingRope = true;

            StartCoroutine(ThrowRope(this.rope, RopeTile));
        }
    }

    private void HaulUpRope()
    {
        playerAudio.PlayRopeAudio();

        rope.ropeState = RopeState.Normal;

        RopeTile.tileState = TileState.Respawning;

        this.rope.gameObject.SetActive(false);

        this.playerMovement.EnableMovement();

        this.IsHoldingRope = false;
    }

    public IEnumerator ThrowRope(Rope rope, Tile tile)
    {
        playerAudio.PlayRopeAudio();
        tile.ResetHighlighting();

        var ropeObject = rope.gameObject;
        var tileSurfacePosition = new Vector3(tile.transform.position.x, this.transform.position.y, tile.transform.position.z);
        this.transform.LookAt(tileSurfacePosition);

        while (Vector3.Distance(this.transform.position, tileSurfacePosition) > distanceFromTileToHoldRope)
        {
            playerMovement.MoveTowards(ropeObject.transform.forward, speedTowardsRope);
            yield return null;
        } // TODO maybe no playerMovement, 
        playerMovement.MoveTowards(Vector3.zero, 0);

        ropeObject.transform.position = new Vector3(tile.transform.position.x, ropeObject.transform.position.y, tile.transform.position.z);
        ropeObject.SetActive(true);

        rope.GetUpperLadder().transform.forward = Vector3.back;
        rope.GetLowerLadder().transform.forward = Vector3.back;
        rope.GetHighlightedLadder().transform.forward = Vector3.back;

        yield return null;
    }
}
