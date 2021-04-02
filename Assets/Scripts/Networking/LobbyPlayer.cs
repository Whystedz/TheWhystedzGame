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
        this.networkMatchChecker.matchId = matchId.ToGuid();
    }
}
