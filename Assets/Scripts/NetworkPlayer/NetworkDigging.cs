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
    private int layerMask;
    private PlayerMovement playerMovement;

    [SerializeField] private bool enableDebugMode = true;
    
    private TileManager tileManager;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("TileMovementCollider");
        playerMovement = this.GetComponent<PlayerMovement>();
        inputManager = InputManager.GetInstance();
        tileManager = TileManager.GetInstance();
    }

    void Update()
    {
        if (base.hasAuthority)
        {
            if (playerMovement.IsFalling())
                return;

            var hits = Physics.RaycastAll(transform.position,
                transform.TransformDirection(Vector3.forward),
                this.maxDistanceToDiggableTile,
                layerMask
                );

            if (hits.Length == 0)
                return;

            var closestCollider = GetClosestCollider(hits);

            if (closestCollider == null)
                return;

            var hitGameObject = closestCollider.transform.parent.gameObject;

            var tile = hitGameObject.GetComponent<NetworkTile>();
            if (tile.HexTile.TileState != TileState.Normal)
                return;

            StartCoroutine(tile.HighlightTile());

            if (inputManager.GetDigging())
                tileManager.DigTile(tile);
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
}
