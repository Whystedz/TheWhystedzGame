using System.Collections;
using UnityEngine;

public enum RopeState
{
    Normal,
    InUse,
    Saved
}
public class Rope : MonoBehaviour
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

    private InputManager inputManager;
    private bool isAnotherPlayerInZone;

    private PlayerMovement otherPlayerMovement;
    private Teammate team;
    internal RopeState ropeState;

    void Awake()
    {
        ropeState = RopeState.Normal;
        this.highlightedLadder.SetActive(false);
        this.team = this.transform.parent.GetComponent<Teammate>();
    }

    void Start() => inputManager = InputManager.GetInstance();

    void Update()
    {
        if ((this.inputManager.GetLadder() || this.inputManager.GetDigging()) // TODO allow both until we decide
            && ropeState != RopeState.InUse 
            && this.isAnotherPlayerInZone)
            StartCoroutine(ClimbRope());
    }

    private bool GetAnyButtonInput()
    {
        return this.inputManager.GetLadder()
            || this.inputManager.GetDigging()
            || this.inputManager.GetInitiateCombo();
    }

    public void EnterLadderZone(Collider other)
    {
        if (other.CompareTag("Player") && this.team.Team == other.transform.GetComponent<Teammate>().Team)
        {
            this.highlightedLadder.SetActive(true);
            this.isAnotherPlayerInZone = true;
            this.otherPlayerMovement = other.GetComponent<PlayerMovement>();
        }

    }

    public void ExitLadderZone(Collider other)
    {
        if (other.CompareTag("Player") && this.team.Team == other.transform.GetComponent<Teammate>().Team)
        {
            this.highlightedLadder.SetActive(false);
            this.isAnotherPlayerInZone = false;
            other.GetComponent<PlayerMovement>().CanClimb = false;
        }
    }

    public void CleanUpAfterSave() // TODO this is now unused??
    {
        ropeState = RopeState.Normal;
        this.gameObject.SetActive(false);

        this.otherPlayerMovement.EnableMovement();
    }

    public IEnumerator ClimbRope()
    {
        ropeState = RopeState.InUse;

        this.otherPlayerMovement.DisableMovement();

        this.otherPlayerMovement.GetComponent<PlayerAudio>().PlayClimbAudio();
        this.GetComponentInParent<PlayerAudio>().PlayClimbAudio();

        var directionToRope = (this.transform.position - this.otherPlayerMovement.transform.position).normalized;
        directionToRope = new Vector3(directionToRope.x, 0,directionToRope.z);
        var ropePositionWithoutY = new Vector3(this.transform.position.x, this.otherPlayerMovement.transform.position.y, this.transform.position.z);
        this.otherPlayerMovement.transform.LookAt(ropePositionWithoutY);

        while (Vector3.Distance(this.otherPlayerMovement.transform.position, ropePositionWithoutY) > 1.0f)
        {
            this.otherPlayerMovement.MoveTowards(directionToRope, 120f);
            yield return null;
        }
        this.otherPlayerMovement.transform.LookAt(ropePositionWithoutY);
        yield return StartCoroutine(this.otherPlayerMovement.ClimbRope(heightToClimb, this.transform.position));
        ropeState = RopeState.Saved;
    }

    public GameObject GetUpperLadder() => this.upperLadder;
    public GameObject GetLowerLadder() => this.lowerLadder;
    public GameObject GetHighlightedLadder() => this.highlightedLadder;

}
