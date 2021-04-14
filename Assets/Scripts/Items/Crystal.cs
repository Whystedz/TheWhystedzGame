using System;
using System.Collections;
using UnityEngine;

public class Crystal : Collectable
{
    private CrystalManager crystalManager;
    public bool IsExploding { get; set; }
    [SerializeField] private float extraForceIfLandedWrong = 3f;
    private Rigidbody rb;
    private float maxTimeToExplode = 3f;
    private float explosionProgress;

    void Awake()
    {
        this.crystalManager = FindObjectOfType<CrystalManager>();
        rb = this.GetComponent<Rigidbody>();
    }

    private IEnumerator ExplosionUpdate()
    {
        var distanceFromUndergroundPlane = Mathf.Abs(this.transform.position.y - crystalManager.underground.transform.position.y);

        this.explosionProgress = this.maxTimeToExplode;

        while (distanceFromUndergroundPlane > this.crystalManager.GetHeightOffset()
            && this.explosionProgress > 0)
        {
            yield return new WaitForFixedUpdate();
            distanceFromUndergroundPlane = Mathf.Abs(this.transform.position.y - crystalManager.underground.transform.position.y);
            this.explosionProgress -= Time.deltaTime;
        }

        if (this.explosionProgress <= 0)
        {
            this.transform.position = new Vector3(
                this.transform.position.x,
                crystalManager.underground.transform.position.y + this.crystalManager.GetHeightOffset(),
                this.transform.position.z);
        }

        FinishExplosion();
    }

    private void FinishExplosion()
    {
        this.crystalManager.OnDroppedCrystal(this);
        this.IsExploding = false;
        
        rb.useGravity = false;
        rb.isKinematic = true;
        this.GetComponent<CapsuleCollider>().isTrigger = true;
    }

    public override void Collect()
    {
        this.crystalManager.OnCollectedCrystal(this);
        base.Collect();
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
                Vector3 force = new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(-1, 1)) * this.extraForceIfLandedWrong;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    internal void Explode() => StartCoroutine(ExplosionUpdate());
}
