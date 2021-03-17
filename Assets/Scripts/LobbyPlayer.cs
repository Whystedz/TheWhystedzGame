using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkBehaviour
{
    public static LobbyPlayer localPlayer;
    [SyncVar] public string matchID;
    [SyncVar] public int playerIndex;
    NetworkMatchChecker networkMatchChecker;
    GameObject playerLobbyUI;

    //[SyncVar(hook = nameof(OnNameChanged))] 
    public string DisplayName;

    void Awake()
    {
        networkMatchChecker = GetComponent<NetworkMatchChecker>();
    }

    void OnNameChanged(string oldName, string newName)
    {
        DisplayName = newName;
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            localPlayer = this;
        }
        else
        {
            Debug.Log($"Spawning other player UI");
            playerLobbyUI = UILobby.instance.SpawnUIPlayerPrefab(this);
        }
    }

    [Command]
    public void CmdSetupPlayer(string displayName)
    {
        DisplayName = displayName;
    }

    public override void OnStopClient()
    {
        Debug.Log($"Client stopped");
        ClientDisconnect();
    }

    public override void OnStopServer()
    {
        Debug.Log($"Client stopped on server");
        ServerDisconnect();
    }

    //---- HOST GAME LOGIC ----

    public void HostGame(bool publicMatch)
    {
        string matchID = MatchMaker.GetRandomMatchID();
        CmdHostGame(matchID, publicMatch);
    }

    [Command]
    void CmdHostGame(string _matchID, bool publicMatch)
    {
        if (MatchMaker.instance.HostGame(_matchID, gameObject, publicMatch, out playerIndex))
        {
            matchID = _matchID;
            Debug.Log($"<color=green>Game hosted successfully</color>");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetHostGame(true, _matchID, playerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game host failed</color>");
            TargetHostGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    void TargetHostGame(bool success, string _matchID, int _playerIndex)
    {
        playerIndex = _playerIndex;
        Debug.Log($"Match ID: {matchID} == {_matchID}");
        UILobby.instance.HostSuccess(success, _matchID);
    }

    //---- JOIN GAME LOGIC ----

    public void JoinGame(string _inputID)
    {
        CmdJoinGame(_inputID);
    }

    [Command]
    void CmdJoinGame(string _matchID)
    {
        if (MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
        {
            matchID = _matchID;
            Debug.Log($"<color=green>Game joined successfully</color>");
            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetJoinGame(true, _matchID, playerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game join failed</color>");
            TargetJoinGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success, string _matchID, int _playerIndex)
    {
        playerIndex = _playerIndex;
        matchID = _matchID;
        Debug.Log($"Match ID: {matchID} == {_matchID}");
        UILobby.instance.JoinSuccess(success, _matchID);
    }

    //---- SEARCH GAME LOGIC ----

    public void SearchGame()
    {
        CmdSearchGame();
    }

    [Command]
    public void CmdSearchGame()
    {
        if (MatchMaker.instance.SearchGame(gameObject, out playerIndex, out matchID))
        {
            Debug.Log($"<color=green>Game found</color>");
            networkMatchChecker.matchId = matchID.ToGuid();
            TargetSearchGame(true, matchID, playerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game not found</color>");
            TargetSearchGame(false, matchID, playerIndex);
        }
    }

    [TargetRpc]
    public void TargetSearchGame(bool success, string _matchID, int _playerIndex)
    {
        playerIndex = _playerIndex;
        matchID = _matchID;
        Debug.Log($"Match ID: {matchID} == {_matchID}");
        UILobby.instance.SearchSuccess(success, _matchID);
    }

    //---- START GAME LOGIC ----

    public void StartGame()
    {
        CmdStartGame();
    }

    [Command]
    void CmdStartGame()
    {
        MatchMaker.instance.StartGame(matchID);
        Debug.Log($"<color=red>Game starting</color>");
    }

    public void StartMatch()
    {
        TargetStartGame();
    }

    [TargetRpc]
    void TargetStartGame()
    {
        Debug.Log($"Match ID: {matchID} | Starting...");
        // Load game scene
        SceneManager.LoadScene(2);
    }

    //---- DISCONNECT LOGIC ----

    public void DisconnectGame()
    {
        CmdDisconnectGame();
    }

    [Command]
    public void CmdDisconnectGame()
    {
        ServerDisconnect();
    }

    void ServerDisconnect()
    {
        MatchMaker.instance.PlayerDisconnected(this, matchID);
        networkMatchChecker.matchId = string.Empty.ToGuid();
        RpcDisconnectGame();
    }

    [ClientRpc]
    void RpcDisconnectGame()
    {
        ClientDisconnect();
    }

    void ClientDisconnect()
    {
        if(playerLobbyUI != null)
            Destroy(playerLobbyUI);
    }
}
