using System.Collections;
using UnityEngine;

public class NetworkRope : MonoBehaviour
{
    [Header("Rope Parameters")]
    [SerializeField] private Rope rope;
    [SerializeField] private float heightToClimb = 8f;
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private float maxDistanceToRope = 1.8f;
    [SerializeField] private float speedTowardsRope = 6.0f;
    [SerializeField] private float minDistanceToRopeTile = 1.0f;
    [SerializeField] private float maxDistanceToRopeTile = 3.0f;
    [SerializeField] private float afterRopePause = 0.5f;

    [Header("Ladder Parts")]
    [SerializeField] private GameObject highlightedLadder;
    [SerializeField] private GameObject upperLadder;
    [SerializeField] private GameObject lowerLadder;

    private bool isAnotherPlayerInZone;
    private NetworkPlayerMovement playerMovement;
    private Teammate team;

    [SerializeField] private NetworkDigging networkDigging;
    [SerializeField] private NetworkRopeInteraction networkRopeInteraction;
    
    void Awake()
    {
        this.highlightedLadder.SetActive(false);
        this.team = this.transform.parent.GetComponent<Teammate>();
    }

    void Update()
    {
        if (NetworkInputManager.Instance.GetLadder() || NetworkInputManager.Instance.GetDigging() // TODO allow both until we decide
            && networkRopeInteraction.RopeState != RopeState.InUse 
            && this.isAnotherPlayerInZone)
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
            this.isAnotherPlayerInZone = true;
            this.playerMovement = other.GetComponent<NetworkPlayerMovement>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && this.team.Team == other.transform.GetComponent<Teammate>().Team)
        {
            this.highlightedLadder.SetActive(false);
            this.isAnotherPlayerInZone = false;
        }
    }

    public void SetRopeState(RopeState newState)
    {
        this.networkRopeInteraction.CmdSetRopeState(newState);
    }

    public GameObject GetUpperLadder() => this.upperLadder;
    public GameObject GetLowerLadder() => this.lowerLadder;
    public GameObject GetHighlightedLadder() => this.highlightedLadder;
}
