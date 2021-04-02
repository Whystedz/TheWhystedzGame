using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TileState
{
    Normal,
    Unstable, 
    Respawning,
    Rope,
}

public enum TileHighlightState
{
    NoHighlight,
    SimpleHighlight,
    ComboHighlight,
    RopeHighlight
}

public class Tile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float timeToBreak = 3f;
    [SerializeField] private float timeToRespawn = 5f;
    [SerializeField] private float timeOfBreakingAnimation = 5f;
    private float progress;

    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material unstableMaterial;
    [SerializeField] private Material destroyedMaterial;
    [SerializeField] private Material highlightedMaterial;
    [SerializeField] private Material ropeMaterial;

    [SerializeField] private bool isClickToDigEnabled;

    private MeshRenderer meshRenderer;

    internal TileState tileState;
    internal TileHighlightState tileHighlightState;

    private void Start()
    {
        this.meshRenderer = GetComponentInChildren<MeshRenderer>();
        this.meshRenderer.material = this.normalMaterial;
    }

    void Update()
    {
        ChangeMaterialAccordingToCurrentState();

        switch (this.tileState)
        {
            case TileState.Normal:
                break;
            case TileState.Unstable:
                this.UnstableUpdate();
                break;
            case TileState.Respawning:
                RespawningUpdate();
                break;
            case TileState.Rope:
                break;
        }
    }

    // Should be called by the agent wishing to dig the tile
    public void DigTile()
    {
        this.progress = this.timeToBreak;
        this.meshRenderer.material = unstableMaterial;
        this.tileState = TileState.Unstable;
    }


    // For debug purposes, dig a tile up upon clicking on it
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!this.isClickToDigEnabled)
            return;

        this.DigTile();
    }

    private void UnstableUpdate()
    {
        progress -= Time.deltaTime;

        if (progress <= 0)
            Break();
    }

    private void Break()
    {
        StartCoroutine(PlayBreakingAnimation());

        this.tileState = TileState.Respawning;
        this.meshRenderer.material = destroyedMaterial;
        this.progress = this.timeToRespawn;
    }

    private IEnumerator PlayBreakingAnimation()
    {
        var breakingAnimationProgress = this.timeOfBreakingAnimation;
        var breakingAnimationSpeed = 10f / this.timeOfBreakingAnimation;

        while (breakingAnimationProgress > 0)
        {
            if (this.tileState == TileState.Normal)
                yield break;

            //this.transform.position += breakingAnimationSpeed * Time.deltaTime * Vector3.down;
            // TODO change to a simple opacity change
            // TODO TODO eventually get a nicer animation 
            breakingAnimationProgress -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private void RespawningUpdate()
    {
        this.progress -= Time.deltaTime;

        if (progress <= 0)
            Respawn();
    }
     
    public void Respawn()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        this.meshRenderer.material = normalMaterial;
        this.tileState = TileState.Normal;
    }

    public IEnumerator HighlightTileSimpleDigPreview()
    {
        if (this.tileState != TileState.Normal)
            yield break;

        this.tileHighlightState = TileHighlightState.SimpleHighlight;
    }

    public IEnumerator HighlightTileComboDigPreview()
    {
        if (this.tileState != TileState.Normal)
            yield break;

        // maybe some logic if it is already a simple combo highlight? 

        this.tileHighlightState = TileHighlightState.ComboHighlight;
    }

    public IEnumerator HighlightTileRopePreview()
    {
        if (this.tileState != TileState.Respawning)
            yield break;

        this.tileHighlightState = TileHighlightState.RopeHighlight;
    }

    private void ChangeMaterialAccordingToCurrentState()
    {       
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

        this.tileHighlightState = TileHighlightState.NoHighlight;
    }

    private void ChangeMaterialForRopePreviewHighlight() => this.meshRenderer.material = ropeMaterial;

    private void ChangeMaterialForComboHighlight() => this.meshRenderer.material = highlightedMaterial;

    private void ChangeMaterialForSimpleHighlight() => this.meshRenderer.material = highlightedMaterial;

    private void ChangeMaterialAccordingToCurrentStateNoHighlight()
    {
        switch (this.tileState)
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
        }
    }
}
