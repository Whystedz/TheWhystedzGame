using System;
using UnityEngine;
using Mirror;

public class LobbyPlayer : NetworkBehaviour
{
    public static LobbyPlayer LocalPlayer { get; set; }
    private NetworkMatchChecker networkMatchChecker;

    void Awake()
    {
        this.networkMatchChecker = GetComponent<NetworkMatchChecker>();
        LocalPlayer = this;
    }

    public void SetMatchIdGuid(string matchId)
    {   
        if (matchId == string.Empty)
        {
            this.networkMatchChecker.matchId = Guid.Empty;
            return;
        }

        this.networkMatchChecker.matchId = matchId.ToGuid();
    }
}
