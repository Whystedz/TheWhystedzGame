using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class NetworkTile : MonoBehaviour
{
    public TileInfo TileInfo;

    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material unstableMaterial;
    [SerializeField] private Material destroyedMaterial;
    [SerializeField] private Material highlightedMaterial;
    [SerializeField] private Material comboHighlightedMaterial;
    [SerializeField] private Material ropeMaterial;

    private MeshRenderer meshRenderer;
    internal TileHighlightState tileHighlightState;

    TileManager tileManager = TileManager.GetInstance();

    private void Start()
    {
        this.meshRenderer = GetComponentInChildren<MeshRenderer>();
        this.meshRenderer.material = this.normalMaterial;
    }

    void Update()
    {
        ChangeMaterialAccordingToCurrentState();

        switch (TileInfo.TileState)
        {
            case TileState.Normal:
                break;
            case TileState.Unstable:
                UnstableUpdate();
                break;
            case TileState.Respawning:
                RespawningUpdate();
                break;
            case TileState.Rope:
                break;
        }
    }

    private void UnstableUpdate()
    {
        TileInfo.Progress -= Time.deltaTime;

        if (TileInfo.Progress <= 0)
            Break();
    }

    private void Break()
    {
        StartCoroutine(PlayBreakingAnimation());

        this.meshRenderer.material = destroyedMaterial;
        this.tileManager.SetTileState(TileInfo, TileState.Respawning, TileInfo.TimeToRespawn);
    }

    private IEnumerator PlayBreakingAnimation()
    {
        var breakingAnimationProgress = TileInfo.TimeOfBreakingAnimation;
        var breakingAnimationSpeed = 10f / TileInfo.TimeOfBreakingAnimation;

        while (breakingAnimationProgress > 0)
        {
            if (TileInfo.TileState == TileState.Normal)
                yield break;

            breakingAnimationProgress -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private void RespawningUpdate()
    {
        TileInfo.Progress -= Time.deltaTime;

        if (TileInfo.Progress <= 0)
            Respawn();
    }

    public void Respawn()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        this.meshRenderer.material = normalMaterial;
        this.tileManager.ResetTile(TileInfo);
    }

    public void HighlightTileSimpleDigPreview()
    {
        if (TileInfo.TileState != TileState.Normal)
            return;

        this.tileHighlightState = TileHighlightState.SimpleHighlight;
    }

    public IEnumerator HighlightTileComboDigPreview()
    {
        if (TileInfo.TileState != TileState.Normal)
            yield break;

        this.tileHighlightState = TileHighlightState.ComboHighlight;
    }

    public void HighlightTileRopePreview()
    {
        if (TileInfo.TileState != TileState.Respawning)
            return;

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
        }
    }
}
