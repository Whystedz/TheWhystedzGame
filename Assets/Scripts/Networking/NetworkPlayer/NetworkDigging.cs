using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class NetworkDigging : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private PlayerAudio playerAudio;

    [SerializeField] private float minDistanceToDiggableTile = 1.0f;
    [SerializeField] private float maxDistanceToDiggableTile = 1.0f;

    private int tileLayerMask;
    private NetworkPlayerMovement playerMovement;

    [SyncVar(hook = nameof(OnRopeUsed))]
    public bool IsRopeInUse = false;
    [SerializeField] private NetworkRope rope;
    [SyncVar]
    public RopeState RopeState = RopeState.Normal;

    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private float maxDistanceToRope = 1.5f;
    [SerializeField] private float speedTowardsRope = 6.0f;

    [SerializeField] private bool isClientPlayer;

    public NetworkTile Tile { get; private set; }
    private NetworkTile lastTileHighlighted;
    private NetworkTile tileCurrentlyOn;

    private TileManager tileManager;

    private void Awake()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();
        this.tileManager = TileManager.GetInstance();

        if (base.hasAuthority)
            isClientPlayer = true;
    }

    void Update()
    {
        if (base.hasAuthority)
        {
            this.tileCurrentlyOn = NetworkTile.FindTileAtPosition(transform.position);
            
            if (this.playerMovement.IsInUnderground)
                return;

            var hits = Physics.RaycastAll(transform.position,
                transform.TransformDirection(Vector3.forward),
                this.maxDistanceToDiggableTile,
                this.tileLayerMask
                );

            if (hits.Length == 0)
                return;

            var closestCollider = GetClosestCollider(hits);

            if (closestCollider is null)
            {
                if (this.lastTileHighlighted != null)
                    this.lastTileHighlighted.ResetHighlighting();
                return;
            }

            var hitGameObject = closestCollider.transform.parent.gameObject;

            Tile = hitGameObject.GetComponent<NetworkTile>();

            if (this.playerMovement.IsFalling() && this.rope != null)
            {
                CmdUseRope(false);
                CmdSetTileState(Tile.TileInfo, TileState.Respawning, Tile.TileInfo.Progress);
                return;
            }

            switch (Tile.TileInfo.TileState)
            {
                case TileState.Normal:
                    Tile.HighlightTileSimpleDigPreview();

                    if (this.lastTileHighlighted != null
                        && this.lastTileHighlighted != Tile)
                        this.lastTileHighlighted.ResetHighlighting();

                    this.lastTileHighlighted = Tile;
                    break;
                case TileState.Unstable:
                    break;
                case TileState.Respawning:
                    Tile.HighlightTileRopePreview();
                    break;
                case TileState.Rope:
                    break;
            }

            if (InputManager.Instance.GetDigging())
                InteractWithTile(Tile);

            if (this.rope != null && RopeState == RopeState.Saved)
            {
                CmdSetTileState(Tile.TileInfo, TileState.Respawning, 0f);
                StartCoroutine(RemoveRope());
            }
        }
    }

    [Command]
    public void CmdDigTile(TileInfo targetTile)
    {
        this.tileManager.DigTile(targetTile);
    }

    [Command]
    public void CmdUseRope(bool isUsing)
    {
        IsRopeInUse = isUsing;
    }

    [Command]
    public void CmdSetTileState(TileInfo targetTile, TileState newState, float newProgress)
    {
        this.tileManager.SetTileState(targetTile, newState, newProgress);
    }

    [Command(ignoreAuthority = true)]
    public void CmdSetRopeState(RopeState newState)
    {
        RopeState = newState;
    }

    private void OnRopeUsed(bool oldValue, bool newValue)
    {
        this.rope.gameObject.SetActive(newValue);
        Debug.Log("Used rope");
    }

    private void OnDrawGizmos()
    {
        if (enableDebugMode)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                Vector3.right * 0.1f + transform.position,
                Vector3.right * 0.1f + transform.position + transform.forward * this.minDistanceToDiggableTile
            );

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                transform.position,
                transform.position + transform.forward * this.maxDistanceToDiggableTile
            );
        }
    }

    private Collider GetClosestCollider(RaycastHit[] hits)
    {
        var closestDistnace = Mathf.Infinity;
        Collider closestCollider = null;

        int hitsLength = hits.Length;
        for (int i = 0; i < hitsLength; i++)
        {
            var distance = Vector3.Distance(transform.position, hits[i].collider.transform.position);
            if (distance < this.minDistanceToDiggableTile)
                continue; // ignore tiles that are too close

            if (distance < closestDistnace)
            {
                closestCollider = hits[i].collider;
                closestDistnace = distance;
            }
        }

        return closestCollider;
    }

    public void InteractWithTile(NetworkTile tile)
    {
        
        if (tile.TileInfo.TileState == TileState.Normal)
        {
            this.animator.SetTrigger("Shoot");
            CmdDigTile(tile.TileInfo);
        }
        else if (RopeState == RopeState.Normal && 
            ((IsRopeInUse && tile.TileInfo.TileState == TileState.Rope) 
            || (!IsRopeInUse && tile.TileInfo.TileState == TileState.Respawning)))
        {
            this.playerMovement.IsMovementDisabled = !this.playerMovement.IsMovementDisabled;

            if (tile.TileInfo.TileState == TileState.Rope)
            {
                CmdSetTileState(tile.TileInfo, TileState.Respawning, tile.TileInfo.Progress);
                StartCoroutine(RemoveRope());
            }
            else
            {
                CmdSetTileState(tile.TileInfo, TileState.Rope, tile.TileInfo.Progress);
                StartCoroutine(ThrowRope(this.rope.gameObject, tile));
            }
        }
    }

    public IEnumerator RemoveRope()
    {
        this.rope.SetRopeState(RopeState.Normal);
        while (RopeState != RopeState.Normal)
            yield return null;

        CmdUseRope(false);
        this.playerMovement.IsMovementDisabled = false;
    }

    public IEnumerator ThrowRope(GameObject ropeObject, NetworkTile tile)
    {
        Vector3 tileSurfacePosition = new Vector3(tile.transform.position.x, this.transform.position.y, tile.transform.position.z);
        while (Vector3.Distance(this.transform.position, tileSurfacePosition) > maxDistanceToRope)
        {
            playerMovement.MoveTowards(ropeObject.transform.forward, speedTowardsRope);
            yield return null;
        }
        playerMovement.MoveTowards(Vector3.zero, 0);
        ropeObject.transform.position = new Vector3(tile.transform.position.x, ropeObject.transform.position.y, tile.transform.position.z);
        CmdUseRope(true);
        yield return null;
    }

    public NetworkTile TileCurrentlyOn() => this.tileCurrentlyOn;
}
