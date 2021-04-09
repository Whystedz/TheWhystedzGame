using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class NetworkDigging : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private PlayerAudio playerAudio;

    [Header("Digging")]
    [SerializeField] private float minDistanceToDiggableTile = 1.0f;
    [SerializeField] private float maxDistanceToDiggableTile = 1.0f;
    [SerializeField] private float afterDiggingPause = 0.5f;

    private int tileLayerMask;
    private NetworkPlayerMovement playerMovement;
    private NetworkRopeInteraction ropeInteractions;

    public NetworkTile TileToDig { get; private set; }
    private NetworkTile lastTileHighlighted;

    private TileManager tileManager;

    private void Awake()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();
        this.playerAudio = this.GetComponent<PlayerAudio>();
        this.ropeInteractions = this.GetComponent<NetworkRopeInteraction>();
        this.tileManager = TileManager.GetInstance();
    }

    void Update()
    {
        if (base.hasAuthority)
        {
            ResetDiggingPreviewHighlighting();
            

            if (this.playerMovement.IsInUnderground
                || this.playerMovement.IsFalling()
                || this.ropeInteractions.IsHoldingRope)
                return;
            
            DetermineTileToDig();

            if (TileToDig is null
                || TileToDig.TileInfo.TileState != TileState.Normal)
                return;

            UpdateHighlighting();

            if (InputManager.Instance.GetDigging())
            {
                this.animator.SetTrigger("Shoot");
                CmdDigTile(TileToDig.TileInfo);
            }

            /*
            if (this.rope != null && RopeState == RopeState.Saved)
            {
                CmdSetTileState(Tile.TileInfo, TileState.Respawning, 0f);
                StartCoroutine(RemoveRope());
            }
            */
        }
    }

    private void ResetDiggingPreviewHighlighting()
    {
        if (this.lastTileHighlighted != null)
            this.lastTileHighlighted.ResetHighlighting();

        this.lastTileHighlighted = null;
    }

    private void UpdateHighlighting()
    {
        TileToDig.HighlightTileSimpleDigPreview();

        this.lastTileHighlighted = TileToDig;
    }

    private void DetermineTileToDig()
    {
        var hits = Physics.RaycastAll(transform.position,
            transform.TransformDirection(Vector3.forward),
            this.maxDistanceToDiggableTile,
            tileLayerMask
            );

        if (hits.Length == 0)
        {
            TileToDig = null;
            return;
        }

        var closestCollider = GetClosestCollider(hits);

        if (closestCollider is null)
        {
            if (this.lastTileHighlighted != null)
                this.lastTileHighlighted.ResetHighlighting();
            return;
        }

        var hitGameObject = closestCollider.transform.parent.gameObject;

        TileToDig = hitGameObject.GetComponent<NetworkTile>();
    }

    private Collider GetClosestCollider(RaycastHit[] hits)
    {
        var closestHit = hits
            .Where(hit => Vector3.Distance(transform.position, hit.transform.position) > this.minDistanceToDiggableTile)
            .OrderBy(hit => Vector3.Distance(transform.position, hit.transform.position))
            .FirstOrDefault();

        return closestHit.collider;
    }

    [Command]
    public void CmdDigTile(TileInfo targetTile)
    {
        if (targetTile.TileState == TileState.Normal)
            this.tileManager.DigTile(targetTile);
    }
}
