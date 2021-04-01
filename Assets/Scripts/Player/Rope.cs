using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RopeState
{
    Normal,
    InUse,
    Saved
}
public class Rope : MonoBehaviour
{
    [SerializeField] private GameObject highlightedLadder;
    private InputManager inputManager;
    private bool inZone;
    //public bool InUse;
    //public bool Saved;
    private PlayerMovement playerMovement;
    [SerializeField] private float heightToClimb = 8f;
    private Teammate team;
    internal RopeState ropeState;
    
    // Start is called before the first frame update
    void Awake()
    {
        ropeState = RopeState.Normal;
        this.highlightedLadder.SetActive(false);
        this.team = this.transform.parent.GetComponent<Teammate>();
    }

    void Start() => inputManager = InputManager.GetInstance();

    // Update is called once per frame
    void Update()
    {
        if (this.inputManager.GetDigging() && ropeState != RopeState.InUse && this.inZone)
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
            this.playerMovement = other.GetComponent<PlayerMovement>();
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

    public void CleanUpAfterSave()
    {
        ropeState = RopeState.Normal;
        this.gameObject.SetActive(false);
        this.playerMovement.isMovementDisabled = false;
    }

    public IEnumerator ClimbRope()
    {
        ropeState = RopeState.InUse;
        this.playerMovement.isMovementDisabled = true;
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
        ropeState = RopeState.Saved;
    }
}
