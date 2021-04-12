using System.Linq;
using UnityEngine;
using Mirror;

public class TileManager : NetworkBehaviour
{
    private static TileManager instance;

    [SerializeField] private GameObject basicTilePrefab;
    [SerializeField] private float tileSurfaceScale = 115f;
    [SerializeField] private float tileDepthScale = 200f;

    [SerializeField] private int mapWidth = 10;
    [SerializeField] private int mapLength = 15;
    [SerializeField] private float xOffset = 2f;
    [SerializeField] private float zOffset = 2f;
    [SerializeField] private float verticalOffset = 0.1f;

    [SerializeField] internal float timeToBreak = 3f;
    [SerializeField] internal float timeToRespawn = 5f;

    private PlayerMovement playerMovement;

    public static TileManager GetInstance() => instance;

    public SyncList<TileInfo> syncTileList = new SyncList<TileInfo>();

    private void Awake() => MaintainSingleInstance();

    private void Start()
    {
        syncTileList.Callback += OnTileUpdated;
        SpawnMap();
    }

    public override void OnStartServer()
    {
        GenerateTiledMap();
    }

    private void GenerateTiledMap()
    {
        for (byte xIndex = 0; xIndex < mapWidth; ++xIndex)
        {
            for (byte zIndex = 0; zIndex < mapLength; ++zIndex)
            {
                TileInfo tile = new TileInfo
                {
                    Progress = 0f,
                    XIndex = xIndex,
                    ZIndex = zIndex,
                    TileState = TileState.Normal,
                };

                syncTileList.Add(tile);
            }
        }
    }

    private void SpawnMap()
    {
        foreach (TileInfo tile in syncTileList)
        {
            SpawnTile(tile);
        }
    }

    private void SpawnTile(TileInfo tile)
    {
        var position = DetermineSpawnPosition(tile.XIndex, tile.ZIndex);
        var tileToSpawn = TileToSpawn(position);
        var generatedTile = Instantiate(tileToSpawn, this.transform);
        generatedTile.GetComponent<NetworkTile>().TileInfo = tile;

        generatedTile.transform.localScale = new Vector3(
            this.tileSurfaceScale,
            this.tileDepthScale,
            this.tileSurfaceScale);

        generatedTile.transform.position = position;

        // TODO: Add this to the struct so all clients have the same vertical offsets
        AddVerticalOffsetChaos(generatedTile);
    }

    private void AddVerticalOffsetChaos(GameObject generatedTile)
    {
        // for now, let's add some chaos so we can see the tile's depth
        generatedTile.transform.localPosition +=
            Random.Range(-this.verticalOffset, this.verticalOffset) * Vector3.up;
    }

    private Vector3 DetermineSpawnPosition(byte xIndex, byte zIndex)
    {
        // index it
        var position = new Vector3((int)xIndex * this.xOffset, 0, (int)zIndex * this.zOffset);

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

        //if (isServer)
            //return this.basicTilePrefab;

        return biomeRegion.GetRandomBiomeThemedTile();
    }

    public void DigTile(TileInfo targetTile)
    {
        if(isServer)
        {
            int index = syncTileList.FindIndex(x => x.XIndex == targetTile.XIndex && x.ZIndex == targetTile.ZIndex);
            UpdateTile(index, this.timeToBreak, TileState.Unstable);
        }
    }

    public void ResetTile(TileInfo targetTile)
    {
        if(isServer)
        {
            int index = syncTileList.FindIndex(x => x.XIndex == targetTile.XIndex && x.ZIndex == targetTile.ZIndex);
            UpdateTile(index, 0f, TileState.Normal);
        }
    }

    public void SetTileState(TileInfo targetTile, TileState newState, float newProgress)
    {
        if(isServer)
        {
            int index = syncTileList.FindIndex(x => x.XIndex == targetTile.XIndex && x.ZIndex == targetTile.ZIndex);
            UpdateTile(index, newProgress, newState);
        }
    }

    private void UpdateTile(int listIndex, float newProgress, TileState newState)
    {
        TileInfo tempTile = syncTileList[listIndex];
        tempTile.Progress = newProgress;
        tempTile.TileState = newState;
        syncTileList[listIndex] = tempTile;
    }

    public NetworkTile GetTileScript(TileInfo tileInfo)
    {
        int index = syncTileList.FindIndex(x => x.XIndex == tileInfo.XIndex && x.ZIndex == tileInfo.ZIndex);
        return this.transform.GetChild(index).GetComponent<NetworkTile>();
    }

    private void MaintainSingleInstance()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }

    void OnTileUpdated(SyncList<TileInfo>.Operation op, int index, TileInfo oldTile, TileInfo newTile)
    {
        switch (op)
        {
            case SyncList<TileInfo>.Operation.OP_SET:
                var tile = this.transform.GetChild(index).GetComponent<NetworkTile>();
                tile.TileInfo = newTile;
                switch (newTile.TileState)
                {
                    case TileState.Unstable:
                        tile.DigTile();
                        break;
                    case TileState.Respawning:
                        tile.StartRespawning();
                        break;
                    case TileState.Rope:
                        tile.PauseState();
                        break;
                }
                break;
        }
    }
}
