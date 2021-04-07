using System.Linq;
using UnityEngine;
using Mirror;

public class NetworkCrystalManager : MonoBehaviour
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

    [SerializeField] private PlaneBounds surface;
    [SerializeField] public PlaneBounds Underground;

    private int totalCrystalsInstantiated;
    private int currentCrystalsSurface;
    private int currentCrystalsUnderground;

    [ServerCallback]
    void Update()
    {
        if (this.totalCrystalsInstantiated < this.maxCrystalsGame
            && this.currentCrystalsSurface < this.maxCrystalsSurface)
            SpawnCrystal();
    }

    private void SpawnCrystal(int attempts = 0)
    {
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

        crystal.transform.position = chosenPosition;
        crystal.GetComponent<NetworkCollectable>().UpdateRestPosition(chosenPosition);

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
            Random.Range(lowerBoundX, upperBoundX),
            0,
            Random.Range(lowerBoundZ, upperBoundZ)
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

    [ServerCallback]
    public void DropCrystals(Transform player, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var randomizedPos = UnityEngine.Random.insideUnitCircle * radiusOfSpawnCircle;
            var spawnPos = player.transform.position + new Vector3(randomizedPos.x, crystalVerticalOffsetHeight, randomizedPos.y);
            var crystal = Instantiate(this.crystalPrefab, spawnPos, Quaternion.identity);
            crystal.transform.parent = this.transform;
            crystal.GetComponent<NetworkCrystal>().IsExploding = true;
            crystal.GetComponent<CapsuleCollider>().isTrigger = false;

            NetworkServer.Spawn(crystal);

            OnDroppedCrystal(crystal.GetComponent<NetworkCrystal>());
        }

        ExplodeCrystals(player);
    }

    private void ExplodeCrystals(Transform player)
    {
        Vector3 explosionPos = player.transform.position + Vector3.up * crystalVerticalOffsetHeight;
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