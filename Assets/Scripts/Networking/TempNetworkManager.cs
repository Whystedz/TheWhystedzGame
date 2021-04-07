using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class TempNetworkManager : NetworkManager
{
    [Header("Player characters")]
    [SerializeField] private List<GameObject> playerPrefabs;
    
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        var rnd = Random.Range(0, playerPrefabs.Count);

        var player = Instantiate(playerPrefabs[rnd]);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
