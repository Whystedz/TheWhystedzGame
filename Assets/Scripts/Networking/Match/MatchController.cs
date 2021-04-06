using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;

public class MatchController : NetworkBehaviour
{
    internal readonly SyncDictionary<NetworkIdentity, MatchPlayerData> matchPlayerData = new SyncDictionary<NetworkIdentity, MatchPlayerData>();

    bool playAgain = false;

    private LobbyNetworkManager networkManager;

    [Header("Diagnostics - Do Not Modify")]
    public CanvasController canvasController;

    public NetworkIdentity tileManagerIdentity;
    public List<NetworkIdentity> playerIdentities;


    void Awake()
    {
        this.canvasController = FindObjectOfType<CanvasController>();
        this.networkManager = GameObject.FindWithTag("NetworkManager").GetComponent<LobbyNetworkManager>();
        DontDestroyOnLoad(this);
    }

    public override void OnStartServer()
    {
        StartCoroutine(AddPlayersToMatchController());
    }

    // For the SyncDictionary to properly fire the update callback, we must
    // wait a frame before adding the players to the already spawned MatchController
    private IEnumerator AddPlayersToMatchController()
    {
        yield return null;

        foreach (var playerIdentity in this.playerIdentities)
        {
            this.matchPlayerData.Add(playerIdentity, new MatchPlayerData
                {
                    playerName = LobbyNetworkManager.playerInfos[playerIdentity.connectionToClient].DisplayName,
                    currentScore = 0,
                    team = LobbyNetworkManager.playerInfos[playerIdentity.connectionToClient].Team
                }
            );
        }
    }

    public override void OnStartClient()
    {
        matchPlayerData.Callback += UpdateScoreUI;
        // TODO: initialize Score UI 
    }

    // TODO: update score UI in the main scene
    public void UpdateScoreUI(SyncDictionary<NetworkIdentity, MatchPlayerData>.Operation op, NetworkIdentity key, MatchPlayerData newMatchPlayerData)
    {
        if (key.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
        {
        }
        else
        {
        }
    }

    // TODO: bind this with an exit button
    [Client]
    public void ReqestExitGame()
    {
        CmdRequestExitGame();
    }

    [Command(ignoreAuthority = true)]
    public void CmdRequestExitGame(NetworkConnectionToClient sender = null)
    {
        StartCoroutine(ServerEndMatch(sender, false));
    }

    // registered to OnPlayerDisconnected on networkManager
    public void OnPlayerDisconnected(NetworkConnection connection)
    {
        // Check that the disconnecting client is a player in this match
        // stop the match if one player in match disconnected
        // TODO: is this what we want?
        if (this.playerIdentities.Contains(connection.identity))
        {
            StartCoroutine(ServerEndMatch(connection, true));
        }
    }

    /// <summary>
    /// End a match; called on server
    /// </summary>
    /// <param name="conn">if this is called because someone disconnect, the connection of disconnected player</param>
    /// <param name="disconnected">is this called because someone disconnected</param>
    /// <returns></returns>
    public IEnumerator ServerEndMatch(NetworkConnection connection, bool disconnected)
    {
        networkManager.OnPlayerDisconnected -= OnPlayerDisconnected;

        RpcExitGame();

        // Skip a frame so the message goes out ahead of object destruction
        yield return null;

        // Mirror will clean up the disconnecting client so we only need to clean up the other remaining client.
        // If both players are just returning to the Lobby, we need to remove both connection Players

        if (!disconnected)
        {
            // send everyone to lobby 
            foreach (var player in this.playerIdentities)
            {
                NetworkServer.RemovePlayerForConnection(player.connectionToClient, true);
                LobbyNetworkManager.waitingConnections.Add(player.connectionToClient);
            }
        }
        else
        {
            // send everyone to lobby, except the one disconnected
            var disconnectedPlayer = this.playerIdentities.Find(x => x.connectionToClient == connection);

            foreach (var player in this.playerIdentities)
            {
                if (player == disconnectedPlayer) continue;
                NetworkServer.RemovePlayerForConnection(player.connectionToClient, true);
                LobbyNetworkManager.waitingConnections.Add(player.connectionToClient);
            }
        }

        // Skip a frame to allow the Removal(s) to complete
        yield return null;

        // Send latest match list
        LobbyNetworkManager.SendMatchList();

        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    public void RpcExitGame()
    {
        canvasController.OnMatchEnded();
    }
}
