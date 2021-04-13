using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkCrystal : NetworkCollectable
{
    public bool IsExploding { get; set; }
    [SerializeField] private float extraForceIfLandedWrong = 3f;
    private Rigidbody rb;
    private float maxTimeToExplode = 3f;
    private float explosionProgress;

    void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    [ServerCallback]
    private IEnumerator ExplosionUpdate()
    {
        var distanceFromUndergroundPlane = Mathf.Abs(this.transform.position.y - NetworkCrystalManager.Instance.Underground.transform.position.y);
        
        this.explosionProgress = this.maxTimeToExplode;

        while (distanceFromUndergroundPlane > NetworkCrystalManager.Instance.GetHeightOffset()
            && this.explosionProgress > 0)
        {
            yield return new WaitForFixedUpdate();
            distanceFromUndergroundPlane = Mathf.Abs(this.transform.position.y - NetworkCrystalManager.Instance.Underground.transform.position.y);
            this.explosionProgress -= Time.deltaTime;
        }

        if (this.explosionProgress <= 0)
        {
            this.transform.position = new Vector3(
                this.transform.position.x,
                NetworkCrystalManager.Instance.Underground.transform.position.y + NetworkCrystalManager.Instance.GetHeightOffset(),
                this.transform.position.z);
        }
    }

    [ServerCallback]
    private void FinishExplosion()
    {
        NetworkCrystalManager.Instance.OnDroppedCrystal(this);
        this.IsExploding = false;
        
        rb.useGravity = false;
        rb.isKinematic = true;
        SetTriggerable(true);
    }

    public override void Collect()
    {
        NetworkCrystalManager.Instance.OnCollectedCrystal(this);
        NetworkServer.UnSpawn(this.gameObject);
        Destroy(this.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Collectable"))
        {
            _ = Physics.Raycast(this.transform.position,
            Vector3.down,
            out RaycastHit downwardTileHit,
            3f,
            1 << LayerMask.NameToLayer("Default"));

            if (downwardTileHit.collider != null 
                && !downwardTileHit.collider.gameObject.CompareTag("Player"))
            {
                Vector3 force = new Vector3(Random.Range(-1, 1), Random.Range(0, 1), Random.Range(-1, 1)) * this.extraForceIfLandedWrong;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    [ServerCallback]
    internal void Explode() => StartCoroutine(ExplosionUpdate());
}
