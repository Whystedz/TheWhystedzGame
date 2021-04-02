using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkBehaviour
{
    public static LobbyPlayer LocalPlayer { get; set; }
    [SyncVar] public string MatchID;
    private NetworkMatchChecker networkMatchChecker;
    public bool IsHost { get; set; }

    [SyncVar(hook = nameof(OnDisplayNameChanged))] 
    public string DisplayName = "";
    [SyncVar(hook = nameof(OnReadyStatusChanged))]
    public bool IsReady;

    void Awake()
    {
        this.networkMatchChecker = GetComponent<NetworkMatchChecker>();
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
            LocalPlayer = this;

        ResetData();
    }

    public void ResetData()
    {
        MatchID = string.Empty;
        IsHost = false;
        IsReady = false;
    }

    public void OnDisplayNameChanged(string oldValue, string newValue) => UpdateLobbyUI();
    public void OnReadyStatusChanged(bool oldValue, bool newValue) => UpdateLobbyUI();

    public void UpdateLobbyUI()
    {
        if (MatchID != string.Empty)
            MatchMaker.Instance.UpdateLobbyUI(MatchID);
    }

    public void UpdateLocalUI(Match match)
    {
        UILobby.Instance.UpdateLobbyUI(match);
        //TargetUpdateLocalUI(match);
    }

    [TargetRpc]
    public void TargetUpdateLocalUI(Match match)
    {
        UILobby.Instance.UpdateLobbyUI(match);
    }

    [Command]
    public void CmdSetDisplayName(string displayName)
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
        Match match;
        if (MatchMaker.Instance.HostGame(matchID, this, publicMatch, out match))
        {
            MatchID = matchID;
            Debug.Log($"<color=green>Game hosted successfully</color>");
            this.networkMatchChecker.matchId = matchID.ToGuid();
            TargetHostGame(true, matchID, match);
        }
        else
        {
            Debug.Log($"<color=red>Game host failed</color>");
            TargetHostGame(false, matchID, match);
        }
    }

    [TargetRpc]
    void TargetHostGame(bool success, string matchID, Match match)
    {
        Debug.Log($"Match ID: {MatchID} == {matchID}");
        UILobby.Instance.HostSuccess(success, match);
    }

    //---- JOIN GAME LOGIC ----

    public void JoinGame(string _inputID)
    {
        CmdJoinGame(_inputID);
    }

    [Command]
    void CmdJoinGame(string matchID)
    {
        Match match;
        if (MatchMaker.Instance.JoinGame(matchID, this, out match))
        {
            MatchID = matchID;
            Debug.Log($"<color=green>Game joined successfully</color>");
            this.networkMatchChecker.matchId = matchID.ToGuid();
            TargetJoinGame(true, matchID, match);
        }
        else
        {
            Debug.Log($"<color=red>Game join failed</color>");
            TargetJoinGame(false, matchID, match);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success, string matchID, Match match)
    {
        MatchID = matchID;
        Debug.Log($"Match ID: {MatchID} == {matchID}");
        UILobby.Instance.JoinSuccess(success, match);
    }

    //---- SEARCH GAME LOGIC ----

    public void SearchGame()
    {
        CmdSearchGame();
    }

    [Command]
    public void CmdSearchGame()
    {
        Match match;
        if (MatchMaker.Instance.SearchGame(this, out match, out MatchID))
        {
            Debug.Log($"<color=green>Game found</color>");
            this.networkMatchChecker.matchId = MatchID.ToGuid();
            TargetSearchGame(true, MatchID, match);
        }
        else
        {
            Debug.Log($"<color=red>Game not found</color>");
            TargetSearchGame(false, MatchID, match);
        }
    }

    [TargetRpc]
    public void TargetSearchGame(bool success, string matchID, Match match)
    {
        MatchID = matchID;
        Debug.Log($"Match ID: {MatchID} == {matchID}");
        UpdateLobbyUI();
        UILobby.Instance.SearchSuccess(success, match);
    }

    //---- READY UP LOGIC ----

    public void ReadyUp()
    {
        CmdReadyUp();
    }

    [Command]
    void CmdReadyUp()
    {
        IsReady = !IsReady;

        //MatchMaker.Instance.NotifyPlayersOfReadyState(MatchID);
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!IsHost) { return; }

        //startGameButton.interactable = readyToStart;
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
        ResetData();
        RpcDisconnectGame();
    }

    [ClientRpc]
    void RpcDisconnectGame()
    {
        ClientDisconnect();
    }

    void ClientDisconnect()
    {

    }
}
