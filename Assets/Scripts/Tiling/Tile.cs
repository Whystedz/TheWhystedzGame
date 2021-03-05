using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material unstableMaterial;

    [SerializeField] private bool isClickToDigEnabled;

    private Material material;

    private TileState tileState;

    // Update is called once per frame
    void Update()
    {
        switch (this.tileState)
        {
            case TileState.Normal:
                break;
            case TileState.Unstable:
                break;
            case TileState.Respawning:
                break;
        }
    }

    // Should be called by the agent wishing to dig the tile
    public void DigTile() => Destroy(this.gameObject);

    // For debug purposes, dig a tile up upon clicking on it
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!this.isClickToDigEnabled)
            return;

        DigTile();
    }
}
