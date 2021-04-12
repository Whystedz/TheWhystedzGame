using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class LobbyCanvasController : MonoBehaviour
{
    public static LobbyCanvasController Instance { get; set; }
    public event Action<NetworkConnection> OnPlayerDisconnected;

    internal string localHostedMatchId = string.Empty;
    internal string localJoinedMatchId = string.Empty;
    internal string enteredMatchId = string.Empty;
    internal string displayName = string.Empty;

    [Header("GUI References")]
    [SerializeField] public GameObject lobbyView;
    [SerializeField] public GameObject roomView;
    [SerializeField] public RoomGUI roomGUI;

    private void Awake() => Instance = this;

    #region UI Functions

    internal void InitializeData()
    {
        this.localHostedMatchId = string.Empty;
        this.localJoinedMatchId = string.Empty;
        this.enteredMatchId = string.Empty;
        this.displayName = string.Empty;
    }

    // Called from OnStopServer and OnStopClient when shutting down
    public void ResetCanvas()
    {
        InitializeData();
        this.lobbyView.SetActive(false);
        this.roomView.SetActive(false);
        gameObject.SetActive(false);
    }

    public void SetDisplayName(string newName) => this.displayName = newName;

    #endregion

    #region Button Calls

    // Assigned in inspector to Host Public button
    public void RequestCreatePublicMatch()
    {
        if (!NetworkClient.active) return;

        NetworkClient.connection.Send(new ServerMatchMessage {ServerMatchOperation = ServerMatchOperation.CreatePublic, PlayerName = this.displayName});
        Debug.Log($"CreatePublicMatch message sent: playerName = {this.displayName}");
    }

    // Assigned in inspector to Host Private button
    public void RequestCreatePrivateMatch()
    {
        if (!NetworkClient.active) return;

        NetworkClient.connection.Send(new ServerMatchMessage {ServerMatchOperation = ServerMatchOperation.CreatePrivate, PlayerName = this.displayName});
    }

    // Assigned in inspector to Join button
    public void RequestJoinMatch()
    {
        if (!NetworkClient.active || this.enteredMatchId == string.Empty) return;

        NetworkClient.connection.Send(new ServerMatchMessage {ServerMatchOperation = ServerMatchOperation.Join, MatchId = this.enteredMatchId, PlayerName = this.displayName});
    }

    // Assigned in inspector to Search button
    public void RequestSearchMatch()
    {
        if (!NetworkClient.active) return;

        NetworkClient.connection.Send(new ServerMatchMessage {ServerMatchOperation = ServerMatchOperation.Search, PlayerName = this.displayName});
    }

    // Assigned in inspector to Leave button
    public void RequestLeaveMatch()
    {
        if (!NetworkClient.active || (this.localHostedMatchId == string.Empty && this.localJoinedMatchId == string.Empty)) return;

        string matchId;
        if (this.localJoinedMatchId == string.Empty)
            matchId = this.localHostedMatchId;
        else
            matchId = this.localJoinedMatchId;

        NetworkClient.connection.Send(new ServerMatchMessage {ServerMatchOperation = ServerMatchOperation.Leave, MatchId = matchId});
    }

    // Assigned in inspector to Cancel button
    public void RequestCancelMatch()
    {
        if (!NetworkClient.active || this.localHostedMatchId == string.Empty) return;

        string matchId;
        if (this.localJoinedMatchId == string.Empty)
            matchId = this.localHostedMatchId;
        else
            matchId = this.localJoinedMatchId;

        NetworkClient.connection.Send(new ServerMatchMessage {ServerMatchOperation = ServerMatchOperation.Cancel, MatchId = matchId});
    }

    // Assigned in inspector to Ready button
    public void RequestReadyChange()
    {
        if (!NetworkClient.active || (this.localHostedMatchId == string.Empty && this.localJoinedMatchId == string.Empty)) return;

        string matchId;
        if (this.localJoinedMatchId == string.Empty)
            matchId = this.localHostedMatchId;
        else
            matchId = this.localJoinedMatchId;

        NetworkClient.connection.Send(new ServerMatchMessage {ServerMatchOperation = ServerMatchOperation.Ready, MatchId = matchId});
        Debug.Log($"ReadyChange message sent: matchId = {matchId}");
    }

    // Assigned in inspector to Start button
    public void RequestStartMatch()
    {
        if (!NetworkClient.active || this.localHostedMatchId == string.Empty) return;

        NetworkClient.connection.Send(new ServerMatchMessage {ServerMatchOperation = ServerMatchOperation.Start});
    }

    public void OnMatchEnded()
    {
        if (!NetworkClient.active) return;
        
        roomGUI.ResetGUI();
        ShowRoomView();
    }

    public void SetEnteredMatchId(string code) => this.enteredMatchId = code;

    #endregion


    internal void OnStopClient() => ResetCanvas();

    #region Client Match Message Handler

    public void ShowLobbyView()
    {
        this.lobbyView.GetComponent<LobbyGUI>().EnableSearchCanvas(false);
        this.lobbyView.SetActive(true);
        Debug.Log("Lobbyview set active");
        this.roomView.SetActive(false);
    }

    public void ShowRoomView()
    {
        this.lobbyView.GetComponent<LobbyGUI>().EnableSearchCanvas(false);
        this.lobbyView.SetActive(false);
        this.roomView.SetActive(true);
    }

    #endregion

    public void EnterRoom(string matchId, PlayerInfo[] playerInfos, bool isHost)
    {
        ShowRoomView();
        if (isHost)
            this.localHostedMatchId = matchId;
        else
            this.localJoinedMatchId = matchId;

        this.roomGUI.SetRoomCode(matchId);
        this.roomGUI.RefreshRoomPlayers(playerInfos);
        this.roomGUI.SetHost(isHost);
    }
}
