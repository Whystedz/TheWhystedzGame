using System;
using System.Linq;
using UnityEngine;
using Mirror;

public class NetworkCrystalManager : NetworkBehaviour
{
    [SerializeField] private GameObject crystalPrefab;

    [SerializeField] private int maxCrystalsSurface = 200;
    [SerializeField] private int maxCrystalsGame = 500;

    [SerializeField] private float surfaceBoundsOffset = 5f;
    [SerializeField] private float undergroundBoundsOffset = 5f;

    [SerializeField] private float crystalVerticalOffsetHeight = 2.25f;

    [SerializeField] private float spawningCollisionPaddingRadius = 1f;

    [SerializeField] private float radiusOfSpawnCircle = .05f;
    [SerializeField] private float radiusOfForce = 10.0f;
    [SerializeField] private float power = 30.0f;

    [SerializeField] private GameObject surfacePlane;
    [SerializeField] private GameObject undergroundPlane;

    private PlaneBounds surface;
    public PlaneBounds Underground;

    private int totalCrystalsInstantiated;
    private int currentCrystalsSurface;
    private int currentCrystalsUnderground;

    public override void OnStartServer()
    {
        Instantiate(surfacePlane);
        Instantiate(undergroundPlane);
    }

    void Start()
    {
        this.surface = GameObject.FindGameObjectWithTag("Surface")
            .GetComponent<PlaneBounds>();
        Underground = GameObject.FindGameObjectWithTag("Underground")
            .GetComponent<PlaneBounds>();
    }

    [ServerCallback]
    void Update()
    {
        if (this.totalCrystalsInstantiated < this.maxCrystalsGame
            && this.currentCrystalsSurface < this.maxCrystalsSurface)
            SpawnCrystal();
    }

    private void SpawnCrystal(int attempts = 0)
    {
        Debug.Log("A crystal start to spawn");
        var randomPositionOnSurface = RandomPointInRectangle(this.surface.UpperLeftCorner,
            this.surface.UpperRightCorner,
            this.surface.LowerLeftCorner,
            this.surface.LowerRightCorner);

        var chosenPosition = randomPositionOnSurface
            + Vector3.up * this.crystalVerticalOffsetHeight;

        var obstacleColliders = Physics.OverlapSphere(chosenPosition,
            this.spawningCollisionPaddingRadius,
            LayerMask.NameToLayer("Tile"));

        if (obstacleColliders.Count() >= 1 || !HasSolidTileUnderneath(randomPositionOnSurface))
        {
            if (attempts > 10)
            {
                Debug.LogWarning($"Too many attempts. Skipping the spawn this update");
                return;
            }

            SpawnCrystal(++attempts);

            return;
        }

        var crystal = Instantiate(this.crystalPrefab, this.transform);
        crystal.GetComponent<NetworkMatchChecker>().matchId = gameObject.GetComponent<NetworkMatchChecker>().matchId;

        crystal.transform.position = chosenPosition;

        NetworkServer.Spawn(crystal);

        this.currentCrystalsSurface += 1;
        this.totalCrystalsInstantiated += 1;
    }

    private static bool HasSolidTileUnderneath(Vector3 randomPositionOnSurface)
    {
        _ = Physics.Raycast(randomPositionOnSurface + Vector3.down * 10f,
            Vector3.up,
            out RaycastHit downwardTileHit,
            10f,
            1 << LayerMask.NameToLayer("Tile"));

        if (downwardTileHit.collider is null)
            return false;

        var tile = downwardTileHit.collider.gameObject.GetComponentInParent<NetworkTile>().TileInfo;

        return tile.TileState == TileState.Normal
            || tile.TileState == TileState.Unbreakable;
    }

    private Vector3 RandomPointInRectangle(Vector3 upperLeft, Vector3 upperRight, Vector3 lowerLeft, Vector3 lowerRight)
    {
        var lowerBoundX = upperLeft.x;
        var upperBoundX = upperRight.x;

        var lowerBoundZ = lowerLeft.z;
        var upperBoundZ = upperLeft.z;

        return new Vector3(
            UnityEngine.Random.Range(lowerBoundX, upperBoundX),
            0,
            UnityEngine.Random.Range(lowerBoundZ, upperBoundZ)
        );
    }

    public void OnCollectedCrystal(NetworkCrystal crystal)
    {
        var distanceToSurface = crystal.transform.position.y - this.surface.transform.position.y;
        var distanceToUnderground = crystal.transform.position.y - Underground.transform.position.y;

        if (distanceToSurface < distanceToUnderground)
            this.currentCrystalsSurface -= 1;
        else
            this.currentCrystalsUnderground -= 1;
    }

    // @Zahra, here's a method for you to use when you do the lost crystals underground!
    public void OnDroppedCrystal(NetworkCrystal crystal)
    {
        var distanceToSurface = crystal.transform.position.y - this.surface.transform.position.y;
        var distanceToUnderground = crystal.transform.position.y - Underground.transform.position.y;

        if (distanceToSurface < distanceToUnderground)
            this.currentCrystalsSurface += 1;
        else
            this.currentCrystalsUnderground += 1;
    }

    public float GetHeightOffset() => crystalVerticalOffsetHeight;

    public void DropCrystals(Vector3 playerPos, int amount, Guid matchId)
    {
        DropPlayerCrystals(playerPos, amount, matchId);
    }

    [ServerCallback]
    private void DropPlayerCrystals(Vector3 playerPos, int amount, Guid matchId)
    {
        for (int i = 0; i < amount; i++)
        {
            var randomizedPos = UnityEngine.Random.insideUnitCircle * radiusOfSpawnCircle;
            var spawnPos = playerPos + new Vector3(randomizedPos.x, crystalVerticalOffsetHeight, randomizedPos.y);
            var crystal = Instantiate(this.crystalPrefab, spawnPos, Quaternion.identity);
            crystal.GetComponent<NetworkMatchChecker>().matchId = matchId;
            crystal.GetComponent<CapsuleCollider>().isTrigger = false;
            crystal.GetComponent<NetworkCrystal>().Explode();

            NetworkServer.Spawn(crystal);

            OnDroppedCrystal(crystal.GetComponent<NetworkCrystal>());
        }

        ExplodeCrystals(playerPos);
    }

    private void ExplodeCrystals(Vector3 playerPos)
    {
        Vector3 explosionPos = playerPos + Vector3.up * crystalVerticalOffsetHeight;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radiusOfForce);
        foreach (Collider hit in colliders)
        {
            var crystal = hit.GetComponent<NetworkCrystal>();
            if (crystal is null)
                continue;
            
            Rigidbody rb = crystal.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.useGravity = true;
                rb.velocity = Vector3.zero;
                rb.AddExplosionForce(power, explosionPos, radiusOfForce, 0.05f, ForceMode.Impulse);
            }
        }
    }
}
