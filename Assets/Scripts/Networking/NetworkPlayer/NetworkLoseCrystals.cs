using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkLoseCrystals : NetworkBehaviour
{
    private NetworkPlayerScore playerScore;
    private NetworkCrystalManager crystalManager;

    void Awake()
    {
        this.playerScore = GetComponent<NetworkPlayerScore>();
        this.crystalManager = FindObjectOfType<NetworkCrystalManager>();
    }

    public void LoseCrystal()
    {
        CmdSpawnCrystalsBasedOnScore();
        this.playerScore.Substract(this.playerScore.CurrentScore);
    }

    [Command]
    private void CmdSpawnCrystalsBasedOnScore()
    {
        this.crystalManager.DropCrystals(this.transform, this.playerScore.CurrentScore);
    }
}
