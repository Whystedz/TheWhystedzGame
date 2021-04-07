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

        // Instiate Player Prefab.
        var player = Instantiate(playerPrefabs[rnd]);
        // object spawned via this function will be a local player
        // which belongs to the client connection who called the ClientScene.AddPlayer
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
