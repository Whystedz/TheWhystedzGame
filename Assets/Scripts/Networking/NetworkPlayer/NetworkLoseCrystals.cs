using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkLoseCrystals : NetworkBehaviour
{
    private NetworkPlayerScore playerScore;

    void Start() => this.playerScore = GetComponent<NetworkPlayerScore>();

    public void LoseCrystal(Vector3 position)
    {
        CmdSpawnCrystalsBasedOnScore(position);
        this.playerScore.Subtract(this.playerScore.currentScore);
    }

    [Command]
    private void CmdSpawnCrystalsBasedOnScore(Vector3 position)
    {
        NetworkCrystalManager.Instance.DropCrystals(position, this.playerScore.currentScore, this.GetComponent<NetworkMatchChecker>().matchId);
    }
}
