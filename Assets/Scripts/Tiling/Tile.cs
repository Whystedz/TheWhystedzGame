using Mirror.Cloud.Examples.Pong;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TileState
{
    Normal, 
    Unstable, 
    Respawning
}

public class Tile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float timeToBreak = 3f;
    [SerializeField] private float timeToRespawn = 5f;
    [SerializeField] private float timeOfBreakingAnimation = 5f;
    private float progress;

    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material unstableMaterial;
    [SerializeField] private Material highlightedMaterial;

    [SerializeField] private bool isClickToDigEnabled;

    private MeshRenderer meshRenderer;

    internal TileState tileState;

    private bool isHighlighted;

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

            this.transform.position += breakingAnimationSpeed * Time.deltaTime * Vector3.down;
            breakingAnimationProgress -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private void RespawningUpdate()
    {
        progress -= Time.deltaTime;

        if (progress <= 0)
            Respawn();
    }

    private void Respawn()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        this.meshRenderer.material = normalMaterial;

        this.tileState = TileState.Normal;
    }

    public IEnumerator HighlightTileForOneFrame()
    {
        if (this.tileState != TileState.Normal)
            yield break;

        this.isHighlighted = true;
    }

    private void ChangeMaterialAccordingToCurrentState()
    {
        if (this.isHighlighted)
        {
            this.meshRenderer.material = highlightedMaterial;
            this.isHighlighted = false;
            return; 
        }

        switch (this.tileState)
        {
            case TileState.Normal:
                this.meshRenderer.material = normalMaterial;
                break;
            case TileState.Unstable:
                this.meshRenderer.material = unstableMaterial;
                break;
            case TileState.Respawning:
                break;
        }
    }

}
