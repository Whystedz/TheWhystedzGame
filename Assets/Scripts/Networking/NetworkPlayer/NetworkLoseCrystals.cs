using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkLoseCrystals : NetworkBehaviour
{
    private NetworkPlayerScore playerScore;
    private NetworkCrystalManager crystalManager;
    [SerializeField] private float crystalheightOffsetFromPlayer = 3f;
    [SerializeField] private float radiusOfForce = 200.0f;
    [SerializeField] private float power = 100.0f;

    void Awake()
    {
        this.playerScore = GetComponent<NetworkPlayerScore>();
        this.crystalManager = FindObjectOfType<NetworkCrystalManager>();
    }

    public void LoseCrystal()
    {
        CmdSpawnCrystalsBasedOnScore();
        ExplodeCoins();
        this.playerScore.Substract(this.playerScore.CurrentScore);
    }

    private void ExplodeCoins()
    {
        Vector3 explosionPos = transform.position + Vector3.up * crystalheightOffsetFromPlayer;
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
                rb.AddExplosionForce(power, explosionPos, radiusOfForce, 0.05f,ForceMode.Impulse);
            }
        }
    }

    [Command]
    private void CmdSpawnCrystalsBasedOnScore()
    {
        this.crystalManager.DropCrystals(this.transform, this.playerScore.CurrentScore);
    }
}
