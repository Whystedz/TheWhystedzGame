using System.Linq;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField] private GameObject basicTilePrefab;
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
                var position = DetermineSpawnPosition(xIndex, zIndex);

                var tileToSpawn = TileToSpawn(position);

                var generatedTile = Instantiate(tileToSpawn, this.transform);

                generatedTile.transform.localScale = new Vector3(
                    this.tileSurfaceScale,
                    this.tileDepthScale,
                    this.tileSurfaceScale);

                generatedTile.transform.position = position;

                AddVerticalOffsetChaos(generatedTile);
            }
        }
    }

    private void AddVerticalOffsetChaos(GameObject generatedTile)
    {
        // for now, let's add some chaos so we can see the tile's depth
        generatedTile.transform.localPosition +=
            Random.Range(-this.verticalOffset, this.verticalOffset) * Vector3.up;
    }

    private Vector3 DetermineSpawnPosition(int xIndex, int zIndex)
    {
        // index it
        var position = new Vector3(xIndex * this.xOffset, 0, zIndex * this.zOffset);

        // center it
        position += new Vector3(-mapWidth / 2 * this.xOffset, 0, -mapLength / 2 * this.zOffset);

        // offset it based on row
        if (zIndex % 2 == 0)
            position += 0.5f * this.xOffset * Vector3.right;

        return position;
    }

    private GameObject TileToSpawn(Vector3 position)
    {
        var colliders = Physics.OverlapSphere(position, 1f);

        colliders = colliders
            .Where(collider => collider.GetComponent<BiomeRegion>() != null)
            .ToArray();

        if (colliders.Length == 0)
            return this.basicTilePrefab;

        var biomeRegion = colliders
            .Where(collider => collider.GetComponent<BiomeRegion>() != null)
            .Select(collider => collider.GetComponent<BiomeRegion>())
            .OrderBy(biome => Vector3.Distance(transform.position, biome.transform.position))
            .First();

        return biomeRegion.GetRandomBiomeThemedTile();
    }
}
