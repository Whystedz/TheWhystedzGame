using System.Linq;
using UnityEngine;

public class SimpleDigging : MonoBehaviour
{
    [Header("Debug Local")]
    [SerializeField] private bool isClientPlayer;

    [Header("Digging")]
    [SerializeField] private float minDistanceToDiggableTile = 3.0f;
    [SerializeField] private float maxDistanceToDiggableTile = 6.0f;
    [SerializeField] private float afterDiggingPause = 0.5f;
    
    private InputManager inputManager;
    private int tileLayerMask;
    private PlayerMovement playerMovement;
    private PlayerAudio playerAudio;
    private RopeInteractions ropeInteractions;

    private Tile lastTileHighlighted;

    public Tile TileToDig { get; private set; }

    private void Awake()
    {
        this.tileLayerMask = LayerMask.GetMask("Tile");
        this.playerMovement = this.GetComponent<PlayerMovement>();
        this.playerAudio = this.GetComponent<PlayerAudio>();
        this.ropeInteractions = this.GetComponent<RopeInteractions>();
    }

    void Start()
    {
        inputManager = InputManager.GetInstance();
    }

    void Update()
    {
        if (!this.isClientPlayer) return;

        ResetDiggingPreviewHighlighting();

        if (this.playerMovement.IsInUnderground
            || this.playerMovement.IsFalling()
            || this.ropeInteractions.IsHoldingRope)
            return;

        DetermineTileToDig();

        if (TileToDig is null
            || TileToDig.tileState != TileState.Normal)
            return;

        UpdateHighlighting();

        if (inputManager.GetDigging())
            Dig();
    }

    private void ResetDiggingPreviewHighlighting()
    {
        if (this.lastTileHighlighted != null)
            this.lastTileHighlighted.ResetHighlighting();

        this.lastTileHighlighted = null;
    }

    private void UpdateHighlighting()
    {
        TileToDig.HighlightTileSimpleDigPreview();

        this.lastTileHighlighted = TileToDig;
    }

    private void DetermineTileToDig()
    {
        var hits = Physics.RaycastAll(transform.position,
            transform.TransformDirection(Vector3.forward),
            this.maxDistanceToDiggableTile,
            tileLayerMask
            );

        if (hits.Length == 0)
        {
            TileToDig = null;
            return;
        }

        var closestCollider = GetClosestCollider(hits);

        if (closestCollider is null)
        {
            if (this.lastTileHighlighted != null)
                this.lastTileHighlighted.ResetHighlighting();
            return;
        }

        var hitGameObject = closestCollider.transform.parent.gameObject;

        TileToDig = hitGameObject.GetComponent<Tile>();
    }

    private Collider GetClosestCollider(RaycastHit[] hits)
    {
        var closestHit = hits
            .Where(hit => Vector3.Distance(transform.position, hit.transform.position) > this.minDistanceToDiggableTile)
            .OrderBy(hit => Vector3.Distance(transform.position, hit.transform.position))
            .FirstOrDefault();

        return closestHit.collider;
    }

    public void Dig() 
    {
        this.playerAudio.PlayLaserAudio();

        playerMovement.DisableMovementFor(this.afterDiggingPause);

        TileToDig.DigTile();
    }
}
