using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkCrystal : NetworkCollectable
{
    private NetworkCrystalManager crystalManager;
    public bool IsExploding { get; set; }
    [SerializeField] private float extraForceIfLandedWrong = 3f;
    private Rigidbody rb;

    void Awake()
    {
        this.crystalManager = FindObjectOfType<NetworkCrystalManager>();
        rb = this.GetComponent<Rigidbody>();
    }

    private IEnumerator ExplosionUpdate()
    {
        var distanceFromUndergroundPlane = Mathf.Abs(this.transform.position.y - crystalManager.Underground.transform.position.y);

        while (distanceFromUndergroundPlane > this.crystalManager.GetHeightOffset())
        {
            yield return new WaitForFixedUpdate();
            distanceFromUndergroundPlane = Mathf.Abs(this.transform.position.y - crystalManager.Underground.transform.position.y);
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
                Vector3 force = new Vector3(Random.Range(-1, 1), Random.Range(0, 1), Random.Range(-1, 1)) * this.extraForceIfLandedWrong;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    [ServerCallback]
    internal void Explode() => StartCoroutine(ExplosionUpdate());
}
