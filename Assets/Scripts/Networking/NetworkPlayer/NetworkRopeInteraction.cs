using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkRopeInteraction : NetworkBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private GameObject rope;
    [SyncVar(hook = nameof(OnRopeUsed))]
    public bool IsRopeInUse = false;
    [SyncVar]
    public RopeState RopeState = RopeState.Normal;

    private NetworkHUDMainButtons hUDMainButtons;

    private NetworkPlayerMovement playerMovement;
    [SerializeField] private float heightToClimb = 8f;
    private Teammate team;
    
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

    private void OnRopeUsed(bool oldValue, bool newValue) => this.rope.SetActive(newValue);

    [SerializeField] private GameObject ropeCanvas;
    private Image Keyimage;
    private Image XBoximage;
    private Image PSimage;
    private Image image;
    private Transform mainCameraTransform;

    void Start()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.playerAudio = this.GetComponent<PlayerAudio>();
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();
        this.hUDMainButtons = FindObjectOfType<NetworkHUDMainButtons>();
        this.tileManager = TileManager.GetInstance();

        this.team = GetComponent<Teammate>();

        this.mainCameraTransform = Camera.main.transform;
        this.ropeCanvas.SetActive(false);
        this.Keyimage = this.ropeCanvas.transform.GetChild(0).GetComponent<Image>();
        this.XBoximage = this.ropeCanvas.transform.GetChild(1).GetComponent<Image>();
        this.PSimage = this.ropeCanvas.transform.GetChild(2).GetComponent<Image>();

        this.image = this.XBoximage; // default

        UpdateControllerScheme();
    }

    void Update()
    {
        if (base.hasAuthority)
        {   
            UpdateRopeHint();

            if (RopeState == RopeState.Saved)
            {
                CmdResetTile(RopeTile.TileInfo);
                StartCoroutine(RemoveRope());
            }

            ResetRopePreviewHighlighting();

            var tileCurrentlyOn = this.playerMovement.TileCurrentlyOn();
            
            if (this.playerMovement.IsFalling())
            {
                CmdUseRope(false);

                if (tileCurrentlyOn != null)
                    CmdSetTileState(tileCurrentlyOn.TileInfo, TileState.Respawning, tileCurrentlyOn.TileInfo.Progress);

                return;
            }

            DetermineRopeTile();

            if (RopeTile != null 
                && !NetworkInputManager.Instance.GetLadder()
                && !this.IsHoldingRope)
            {
                PreviewRope();
                return;
            }

            if (RopeTile != null && NetworkInputManager.Instance.GetLadder())
                DetermineRopeAction();
        }
    }

    private void UpdateControllerScheme()
    {
        if (this.hUDMainButtons is null)
            return;

        if (this.image != null)
            this.image.gameObject.SetActive(false);

        switch (this.hUDMainButtons.ControllerSchemeInUse)
        {
            case ControllerSchemeInUse.PC:
                this.image = this.Keyimage;
                break;
            case ControllerSchemeInUse.XBox:
                this.image = this.XBoximage;
                break;
            case ControllerSchemeInUse.PlayStation:
                this.image = this.PSimage;
                break;
        }
        this.image.gameObject.SetActive(true);
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

    [Command]
    public void CmdResetTile(TileInfo targetTile)
    {
        this.tileManager.ResetTile(targetTile);
    }

    private void PreviewRope()
    {
        if (!this.ropeCanvas.activeSelf)
        {
            UpdateControllerScheme();
            this.ropeCanvas.SetActive(true);
            this.image.color = new Color(this.image.color.r,
                this.image.color.g,
                this.image.color.b,
                1f);
        }

        RopeTile.HighlightTileRopePreview();
        lastTileHighlighted = RopeTile;
        return;
    }

    private void ResetRopePreviewHighlighting()
    {
        if (this.ropeCanvas.activeSelf && !this.IsHoldingRope)
            this.ropeCanvas.SetActive(false);

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
            && this.rope.activeSelf 
            && RopeTile.TileInfo.TileState == TileState.Rope)
        {
            HaulUpRope();
            return;
        }

        if (RopeState == RopeState.Normal &&
                 !this.rope.activeSelf 
                 && RopeTile.TileInfo.TileState == TileState.Respawning)
        {
            playerMovement.DisableMovement();
            IsHoldingRope = true;

            UpdateControllerScheme();
            this.ropeCanvas.SetActive(true);
            this.image.color = new Color(this.image.color.r, 
                this.image.color.g,
                this.image.color.b,
                0.5f); 

            CmdSetTileState(RopeTile.TileInfo, TileState.Rope, RopeTile.TileInfo.Progress);
            StartCoroutine(ThrowRope(this.rope, RopeTile));
        }
    }

    private void HaulUpRope()
    {   
        playerAudio.PlayRopeAudio();
        CmdSetTileState(RopeTile.TileInfo, TileState.Respawning, RopeTile.TileInfo.Progress);
        StartCoroutine(RemoveRope());
    }

    public IEnumerator RemoveRope()
    {
        CmdSetRopeState(RopeState.Normal);
        while (RopeState != RopeState.Normal)
            yield return null;
        
        this.animator.SetTrigger("RemoveLadder");
        CmdUseRope(false);
        this.playerMovement.EnableMovement();
        IsHoldingRope = false;
    }

    public IEnumerator ThrowRope(GameObject rope, NetworkTile tile)
    {
        tile.ResetHighlighting();

        var ropeScript = rope.GetComponent<NetworkRope>();
        var tileSurfacePosition = new Vector3(tile.transform.position.x, this.transform.position.y, tile.transform.position.z);
        this.transform.LookAt(tileSurfacePosition);

        while (Vector3.Distance(this.transform.position, tileSurfacePosition) > distanceFromTileToHoldRope)
        {
            playerMovement.MoveTowards(rope.transform.forward, speedTowardsRope);
            yield return null;
        } // TODO maybe no playerMovement, 
        playerMovement.MoveTowards(Vector3.zero, 0);

        rope.transform.position = new Vector3(tile.transform.position.x, rope.transform.position.y, tile.transform.position.z);

        rope.transform.LookAt(rope.transform.position - Camera.main.transform.forward);

        this.animator.SetTrigger("PutLadder");
        playerAudio.PlayRopeAudio();
        CmdUseRope(true);
    }

    private void UpdateRopeHint()
    {
        if (!this.ropeCanvas.activeSelf)
            return;

        this.ropeCanvas.transform.LookAt(this.mainCameraTransform);

        if (RopeTile != null)
        {
            this.ropeCanvas.transform.position = new Vector3(
                RopeTile.transform.position.x,
                this.ropeCanvas.transform.position.y,
                RopeTile.transform.position.z);
        }
    }
}
