using System.Collections;
using System.Linq;
using UnityEngine;

public class NetworkTile : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float obstacleCheckRadius = 1f;
    public TileInfo TileInfo;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material unstableMaterial;
    [SerializeField] private Material destroyedMaterial;
    [SerializeField] private Material highlightedMaterial;
    [SerializeField] private Material comboHighlightedMaterial;
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private Material unbreakableMaterial;

    [SerializeField] private GameObject breakParticleEffectPrefab;

    internal MeshRenderer meshRenderer;
    internal TileHighlightState tileHighlightState;

    TileManager tileManager = TileManager.GetInstance();
    private static GameObject surface;
    private static GameObject underground;

    private bool isDisplayingRopePreview;
    [SerializeField] private GameObject ropePrefab;
    private GameObject rope;

    protected virtual void Start()
    {
        this.meshRenderer = GetComponentInChildren<MeshRenderer>();
        this.meshRenderer.material = this.normalMaterial;

        if (surface is null)
            surface = GameObject.FindGameObjectWithTag("Surface");
        if (underground is null)
            underground = GameObject.FindGameObjectWithTag("Underground");

        CheckObstacles();
    }

    public void DigTile() => StartCoroutine(WaitUntilBroken());
    public void StartRespawning() => StartCoroutine(WaitUntilRespawn());
    public void PauseState() => StopAllCoroutines();

    private IEnumerator WaitUntilBroken()
    {
        while (TileInfo.Progress > 0)
        {
            TileInfo.Progress -= Time.deltaTime;
            yield return null;
        }

        Break();
    }

    private void Break()
    {
        this.tileManager.SetTileState(TileInfo, TileState.Respawning, this.tileManager.timeToRespawn);
        Instantiate(this.breakParticleEffectPrefab, transform.position, Quaternion.identity);
        this.meshRenderer.material = destroyedMaterial;
    }

    private IEnumerator WaitUntilRespawn()
    {
        while (TileInfo.Progress > 0)
        {
            TileInfo.Progress -= Time.deltaTime;
            yield return null;
        }

        Respawn();
    }

    public void Respawn()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        this.meshRenderer.material = normalMaterial;
        this.tileManager.ResetTile(TileInfo);
    }

    public void ResetHighlighting()
    {
        this.tileHighlightState = TileHighlightState.NoHighlight;

        ChangeMaterialAccordingToCurrentState();
    }

    public void ResetComboHighlighting()
    {
        if (this.tileHighlightState == TileHighlightState.ComboHighlight)
            ResetHighlighting();
    }

    public void HighlightTileSimpleDigPreview()
    {
        if (TileInfo.TileState != TileState.Normal)
            return;

        this.tileHighlightState = TileHighlightState.SimpleHighlight;

        ChangeMaterialAccordingToCurrentState();
    }

    public void HighlightTileComboDigPreview()
    {
        if (TileInfo.TileState != TileState.Normal)
            return;

        if (this.tileHighlightState == TileHighlightState.SimpleHighlight)
            return;

        this.tileHighlightState = TileHighlightState.ComboHighlight;

        ChangeMaterialAccordingToCurrentState();
    }

    public void HighlightTileRopePreview()
    {
        if (TileInfo.TileState != TileState.Respawning)
            return;

        this.tileHighlightState = TileHighlightState.RopeHighlight;

        ChangeMaterialAccordingToCurrentState();
    }

    public void ChangeMaterialAccordingToCurrentState()
    {    
        if (this.isDisplayingRopePreview && this.tileHighlightState != TileHighlightState.RopeHighlight)
        {
            ropePrefab.SetActive(false);
            this.isDisplayingRopePreview = false;
        }

        switch (this.tileHighlightState)
        {
            case TileHighlightState.NoHighlight:
                ChangeMaterialAccordingToCurrentStateNoHighlight();
                break;
            case TileHighlightState.SimpleHighlight:
                ChangeMaterialForSimpleHighlight();
                break;
            case TileHighlightState.ComboHighlight:
                ChangeMaterialForComboHighlight();
                break;
            case TileHighlightState.RopeHighlight:
                ChangeMaterialForRopePreviewHighlight();
                break;
        }
    }

    private void ChangeMaterialForRopePreviewHighlight()
    {
        if (this.isDisplayingRopePreview)
            return;

        ropePrefab.SetActive(true);
        this.isDisplayingRopePreview = true;
    }

    private void ChangeMaterialForComboHighlight() => this.meshRenderer.material = comboHighlightedMaterial;

    private void ChangeMaterialForSimpleHighlight() => this.meshRenderer.material = highlightedMaterial;

    private void ChangeMaterialAccordingToCurrentStateNoHighlight()
    {
        switch (TileInfo.TileState)
        {
            case TileState.Normal:
                this.meshRenderer.material = normalMaterial;
                break;
            case TileState.Unstable:
                this.meshRenderer.material = unstableMaterial;
                break;
            case TileState.Respawning:
                this.meshRenderer.material = destroyedMaterial;
                break;
            case TileState.Rope:
                this.meshRenderer.material = destroyedMaterial;
                break;
            case TileState.Unbreakable:
                this.meshRenderer.material = this.unbreakableMaterial;
                break;
        }
    }

    public static NetworkTile FindTileAtPosition(Vector3 position)
    {
        var distanceToSurface = Mathf.Abs(position.y - surface.transform.position.y);
        var distanceToUnderground = Mathf.Abs(position.y - underground.transform.position.y);

        if (distanceToUnderground < distanceToSurface)
            return null;

        var hasHitTile = Physics.Raycast(position + Vector3.down * 5f,
            Vector3.up,
            out RaycastHit hitTile,
            10f,
            1 << LayerMask.NameToLayer("Tile"));

        if (!hasHitTile)
            return GetClosestTileWithSphereCheck(position);

        return hitTile.collider.transform.parent.GetComponent<NetworkTile>();
    }

    private static NetworkTile GetClosestTileWithSphereCheck(Vector3 position)
    {
        var colliders = Physics.OverlapSphere(position, 1f, 1 << LayerMask.NameToLayer("Tile"));

        var closestCollider = colliders
            .OrderBy(collider => Vector3.Distance(position, collider.transform.position))
            .FirstOrDefault();

        if (closestCollider is null)
            return null;

        return closestCollider.GetComponentInParent<NetworkTile>();
    }

    private void CheckObstacles()
    {
        var colliders = Physics.OverlapSphere(transform.position, this.obstacleCheckRadius);
        if (colliders.Count() == 0) return;

        var obstacles = colliders
            .Where(collider => collider.gameObject.CompareTag("Obstacle"));

        if (obstacles.Count() == 0)
            return;

        SetUnbreakable();
    }

    protected void SetUnbreakable()
    {
        if (TileInfo.TileState == TileState.Unbreakable)
            return;

        TileInfo.TileState = TileState.Unbreakable;
        this.meshRenderer.material = this.unbreakableMaterial;
    }

    internal bool IsDiggable() => TileInfo.TileState == TileState.Normal;
}
