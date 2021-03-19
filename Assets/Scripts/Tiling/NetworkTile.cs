using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public struct HexTile
{
    public float TimeToBreak;
    public float TimeToRespawn;
    public float TimeOfBreakingAnimation;
    public float Progress;
    public int XIndex;
    public int ZIndex;
    public TileState TileState;
}

public class NetworkTile : MonoBehaviour
{
    public HexTile HexTile;

    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material unstableMaterial;
    [SerializeField] private Material highlightedMaterial;

    private MeshRenderer meshRenderer;

    private void Start()
    {
        this.meshRenderer = GetComponentInChildren<MeshRenderer>();
        this.meshRenderer.material = this.normalMaterial;
    }

    void Update()
    {
        switch (HexTile.TileState)
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

    private void UnstableUpdate()
    {
        HexTile.Progress -= Time.deltaTime;

        if (HexTile.Progress <= 0)
            Break();
    }

    private void Break()
    {
        StartCoroutine(PlayBreakingAnimation());

        HexTile.TileState = TileState.Respawning;
        HexTile.Progress = HexTile.TimeToRespawn;
    }

    private IEnumerator PlayBreakingAnimation()
    {
        var breakingAnimationProgress = HexTile.TimeOfBreakingAnimation;
        var breakingAnimationSpeed = 10f / HexTile.TimeOfBreakingAnimation;

        while (breakingAnimationProgress > 0)
        {
            if (HexTile.TileState == TileState.Normal)
                yield break;

            this.transform.position += breakingAnimationSpeed * Time.deltaTime * Vector3.down;
            breakingAnimationProgress -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private void RespawningUpdate()
    {
        HexTile.Progress -= Time.deltaTime;

        if (HexTile.Progress <= 0)
            Respawn();
    }

    private void Respawn()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        this.meshRenderer.material = normalMaterial;

        HexTile.TileState = TileState.Normal;
    }

    public IEnumerator HighlightTile()
    {
        if (HexTile.TileState != TileState.Normal)
            yield break;

        this.meshRenderer.material = highlightedMaterial;

        yield return new WaitForEndOfFrame();

        ChangeMaterialAccordingToCurrentState();
    }

    public void ChangeMaterialAccordingToCurrentState()
    {
        switch (HexTile.TileState)
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
