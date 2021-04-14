using System;
using UnityEngine;
using Mirror;

public class NetworkServerObstacles : MonoBehaviour
{
    public void SetMatchId(Guid matchId)
    {
        this.GetComponent<NetworkMatchChecker>().matchId = matchId;
    }
}
