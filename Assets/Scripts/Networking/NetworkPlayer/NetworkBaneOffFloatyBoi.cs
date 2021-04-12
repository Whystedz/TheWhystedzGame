using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkBaneOffFloatyBoi : NetworkBehaviour
{
    private CharacterController characterController;
    private NetworkPlayerMovement playerMovement;

    private float initialHeight;
    private float maxHeight;
    private float minHeight;
    private readonly float maxHeightThreshold = 0.001f;
    private readonly float minHeightThreshold = 0.05f;

    private Stack<Vector3> recentPositions;

    private float timeSinceLastRefresh;
    private readonly float timeToRefresh = 0.1f;

    public override void OnStartLocalPlayer()
    {
        this.characterController = GetComponent<CharacterController>();
        this.playerMovement = GetComponent<NetworkPlayerMovement>();

        this.initialHeight = transform.position.y;
        this.maxHeight = this.initialHeight + this.maxHeightThreshold;
        this.minHeight = this.initialHeight - this.minHeightThreshold;

        this.recentPositions = new Stack<Vector3>();
        this.recentPositions.Push(this.transform.position);
    }

    void Update()
    {
        if (base.hasAuthority)
        {
            timeSinceLastRefresh += Time.deltaTime;

            if (!this.playerMovement.IsInUnderground && !this.playerMovement.IsFalling())
                if (this.transform.position.y > this.maxHeight || this.transform.position.y < this.minHeight)
                    RevertPosition();

            if (this.timeSinceLastRefresh > this.timeToRefresh
                && Vector3.Distance(this.transform.position, this.recentPositions.Peek()) > 0.1f)
                this.recentPositions.Push(this.transform.position);

            if (this.recentPositions.Count > 50)
                ShortenStack();
        }
    }

    private void ShortenStack()
    {
        var moreRecentPositions = new Stack<Vector3>();

        for (int i = 0; i < 20; ++i)
        {
            if (this.recentPositions.Count == 0)
                break;

            moreRecentPositions.Push(this.recentPositions.Pop());
        }

        this.recentPositions.Clear();

        for (int i = 0; i < 20; ++i)
        {
            if (moreRecentPositions.Count == 0)
                break;

            this.recentPositions.Push(moreRecentPositions.Pop());
        }
    }

    private void RevertPosition()
    {
        //Debug.Log($"Reverting position! At {this.transform.position}", this.gameObject);

        var revertPosition = this.recentPositions.Pop();        

        this.characterController.enabled = false;
        this.transform.position = new Vector3 (revertPosition.x, 
            this.initialHeight, 
            revertPosition.z);
        this.characterController.enabled = true;
    }
}
