using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiggingAndRopeInteractions : MonoBehaviour
{
    [SerializeField] private float minDistanceToDiggableTile = 1.0f;
    [SerializeField] private float maxDistanceToDiggableTile = 1.0f;
    private InputManager inputManager;
    private int tileLayerMask;
    private PlayerMovement playerMovement;
    private PlayerAudio playerAudio;
    [SerializeField] private Rope rope;
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private float maxDistanceToRope = 1.5f;
    [SerializeField] private float speedTowardsRope = 6.0f;
    public Tile tile { get; private set; }
    private void Awake()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.playerMovement = this.GetComponent<PlayerMovement>();
        this.playerAudio = this.GetComponent<PlayerAudio>();
    }
    void Start() => inputManager = InputManager.GetInstance();

    void Update()
    {
        if (this.playerMovement.IsInUnderground)
            return;

        var hits = Physics.RaycastAll(transform.position,
            transform.TransformDirection(Vector3.forward),
            this.maxDistanceToDiggableTile,
            tileLayerMask
            );

        if (hits.Length == 0)
            return;

        var closestCollider = GetClosestCollider(hits);

        if (closestCollider == null)
            return;

        var hitGameObject = closestCollider.transform.parent.gameObject;

        tile = hitGameObject.GetComponent<Tile>();

        if (playerMovement.IsFalling() && this.rope != null)
        {
            this.rope.gameObject.SetActive(false);
            tile.tileState = TileState.Respawning;
            return;
        }

        switch (tile.tileState)
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

        if (this.rope != null && this.rope.ropeState == RopeState.Saved)
        {
            tile.tileState = TileState.Respawning;
            tile.Respawn();
            this.rope.CleanUpAfterSave();
        }
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

    public IEnumerator ThrowRope(GameObject ropeObject, Tile tile)
    {
        playerAudio.PlayRopeAudio();
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

    public void InteractWithTile(Tile tile)
    {

        if (tile.tileState == TileState.Normal)
        {
            this.playerAudio.PlayLaserAudio();
            tile.DigTile();
        }
        else if (rope.ropeState == RopeState.Normal && 
                 ((this.rope.gameObject.activeSelf && tile.tileState == TileState.Rope) 
                  || (!this.rope.gameObject.activeSelf && tile.tileState == TileState.Respawning)))
        {
            playerMovement.IsMovementDisabled = !playerMovement.IsMovementDisabled;

            if (tile.tileState == TileState.Rope)
                tile.tileState = TileState.Respawning;
            else
                tile.tileState = TileState.Rope;

            if (tile.tileState == TileState.Rope)
                StartCoroutine(ThrowRope(this.rope.gameObject, tile));
            else
            {
                playerAudio.PlayRopeAudio();
                rope.ropeState = RopeState.Normal;
                tile.tileState = TileState.Respawning;
                this.rope.gameObject.SetActive(false);
            }
        }
    }
}

    
