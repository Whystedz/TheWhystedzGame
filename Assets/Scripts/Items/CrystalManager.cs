
using System.Linq;
using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    [SerializeField] private GameObject CrystalPrefab;

    [SerializeField] private int maxCrystalsSurface = 200;
    [SerializeField] private int maxCrystalsGame = 500;

    [SerializeField] private float surfaceBoundsOffset = 5f;
    [SerializeField] private float undergroundBoundsOffset = 5f;
    
    [SerializeField] private float crystalVerticalOffsetHeight = 2f;
    
    [SerializeField] private float spawningCollisionPaddingRadius = 1f;

    private PlaneBounds surface;
    private PlaneBounds underground;

    private int totalCrystalsInstantiated;
    private int currentCrystalsSurface;
    private int currentCrystalsUnderground;

    void Awake()
    {
        this.surface = GameObject.FindGameObjectWithTag("Surface")
            .GetComponent<PlaneBounds>();
        this.underground = GameObject.FindGameObjectWithTag("Underground")
            .GetComponent<PlaneBounds>();
    }

    void Update()
    {
        if (this.totalCrystalsInstantiated < this.maxCrystalsGame
            && this.currentCrystalsSurface < this.maxCrystalsSurface)
            SpawnCrystal();
    }

    private void SpawnCrystal(int attempts = 0)
    {
        var randomPositionOnSurface = RandomPointInRectangle(surface.UpperLeftCorner,
            surface.UpperRightCorner,
            surface.LowerLeftCorner,
            surface.LowerRightCorner);

        var chosenPosition = randomPositionOnSurface
            + Vector3.up * crystalVerticalOffsetHeight;

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

        var crystal = Instantiate(this.CrystalPrefab, this.transform);

        crystal.transform.position = chosenPosition;
        crystal.GetComponent<Collectable>().UpdateRestPosition(chosenPosition);

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

        var tile = downwardTileHit.collider.gameObject.GetComponentInParent<Tile>();

        return tile.tileState == TileState.Normal;
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

    public void OnCollectedCrystal(Crystal crystal)
    {
        var distanceToSurface = crystal.transform.position.y - this.surface.transform.position.y;
        var distanceToUnderground = crystal.transform.position.y - this.underground.transform.position.y;

        if (distanceToSurface < distanceToUnderground)
            this.currentCrystalsSurface -= 1;
        else
            this.currentCrystalsUnderground -= 1;
    }

    // @Zahra, here's a method for you to use when you do the lost crystals underground!
    public void OnDroppedCrystal(Crystal crystal)
    {
        var distanceToSurface = crystal.transform.position.y - this.surface.transform.position.y;
        var distanceToUnderground = crystal.transform.position.y - this.underground.transform.position.y;

        if (distanceToSurface < distanceToUnderground)
            this.currentCrystalsSurface += 1;
        else
            this.currentCrystalsUnderground += 1;
    }
}
