using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TileState
{
    Normal,
    Unstable, 
    Respawning,
    Rope,
    Unbreakable,
}

public enum TileHighlightState
{
    NoHighlight,
    SimpleHighlight,
    ComboHighlight,
    RopeHighlight
}

public class Tile : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float obstacleCheckRadius = 1f;

    [Header("Animations")]
    [SerializeField] private float timeToBreak = 3f;
    [SerializeField] private float timeToRespawn = 5f;
    private float progress;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material unstableMaterial;
    [SerializeField] private Material destroyedMaterial;
    [SerializeField] private Material highlightedMaterial;
    [SerializeField] private Material comboHighlightedMaterial;
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private Material unbreakableMaterial;

    [SerializeField] private GameObject breakParticleEffectPrefab;
    private MeshRenderer meshRenderer;

    internal TileState tileState;
    internal TileHighlightState tileHighlightState;

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

    public void DigTile() {
        if (this.tileState != TileState.Normal) return;
        StartCoroutine(WaitUntilBroken());
    }

    private IEnumerator WaitUntilBroken()
    {
        //this.meshRenderer.material = unstableMaterial;
        this.tileState = TileState.Unstable;

        this.progress = this.timeToBreak; 
        while (this.progress > 0)
        {
            this.progress -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        Break();
    }

    private void Break()
    {
        Instantiate(this.breakParticleEffectPrefab, transform.position, Quaternion.identity);
        this.tileState = TileState.Respawning;
        this.meshRenderer.material = destroyedMaterial;
        StartCoroutine(WaitUntilRespawn());
    }

    private IEnumerator WaitUntilRespawn()
    {
        this.progress = this.timeToRespawn;

        while (this.progress > 0)
        {
            if (this.tileState == TileState.Respawning)
                this.progress -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        Respawn();
    }
     
    public void Respawn()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        this.meshRenderer.material = normalMaterial;
        this.tileState = TileState.Normal;
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
        if (this.tileState != TileState.Normal)
            return;

        this.tileHighlightState = TileHighlightState.SimpleHighlight;

        ChangeMaterialAccordingToCurrentState();
    }

    public void HighlightTileComboDigPreview()
    {
        if (this.tileState != TileState.Normal)
            return;

        if (this.tileHighlightState == TileHighlightState.SimpleHighlight)
            return;

        this.tileHighlightState = TileHighlightState.ComboHighlight;

        ChangeMaterialAccordingToCurrentState();
    }

    public void HighlightTileRopePreview()
    {
        if (this.tileState != TileState.Respawning)
            return;

        this.tileHighlightState = TileHighlightState.RopeHighlight;

        ChangeMaterialAccordingToCurrentState();
    }

    private void ChangeMaterialAccordingToCurrentState()
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
        switch (this.tileState)
        {
            case TileState.Normal:
                this.meshRenderer.material = this.normalMaterial;
                break;
            case TileState.Unstable:
                this.meshRenderer.material = this.unstableMaterial;
                break;
            case TileState.Respawning:
                this.meshRenderer.material = this.destroyedMaterial;
                break;
            case TileState.Rope:
                this.meshRenderer.material = this.destroyedMaterial;
                break;
            case TileState.Unbreakable:
                this.meshRenderer.material = this.unbreakableMaterial;
                break;
        }
    }

    public static Tile FindTileAtPosition(Vector3 position)
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

        return hitTile.collider.transform.parent.GetComponent<Tile>();
    }

    private static Tile GetClosestTileWithSphereCheck(Vector3 position)
    {
        var colliders = Physics.OverlapSphere(position, 1f, 1 << LayerMask.NameToLayer("Tile"));

        var closestCollider = colliders
            .OrderBy(collider => Vector3.Distance(position, collider.transform.position))
            .FirstOrDefault();

        if (closestCollider is null)
            return null;

        return closestCollider.GetComponentInParent<Tile>();
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
        if (this.tileState == TileState.Unbreakable)
            return;

        this.tileState = TileState.Unbreakable;
        this.meshRenderer.material = this.unbreakableMaterial;
    }

    internal bool IsDiggable() => this.tileState == TileState.Normal;
}
