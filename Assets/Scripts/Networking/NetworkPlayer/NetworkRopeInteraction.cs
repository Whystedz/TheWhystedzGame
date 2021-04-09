using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class NetworkRopeInteraction : NetworkBehaviour
{
    [SerializeField] private NetworkRope rope;
    [SyncVar(hook = nameof(OnRopeUsed))]
    public bool IsRopeInUse = false;
    [SyncVar]
    public RopeState RopeState = RopeState.Normal;

    private NetworkPlayerMovement playerMovement;
    [SerializeField] private float heightToClimb = 8f;
    private Teammate team;

    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private float speedTowardsRope = 6.0f;

    [Header("Ropping")]
    private float distanceFromTileToHoldRope = 1.8f;
    [SerializeField] private float maxDistanceToRopeTile = 2.5f;
    [SerializeField] private float afterRopePause = 0.5f;

    private NetworkTile lastTileHighlighted;
    private int tileLayerMask;

    private PlayerAudio playerAudio;
    private TileManager tileManager;

    public NetworkTile RopeTile { get; private set; }

    public bool IsHoldingRope { get; private set; }

    [SerializeField] private bool isClientPlayer;

    private void OnRopeUsed(bool oldValue, bool newValue) => this.rope.gameObject.SetActive(newValue);

    void Awake()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.playerAudio = this.GetComponent<PlayerAudio>();
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();
        this.tileManager = TileManager.GetInstance();

        this.team = GetComponent<Teammate>();

        if (base.hasAuthority)
            isClientPlayer = true;
    }

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
                tileCurrentlyOn.TileInfo.TileState = TileState.Respawning;

            return;
        }

        DetermineRopeTile();

        if (RopeTile != null 
            && !NetworkInputManager.Instance.GetLadder())
        {
            PreviewRope();
            return;
        }

        if (NetworkInputManager.Instance.GetLadder())
            DetermineRopeAction();
    }

    [Command]
    public void CmdUseRope(bool isUsing)
    {
        IsRopeInUse = isUsing;
    }

    [Command(ignoreAuthority = true)]
    public void CmdSetRopeState(RopeState newState)
    {
        RopeState = newState;
    }

    [Command]
    public void CmdSetTileState(TileInfo targetTile, TileState newState, float newProgress)
    {
        this.tileManager.SetTileState(targetTile, newState, newProgress);
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

    private NetworkTile GetClosestTileInSphere()
    {
        var colliders = Physics.OverlapSphere(transform.position,
            this.maxDistanceToRopeTile,
            tileLayerMask);

        return colliders
            .Select(collider => collider.GetComponentInParent<NetworkTile>())
            .Where(tile => tile.TileInfo.TileState == TileState.Respawning)
            .OrderBy(tile => Vector3.Distance(transform.position, tile.transform.position))
            .FirstOrDefault();
    }

    private NetworkTile GetClosestTileDirectlyInFront()
    {
        var hits = Physics.RaycastAll(transform.position,
            transform.TransformDirection(Vector3.forward),
            this.maxDistanceToRopeTile,
            tileLayerMask
        );

        return hits
            .Select(hit => hit.collider.GetComponentInParent<NetworkTile>())
            .Where(tile => tile.TileInfo.TileState == TileState.Respawning)
            .OrderBy(tile => Vector3.Distance(transform.position, tile.transform.position))
            .FirstOrDefault();
    }

    public void DetermineRopeAction() // rename
    {
        if (RopeTile is null)
            return;

        if (RopeTile.TileInfo.TileState == TileState.Normal
            || RopeTile.TileInfo.TileState == TileState.Unbreakable
            || RopeTile.TileInfo.TileState == TileState.Unstable)
            return;

        if (RopeState == RopeState.Normal 
            && this.rope.gameObject.activeSelf 
            && RopeTile.TileInfo.TileState == TileState.Rope)
        {
            HaulUpRope();
            return;
        }

        if (RopeState == RopeState.Normal &&
                 !this.rope.gameObject.activeSelf 
                 && RopeTile.TileInfo.TileState == TileState.Respawning)
        {
            playerMovement.DisableMovement();
            this.IsHoldingRope = true;

            CmdSetTileState(RopeTile.TileInfo, TileState.Rope, RopeTile.TileInfo.Progress);
            StartCoroutine(ThrowRope(this.rope, RopeTile));
        }
    }

    private void HaulUpRope()
    {   
        playerAudio.PlayRopeAudio();
        CmdSetTileState(RopeTile.TileInfo, TileState.Respawning, RopeTile.TileInfo.Progress);
        StartCoroutine(RemoveRope());

        this.playerMovement.EnableMovement();
        this.IsHoldingRope = false;
    }

    public IEnumerator RemoveRope()
    {
        this.rope.SetRopeState(RopeState.Normal);
        while (RopeState != RopeState.Normal)
            yield return null;

        CmdUseRope(false);
    }

    public IEnumerator ThrowRope(NetworkRope rope, NetworkTile tile)
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
        CmdUseRope(true);

        rope.GetUpperLadder().transform.forward = Vector3.back;
        rope.GetLowerLadder().transform.forward = Vector3.back;
        rope.GetHighlightedLadder().transform.forward = Vector3.back;

        yield return null;
    }
}
