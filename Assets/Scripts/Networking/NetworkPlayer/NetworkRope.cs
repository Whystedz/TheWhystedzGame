using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRope : MonoBehaviour
{
    [SerializeField] private GameObject highlightedLadder;
    private InputManager inputManager;
    private bool inZone;
    private NetworkPlayerMovement playerMovement;
    [SerializeField] private float heightToClimb = 8f;
    private Teammate team;

    [SerializeField] private NetworkDigging networkDigging;
    
    void Awake()
    {
        this.highlightedLadder.SetActive(false);
        this.team = this.transform.parent.GetComponent<Teammate>();
        inputManager = InputManager.GetInstance();
    }

    void Update()
    {
        if (this.inputManager.GetDigging() && networkDigging.RopeState != RopeState.InUse && this.inZone)
        {
            StartCoroutine(ClimbRope());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && this.team.Team == other.transform.GetComponent<Teammate>().Team)
        {
            this.highlightedLadder.SetActive(true);
            this.inZone = true;
            this.playerMovement = other.GetComponent<NetworkPlayerMovement>();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && this.team.Team == other.transform.GetComponent<Teammate>().Team)
        {
            this.highlightedLadder.SetActive(false);
            this.inZone = false;
        }
    }

    public void SetRopeState(RopeState newState)
    {
        this.networkDigging.CmdSetRopeState(newState);
    }

    public IEnumerator ClimbRope()
    {
        SetRopeState(RopeState.InUse);
        this.playerMovement.IsMovementDisabled = true;
        var directionToRope = (this.transform.position - this.playerMovement.transform.position).normalized;
        directionToRope = new Vector3(directionToRope.x, 0,directionToRope.z);
        var ropePositionWithoutY = new Vector3(this.transform.position.x, this.playerMovement.transform.position.y, this.transform.position.z);
        this.playerMovement.transform.LookAt(ropePositionWithoutY);

        while (Vector3.Distance(this.playerMovement.transform.position, ropePositionWithoutY) > 1.0f)
        {
            this.playerMovement.MoveTowards(directionToRope, 120f);
            yield return null;
        }
        this.playerMovement.transform.LookAt(ropePositionWithoutY);
        yield return StartCoroutine(this.playerMovement.ClimbRope(heightToClimb, this.transform.position));
        SetRopeState(RopeState.Saved);
    }
}
