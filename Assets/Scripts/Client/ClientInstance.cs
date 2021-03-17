using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ClientInstance : NetworkBehaviour
{
    [SerializeField]
    private NetworkIdentity playerPrefab = null;

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkSpawnPlayer();
    }
    
    private void NetworkSpawnPlayer()
    {
        GameObject go = Instantiate(playerPrefab.gameObject, transform.position, Quaternion.identity);
        NetworkServer.Spawn(go, base.connectionToClient);
    }

}
