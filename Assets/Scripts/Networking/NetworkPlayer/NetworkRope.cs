using UnityEngine;

public class NetworkRope : MonoBehaviour
{
    [Header("Rope Parameters")]
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

    private int nPlayersInZone;
    private NetworkPlayerMovement otherPlayerMovement;
    private Teammate team;

    [SerializeField] private NetworkDigging networkDigging;
    [SerializeField] private NetworkRopeInteraction networkRopeInteraction;
    
    void Awake()
    {
        this.highlightedLadder.SetActive(false);
        this.team = this.transform.parent.GetComponent<Teammate>();
        this.nPlayersInZone = 0;
    }

    void Update()
    {
        if (GetAnyButtonInput()
            && networkRopeInteraction.RopeState != RopeState.InUse
            && this.nPlayersInZone > 0)
        {
            this.otherPlayerMovement.StartClimbing(this.gameObject, heightToClimb);
            SetRopeState(RopeState.InUse);
        }
    }

    private bool GetAnyButtonInput()
    {
        return NetworkInputManager.Instance.GetLadder()
            || NetworkInputManager.Instance.GetDigging()
            || NetworkInputManager.Instance.GetInitiateCombo();
    }

    public void EnterLadderZone(Collider other)
    {
        if (IsATeammate(other))
        {
            this.highlightedLadder.SetActive(true);
            this.nPlayersInZone += 1;
            this.otherPlayerMovement = other.GetComponent<NetworkPlayerMovement>();
        }
    }

    public void ExitLadderZone(Collider other)
    {
        if (IsATeammate(other))
        {
            this.highlightedLadder.SetActive(false);

            this.nPlayersInZone -= 1;
            if (this.nPlayersInZone < 0)
            {
                Debug.LogError("Number of players in zone are negative, apparently!", gameObject);
                this.nPlayersInZone = 0;
            }

            this.otherPlayerMovement.CanClimb = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")
            && this.team.Team == other.transform.GetComponent<Teammate>().Team
            && networkRopeInteraction.RopeState != RopeState.InUse)
            this.otherPlayerMovement.CanClimb = true;
        else
            this.otherPlayerMovement.CanClimb = false;            
    }

    private bool IsATeammate(Collider other)
    {
        return other.CompareTag("Player") && this.team.Team == other.transform.GetComponent<Teammate>().Team;
    }

    public void SetRopeState(RopeState newState)
    {
        this.networkRopeInteraction.CmdSetRopeState(newState);
    }

    public GameObject GetUpperLadder() => this.upperLadder;
    public GameObject GetLowerLadder() => this.lowerLadder;
    public GameObject GetHighlightedLadder() => this.highlightedLadder;
}
