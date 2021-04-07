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
            this.playerMovement.StartClimbing(this.gameObject, heightToClimb);
            SetRopeState(RopeState.InUse);
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
}
