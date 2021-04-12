using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkLoseCrystals : NetworkBehaviour
{
    private NetworkPlayerScore playerScore;
    private NetworkCrystalManager crystalManager;

    void Start()
    {
        this.playerScore = GetComponent<NetworkPlayerScore>();
        this.crystalManager = FindObjectOfType<NetworkCrystalManager>();
    }

    public void LoseCrystal(Vector3 position)
    {
        CmdSpawnCrystalsBasedOnScore(position);
        this.playerScore.Subtract(this.playerScore.currentScore);
    }

    [Command]
    private void CmdSpawnCrystalsBasedOnScore(Vector3 position)
    {
        this.crystalManager.DropCrystals(position, this.playerScore.currentScore, this.GetComponent<NetworkMatchChecker>().matchId);
    }
}
