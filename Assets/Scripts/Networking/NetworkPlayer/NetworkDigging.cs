using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class NetworkDigging : NetworkBehaviour
{
    [SerializeField] private float minDistanceToDiggableTile = 1.0f;
    [SerializeField] private float maxDistanceToDiggableTile = 1.0f;
    private InputManager inputManager;
    private int tileLayerMask;
    private NetworkPlayerMovement playerMovement;
    [SerializeField] private Rope rope;
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private float maxDistanceToRope = 1.5f;
    [SerializeField] private float speedTowardsRope = 6.0f;

    private TileManager tileManager;

    private void Awake()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();
        this.inputManager = InputManager.GetInstance();
        this.tileManager = TileManager.GetInstance();
    }

    void Update()
    {
        if (base.hasAuthority)
        {
            if (this.playerMovement.isInUnderground)
            return;

            var hits = Physics.RaycastAll(transform.position,
                transform.TransformDirection(Vector3.forward),
                this.maxDistanceToDiggableTile,
                this.tileLayerMask
                );

            if (hits.Length == 0)
                return;

            var closestCollider = GetClosestCollider(hits);

            if (closestCollider == null)
                return;

            var hitGameObject = closestCollider.transform.parent.gameObject;

            var tile = hitGameObject.GetComponent<NetworkTile>();

            if (playerMovement.IsFalling())
            {
                rope.gameObject.SetActive(false);
                tile.TileInfo.TileState = TileState.Respawning;
                return;
            }

            switch (tile.TileInfo.TileState)
            {
                case TileState.Normal:
                    StartCoroutine(tile.HighlightTileSimpleDigPreview());
                    break;
                case TileState.Unstable:
                    break;
                case TileState.Respawning:
                    StartCoroutine(tile.HighlightTileRopePreview());
                    break;
                case TileState.Rope:
                    break;
            }

            if (inputManager.GetDigging())
                InteractWithTile(tile);

            if (rope.ropeState == RopeState.Saved)
            {
                tile.TileInfo.TileState = TileState.Respawning;
                tile.Respawn();
                rope.CleanUpAfterSave();
            }
        }
    }

    [Command]
    public void CmdDigTile(TileInfo targetTile)
    {
        this.tileManager.DigTile(targetTile);
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
            CmdDigTile(tile.TileInfo);
        else if (rope.ropeState == RopeState.Normal && 
            ((this.rope.gameObject.activeSelf && tile.TileInfo.TileState == TileState.Rope) 
            || (!this.rope.gameObject.activeSelf && tile.TileInfo.TileState == TileState.Respawning)))
        {
            playerMovement.IsMovementDisabled = !playerMovement.IsMovementDisabled;

            if (tile.TileInfo.TileState == TileState.Rope)
                tile.TileInfo.TileState = TileState.Respawning;
            else
                tile.TileInfo.TileState = TileState.Rope;

            if (tile.TileInfo.TileState == TileState.Rope)
                StartCoroutine(ThrowRope(this.rope.gameObject, tile));
            else
            {
                rope.ropeState = RopeState.Normal;
                tile.TileInfo.TileState = TileState.Respawning;
                this.rope.gameObject.SetActive(false);
            }
        }
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
        ropeObject.SetActive(true);
        yield return null;
    }
}
