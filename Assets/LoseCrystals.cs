using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseCrystals : MonoBehaviour
{
    private PlayerScore playerScore;
    [SerializeField] private float radiusOfForce = 200.0f;
    [SerializeField] private float radiusOfSpawnCircle = .05f;
    [SerializeField] private float power = 100.0f;
    [SerializeField] private GameObject crystalPrefab;
    [SerializeField] private float crystalheightOffsetFromPlayer = 3f;
    private CrystalManager crystalManager;
    // Start is called before the first frame update
    void Awake()
    {
        this.playerScore = this.GetComponent<PlayerScore>();
        this.crystalManager = FindObjectOfType<CrystalManager>();

    }

    public void LoseCrystal()
    {
        SpawnCrystalsBasedOnScore();
        ExplodeCoins();
        this.playerScore.Substract(this.playerScore.CurrentScore);
    }

    private void ExplodeCoins()
    {
        Vector3 explosionPos = transform.position + Vector3.up* crystalheightOffsetFromPlayer;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radiusOfForce);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.useGravity = true;
                rb.velocity = Vector3.zero;
                rb.AddExplosionForce(power, explosionPos, radiusOfForce, 0.05f,ForceMode.Impulse);
            }
        }
    }

    private void SpawnCrystalsBasedOnScore()
    {
        for (int i = 0; i < this.playerScore.CurrentScore; i++)
        {
            var randomizedPos = UnityEngine.Random.insideUnitCircle * radiusOfSpawnCircle;
            var spawnPos = transform.position + new Vector3(randomizedPos.x, crystalheightOffsetFromPlayer, randomizedPos.y);
            
            var crystal = Instantiate(crystalPrefab, spawnPos, Quaternion.identity);
            
            crystal.transform.parent = this.crystalManager.transform;
                        
            crystal.GetComponent<Crystal>().Explode();
            crystal.GetComponent<CapsuleCollider>().isTrigger = false;
        }

    }
}
