using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private float tileSurfaceScale = 115f;
    [SerializeField] private float tileDepthScale = 200f;

    [SerializeField] private int mapWidth = 10;
    [SerializeField] private int mapLength = 15;
    [SerializeField] private float xOffset = 2f;
    [SerializeField] private float zOffset = 2f;
    [SerializeField] private float verticalOffset = 0.1f;

    void Start() => GenerateTiledMap();

    private void GenerateTiledMap()
    {
        for (int xIndex = 0; xIndex < mapWidth; ++xIndex)
        {
            for (int zIndex = 0; zIndex < mapLength; ++zIndex)
            {
                var generatedTile = Instantiate(this.tilePrefab, this.transform);

                generatedTile.transform.localScale = new Vector3(
                    this.tileSurfaceScale, 
                    this.tileDepthScale, 
                    this.tileSurfaceScale);

                generatedTile.transform.position = new Vector3(
                    xIndex * this.xOffset, 
                    0, 
                    zIndex * this.zOffset);
                
                if (zIndex % 2 == 0)
                    generatedTile.transform.position += 0.5f * this.xOffset * Vector3.right;

                // for now, let's add some chaos so we can see the tile's depth
                generatedTile.transform.localPosition += 
                    Random.Range(-this.verticalOffset, this.verticalOffset) * Vector3.up;
            }
        }
    }
    
}
