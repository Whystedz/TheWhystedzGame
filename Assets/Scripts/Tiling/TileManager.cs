using System.Linq;
using UnityEngine;
using System.Collections;
using System.Threading;
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

    [SerializeField] private float timeToBreak = 3f;
    [SerializeField] private float timeToRespawn = 5f;
    [SerializeField] private float timeOfBreakingAnimation = 5f;

    public static TileManager GetInstance() => instance;

    public SyncList<HexTile> syncTileList = new SyncList<HexTile>();

    private void Awake() => MaintainSingleInstance();

    void Start()
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
        for (int xIndex = 0; xIndex < mapWidth; ++xIndex)
        {
            for (int zIndex = 0; zIndex < mapLength; ++zIndex)
            {
                HexTile tile = new HexTile
                {
                    TimeToRespawn = this.timeToRespawn,
                    TimeOfBreakingAnimation = this.timeOfBreakingAnimation,
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
        foreach (HexTile tile in syncTileList)
        {
            SpawnTile(tile);
        }
    }

    private void SpawnTile(HexTile tile)
    {
        var position = DetermineSpawnPosition(tile.XIndex, tile.ZIndex);
        var tileToSpawn = TileToSpawn(position);
        var generatedTile = Instantiate(tileToSpawn, this.transform);
        generatedTile.GetComponent<NetworkTile>().HexTile = tile;

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

        var biomeRegion = colliders[0].GetComponent<BiomeRegion>();

        return biomeRegion.GetRandomBiomeThemedTile();
    }

    public void DigTile(HexTile targetTile)
    {
        if(isServer)
        {
            int index = syncTileList.FindIndex(x => x.XIndex == targetTile.XIndex && x.ZIndex == targetTile.ZIndex);
            UpdateTile(index, this.timeToBreak, TileState.Unstable);
        }
    }

    public void ResetTile(HexTile targetTile)
    {
        if(isServer)
        {
            int index = syncTileList.FindIndex(x => x.XIndex == targetTile.XIndex && x.ZIndex == targetTile.ZIndex);
            UpdateTile(index, 0f, TileState.Normal);
        }
    }

    private void MaintainSingleInstance()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }

    public void UpdateTile(int listIndex, float newProgress, TileState newState)
    {
        HexTile tempTile = syncTileList[listIndex];
        tempTile.Progress = newProgress;
        tempTile.TileState = newState;
        syncTileList[listIndex] = tempTile;
    }

    void OnTileUpdated(SyncList<HexTile>.Operation op, int index, HexTile oldTile, HexTile newTile)
    {
        switch (op)
        {
            case SyncList<HexTile>.Operation.OP_SET:
                //Debug.Log("Set");
                StartCoroutine(UpdateMap(index, newTile));
                break;
        }
    }

    IEnumerator UpdateMap(int index, HexTile newTile)
    {
        //Debug.Log("Update Map");
        this.transform.GetChild(index).GetComponent<NetworkTile>().HexTile = newTile;

        if(newTile.TileState == TileState.Normal)
            yield break;
            
        while(this.transform.GetChild(index).GetComponent<NetworkTile>().HexTile.TileState != TileState.Normal)
            yield return new WaitForEndOfFrame();
        
        ResetTile(this.transform.GetChild(index).GetComponent<NetworkTile>().HexTile);
    }
}
