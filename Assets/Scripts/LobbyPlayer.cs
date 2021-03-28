using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkBehaviour
{
    public static LobbyPlayer LocalPlayer { get; set; }
    [SyncVar] public string MatchID;
    [SyncVar] public int PlayerIndex;
    [SyncVar] public bool IsReady;
    private NetworkMatchChecker networkMatchChecker;
    private GameObject playerLobbyUI;

    [SyncVar(hook = nameof(OnNameChanged))] 
    public string DisplayName;

    void Awake()
    {
        this.networkMatchChecker = GetComponent<NetworkMatchChecker>();
    }

    void OnNameChanged(string oldName, string newName)
    {
        DisplayName = newName;
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            LocalPlayer = this;
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
    void CmdHostGame(string matchID, bool publicMatch)
    {
        if (MatchMaker.Instance.HostGame(matchID, gameObject, publicMatch, out PlayerIndex))
        {
            MatchID = matchID;
            Debug.Log($"<color=green>Game hosted successfully</color>");
            this.networkMatchChecker.matchId = matchID.ToGuid();
            TargetHostGame(true, matchID, PlayerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game host failed</color>");
            TargetHostGame(false, matchID, PlayerIndex);
        }
    }

    [TargetRpc]
    void TargetHostGame(bool success, string matchID, int _playerIndex)
    {
        PlayerIndex = _playerIndex;
        Debug.Log($"Match ID: {MatchID} == {matchID}");
        UILobby.instance.HostSuccess(success, matchID);
    }

    //---- JOIN GAME LOGIC ----

    public void JoinGame(string _inputID)
    {
        CmdJoinGame(_inputID);
    }

    [Command]
    void CmdJoinGame(string matchID)
    {
        if (MatchMaker.Instance.JoinGame(matchID, gameObject, out PlayerIndex))
        {
            MatchID = matchID;
            Debug.Log($"<color=green>Game joined successfully</color>");
            this.networkMatchChecker.matchId = matchID.ToGuid();
            TargetJoinGame(true, matchID, PlayerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game join failed</color>");
            TargetJoinGame(false, matchID, PlayerIndex);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success, string matchID, int _playerIndex)
    {
        PlayerIndex = _playerIndex;
        MatchID = matchID;
        Debug.Log($"Match ID: {MatchID} == {matchID}");
        UILobby.instance.JoinSuccess(success, matchID);
    }

    //---- SEARCH GAME LOGIC ----

    public void SearchGame()
    {
        CmdSearchGame();
    }

    [Command]
    public void CmdSearchGame()
    {
        if (MatchMaker.Instance.SearchGame(gameObject, out PlayerIndex, out MatchID))
        {
            Debug.Log($"<color=green>Game found</color>");
            this.networkMatchChecker.matchId = MatchID.ToGuid();
            TargetSearchGame(true, MatchID, PlayerIndex);
        }
        else
        {
            Debug.Log($"<color=red>Game not found</color>");
            TargetSearchGame(false, MatchID, PlayerIndex);
        }
    }

    [TargetRpc]
    public void TargetSearchGame(bool success, string matchID, int _playerIndex)
    {
        PlayerIndex = _playerIndex;
        MatchID = matchID;
        Debug.Log($"Match ID: {MatchID} == {matchID}");
        UILobby.instance.SearchSuccess(success, matchID);
    }

    //---- START GAME LOGIC ----

    public void StartGame()
    {
        CmdStartGame();
    }

    [Command]
    void CmdStartGame()
    {
        MatchMaker.Instance.StartGame(MatchID);
        Debug.Log($"<color=red>Game starting</color>");
    }

    public void StartMatch()
    {
        TargetStartGame();
    }

    [TargetRpc]
    void TargetStartGame()
    {
        Debug.Log($"Match ID: {MatchID} | Starting...");
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
        MatchMaker.Instance.PlayerDisconnect(this, MatchID);
        this.networkMatchChecker.matchId = string.Empty.ToGuid();
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
