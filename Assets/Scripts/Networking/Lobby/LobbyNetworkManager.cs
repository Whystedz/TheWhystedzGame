using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class LobbyNetworkManager : NetworkManager
{
    // ****** Server only data ********
    // from player connection to matchId
    internal static readonly Dictionary<NetworkConnection, string> playerMatches = new Dictionary<NetworkConnection, string>();

    // from matchId to player connections
    internal static readonly Dictionary<string, HashSet<NetworkConnection>> matchConnections = new Dictionary<string, HashSet<NetworkConnection>>();

    // from matchId to matchInfo, for opened matches
    internal static readonly Dictionary<string, MatchInfo> openMatches = new Dictionary<string, MatchInfo>();

    // from player connection to player info
    internal static readonly Dictionary<NetworkConnection, PlayerInfo> playerInfos = new Dictionary<NetworkConnection, PlayerInfo>();

    // Network Connections that have neither started nor joined a match yet
    internal static readonly List<NetworkConnection> waitingConnections = new List<NetworkConnection>();

    // ******** Client only data ********
    internal string localHostedMatchId = string.Empty;
    internal string localJoinedMatchId = string.Empty;
    internal string enteredMatchId = string.Empty;
    internal string displayName = string.Empty;

    private readonly MatchMaker matchMaker = MatchMaker.Instance;

    [Header("Lobby GUI")]
    [SerializeField] private GameObject lobbyCanvas;

    [SerializeField] private CanvasController canvasController;

    [Header("Room Settings")]
    [SerializeField] private LobbyPlayer lobbyPlayerPrefab;

    [Scene] public string LobbyScene;

    [Scene] public string MainScene;


    public override void Awake()
    {
        base.Awake();
        InitializeData();
        this.canvasController.InitializeData();
    }

    public void InitializeData()
    {
        playerMatches.Clear();
        openMatches.Clear();
        matchConnections.Clear();
        waitingConnections.Clear();
    }

    #region Server & client callbacks

    public override void OnServerReady(NetworkConnection connection)
    {
        base.OnServerReady(connection);

        if (!NetworkServer.active) return;

        waitingConnections.Add(connection);
        playerInfos.Add(connection, new PlayerInfo());

        SendMatchList();
    }

    public override void OnServerDisconnect(NetworkConnection connection)
    {
        // canvasController.OnServerDisconnect(conn);
        if (!NetworkServer.active) return;

        // Invoke OnPlayerDisconnected on all instances of MatchController
        // comment this out because no feature uses this now
        // OnPlayerDisconnected?.Invoke(connection);

        // if player is a match host, remove player and match as well
        if (playerMatches.TryGetValue(connection, out var matchId))
        {
            playerMatches.Remove(connection);
            openMatches.Remove(matchId);

            foreach (var playerConnection in matchConnections[matchId])
            {
                var playerInfoL = playerInfos[playerConnection];
                ResetPlayerInfo(playerInfoL);
                playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.Departed});
            }
        }

        // remove player connection from every match
        foreach (var entry in matchConnections)
        {
            entry.Value.Remove(connection);
        }

        var playerInfo = playerInfos[connection];
        // update matchInfo and send to clients, if the player was in a match
        if (playerInfo.MatchId != string.Empty)
        {
            if (openMatches.TryGetValue(playerInfo.MatchId, out var matchInfo))
            {
                matchInfo.Players--;
                openMatches[playerInfo.MatchId] = matchInfo;
            }

            if (matchConnections.TryGetValue(playerInfo.MatchId, out var connections))
            {
                var infos = connections.Select(playerConnection => playerInfos[playerConnection]).ToArray();

                foreach (var playerConnection in matchConnections[playerInfo.MatchId])
                {
                    if (playerConnection != connection)
                    {
                        playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos});
                    }
                }
            }
        }

        SendMatchList();

        base.OnServerDisconnect(connection);
    }

    public override void OnClientConnect(NetworkConnection connection)
    {
        base.OnClientConnect(connection);
        // canvasController.OnClientConnect(connection);
        playerInfos.Add(connection, new PlayerInfo {IsReady = false, IsHost = false});
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        if (!NetworkClient.active) return;

        this.canvasController.InitializeData();
        base.OnClientDisconnect(conn);
    }

    public override void OnStartServer()
    {
        if (!NetworkServer.active) return;

        if (mode == NetworkManagerMode.ServerOnly)
            this.lobbyCanvas.SetActive(true);

        InitializeData();
        this.canvasController.InitializeData();

        NetworkServer.RegisterHandler<ServerMatchMessage>(OnServerMatchMessage);
    }

    public override void OnStartClient()
    {
        if (!NetworkClient.active) return;

        InitializeData();
        this.canvasController.InitializeData();
        this.lobbyCanvas.SetActive(true);
        this.canvasController.ShowLobbyView();
        NetworkClient.RegisterHandler<ClientMatchMessage>(OnClientMatchMessage);
    }

    public override void OnStopServer()
    {
        this.canvasController.ResetCanvas();
        this.lobbyCanvas.SetActive(false);
    }

    public override void OnStopClient()
    {
        this.canvasController.ResetCanvas();
    }

    #endregion

    #region Server message handlers

    private void OnServerMatchMessage(NetworkConnection connection, ServerMatchMessage message)
    {
        if (!NetworkServer.active) return;

        switch (message.ServerMatchOperation)
        {
            case ServerMatchOperation.None:
            {
                Debug.LogWarning("Missing ServerMatchOperation");
                break;
            }
            case ServerMatchOperation.CreatePublic:
            {
                OnServerCreatePublicMatch(connection, message.PlayerName);
                break;
            }
            case ServerMatchOperation.CreatePrivate:
            {
                OnServerCreatePrivateMatch(connection, message.PlayerName);
                break;
            }
            case ServerMatchOperation.Cancel:
            {
                OnServerCancelMatch(connection, message.MatchId);
                break;
            }
            case ServerMatchOperation.Start:
            {
                OnServerStartMatch(connection);
                break;
            }
            case ServerMatchOperation.Join:
            {
                OnServerJoinMatch(connection, message.MatchId, message.PlayerName);
                break;
            }
            case ServerMatchOperation.Leave:
            {
                OnServerLeaveMatch(connection, message.MatchId);
                break;
            }
            case ServerMatchOperation.Search:
            {
                OnServerSearchMatch(connection, message.PlayerName);
                break;
            }
            case ServerMatchOperation.Ready:
            {
                OnServerPlayerReady(connection, message.MatchId);
                break;
            }
        }
    }

    public void OnServerCreatePublicMatch(NetworkConnection connection, string playerName)
    {
        
        if (!NetworkServer.active || playerMatches.ContainsKey(connection)) return;

        Debug.Log("CreatePublicMatch message received");
        var newMatchId = MatchMaker.GetRandomMatchID();
        // create new match
        matchConnections.Add(newMatchId, new HashSet<NetworkConnection>());
        // add the player to the match
        matchConnections[newMatchId].Add(connection);
        // add match to the player
        playerMatches.Add(connection, newMatchId);
        // bind matchId with matchInfo
        openMatches.Add(newMatchId, new MatchInfo {MatchId = newMatchId, Players = 1, MaxPlayers = this.matchMaker.MaxPlayers, InProgress = false, IsPublic = true});
        // set playerInfo
        PlayerInfo playerInfo = playerInfos[connection];
        playerInfo.IsReady = false;
        playerInfo.IsHost = true;
        playerInfo.MatchId = newMatchId;
        playerInfo.DisplayName = playerName;
        playerInfo.Team = Team.RedTeam; // host is in team red by default
        playerInfos[connection] = playerInfo;

        // get playerInfos of everyone in this match 
        PlayerInfo[] infos = matchConnections[newMatchId].Select(playerConnection => playerInfos[playerConnection]).ToArray();

        connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.Created, MatchId = newMatchId, PlayerInfos = infos});

        SendMatchList();
    }

    public void OnServerCreatePrivateMatch(NetworkConnection connection, string playerName)
    {
        if (!NetworkServer.active || playerMatches.ContainsKey(connection)) return;

        var newMatchId = MatchMaker.GetRandomMatchID();
        // create new match
        matchConnections.Add(newMatchId, new HashSet<NetworkConnection>());
        // add player to match
        matchConnections[newMatchId].Add(connection);
        // add match to player
        playerMatches.Add(connection, newMatchId);
        // bind matchId with matchInfo
        openMatches.Add(newMatchId, new MatchInfo {MatchId = newMatchId, Players = 1, MaxPlayers = 8, InProgress = false, IsPublic = false});

        // set playerInfo
        PlayerInfo playerInfo = playerInfos[connection];
        playerInfo.IsReady = false;
        playerInfo.IsHost = true;
        playerInfo.MatchId = newMatchId;
        playerInfo.DisplayName = playerName;
        playerInfo.Team = Team.RedTeam; // host is in team red by default
        playerInfos[connection] = playerInfo;

        // get playerInfos of everyone in this match 
        PlayerInfo[] infos = matchConnections[newMatchId].Select(playerConnection => playerInfos[playerConnection]).ToArray();

        // send match and player data back to client
        connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.Created, MatchId = newMatchId, PlayerInfos = infos});

        SendMatchList();
    }

    public void OnServerCancelMatch(NetworkConnection connection, string matchId)
    {
        if (!NetworkServer.active || !playerMatches.ContainsKey(connection)) return;

        // respond to client
        connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.Cancelled});

        // remove match if exists
        if (playerMatches.ContainsKey(connection))
        {
            // remove match host
            playerMatches.Remove(connection);
            // remove match
            openMatches.Remove(matchId);

            foreach (var playerConnection in matchConnections[matchId])
            {
                // update playerInfo in the match
                playerInfos[playerConnection] = ResetPlayerInfo(playerInfos[playerConnection]);
                // tell client player to depart the match
                playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.Departed});
            }

            SendMatchList();
        }
    }

    // TODO
    public void OnServerStartMatch(NetworkConnection connection)
    {
        throw new NotImplementedException();
    }

    public void OnServerJoinMatch(NetworkConnection connection, string matchId, string playerName)
    {
        if (!NetworkServer.active || !matchConnections.ContainsKey(matchId) || !openMatches.ContainsKey(matchId)) return;

        var matchInfo = openMatches[matchId];
        matchInfo.Players++;
        if (matchInfo.Players == matchInfo.MaxPlayers)
            // close the match if full
            openMatches.Remove(matchId);
        else
            openMatches[matchId] = matchInfo;
        // add player connection to the match
        matchConnections[matchId].Add(connection);

        // set playerInfo
        PlayerInfo playerInfo = playerInfos[connection];
        playerInfo.DisplayName = playerName;
        playerInfo.IsReady = false;
        playerInfo.IsHost = false;
        playerInfo.MatchId = matchId;
        // set player team: red by default; if red is full, then blue
        playerInfo.Team = matchInfo.Players <= this.matchMaker.MaxPlayers ? Team.RedTeam : Team.BlueTeam;
        playerInfos[connection] = playerInfo;

        SendMatchList();

        // get info of every player and send back 
        PlayerInfo[] infos = matchConnections[matchId].Select(playerConnection => playerInfos[playerConnection]).ToArray();
        connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.Joined, MatchId = matchId, PlayerInfos = infos});

        // send new playerInfo to everyone in the match
        foreach (NetworkConnection playerConnection in matchConnections[matchId])
        {
            playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos});
        }
    }

    public void OnServerLeaveMatch(NetworkConnection connection, string matchId)
    {
        if (!NetworkServer.active) return;

        MatchInfo matchInfo = openMatches[matchId];
        matchInfo.Players--;

        if (matchInfo.Players == 0)
        {
            // if no player in the match, cancel match
            OnServerCancelMatch(connection, matchId);
            return;
        }

        // update this matchInfo
        openMatches[matchId] = matchInfo;

        // init this playerInfo
        PlayerInfo playerInfo = playerInfos[connection];
        var isHosting = playerInfo.IsHost;
        playerInfos[connection] = ResetPlayerInfo(playerInfo);

        // remove player from all matches
        foreach (var entry in matchConnections)
        {
            entry.Value.Remove(connection);
        }

        var connections = matchConnections[matchId];

        // if the player removed is host, assign a new host
        if (isHosting)
            SetNewHost(connections.First());

        // get info of every player and send back 
        var infos = connections.Select(playerConnection => playerInfos[playerConnection]).ToArray();
        foreach (var playerConnection in matchConnections[matchId])
        {
            playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos});
        }

        // tell the player to depart
        connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.Departed});

        SendMatchList();
    }

    public void OnServerSearchMatch(NetworkConnection connection, string playerName)
    {
        if (!NetworkServer.active) return;

        // automatically join the first open match when searching
        foreach (var entry in openMatches)
        {
            var matchInfo = entry.Value;
            if (matchInfo.IsPublic)
            {
                OnServerJoinMatch(connection, matchInfo.MatchId, playerName);
                break;
            }
        }
    }

    public void OnServerPlayerReady(NetworkConnection connection, string matchId)
    {
        if (!NetworkServer.active) return;

        // change player ready state
        var playerInfo = playerInfos[connection];
        playerInfo.IsReady = !playerInfo.IsReady;
        playerInfos[connection] = playerInfo;

        // send back playInfos to clients
        var connections = matchConnections[matchId];
        var infos = connections.Select(playerConnection => playerInfos[playerConnection]).ToArray();

        foreach (var playerConnection in matchConnections[matchId])
        {
            playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos});
        }
    }

    #endregion

    #region Client message handlers

    private void OnClientMatchMessage(NetworkConnection connection, ClientMatchMessage message)
    {
        if (!NetworkClient.active) return;

        switch (message.ClientMatchOperation)
        {
            case ClientMatchOperation.None:
            {
                Debug.LogWarning("Missing ClientMatchOperation");
                break;
            }
            case ClientMatchOperation.RefreshList:
            {
                // refresh matchInfos
                openMatches.Clear();
                foreach (var matchInfo in message.MatchInfos)
                {
                    openMatches.Add(matchInfo.MatchId, matchInfo);
                }

                break;
            }
            case ClientMatchOperation.Created:
            {
                this.localHostedMatchId = message.MatchId;
                LobbyPlayer.LocalPlayer.SetMatchIdGuid(message.MatchId);
                this.canvasController.EnterRoom(message.MatchId, message.PlayerInfos, true);
                break;
            }
            case ClientMatchOperation.Cancelled:
            {
                ResetLocalMatchInfo();
                this.canvasController.ShowLobbyView();
                break;
            }
            case ClientMatchOperation.Joined:
            {
                this.localJoinedMatchId = message.MatchId;
                LobbyPlayer.LocalPlayer.SetMatchIdGuid(message.MatchId);
                this.canvasController.EnterRoom(message.MatchId, message.PlayerInfos, false);
                break;
            }
            case ClientMatchOperation.Departed:
            {
                ResetLocalMatchInfo();
                this.canvasController.ShowLobbyView();
                break;
            }
            case ClientMatchOperation.UpdateRoom:
            {
                this.canvasController.roomGUI.RefreshRoomPlayers(message.PlayerInfos);
                break;
            }
            case ClientMatchOperation.UpdateHost:
            {
                this.canvasController.roomGUI.SetHost(true);
                break;
            }
            case ClientMatchOperation.Started:
            {
                this.canvasController.lobbyView.SetActive(false);
                this.canvasController.roomView.SetActive(false);
                break;
            }
        }
    }

    #endregion

    /// <summary>
    /// Sends updated match list to all waiting connections or just one if specified
    /// </summary>
    /// <param name="connection">the specified connection</param>
    private static void SendMatchList(NetworkConnection connection = null)
    {
        if (!NetworkServer.active) return;

        if (connection != null)
        {
            connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.RefreshList, MatchInfos = openMatches.Values.ToArray()});
        }
        else
        {
            foreach (var waiter in waitingConnections)
            {
                waiter.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.RefreshList, MatchInfos = openMatches.Values.ToArray()});
            }
        }
    }


    /// <summary>
    /// Initialize a playerInfo
    /// </summary>
    /// <param name="playerInfo">The playerInfo to be initialized</param>
    /// <returns></returns>
    private static PlayerInfo ResetPlayerInfo(PlayerInfo playerInfo)
    {
        var newInfo = playerInfo;
        newInfo.DisplayName = string.Empty;
        newInfo.IsReady = false;
        newInfo.IsHost = false;
        newInfo.MatchId = string.Empty;
        newInfo.Team = Team.RedTeam;

        return newInfo;
    }

    /// <summary>
    /// Set the player of the connection as the new host of a match
    /// </summary>
    /// <param name="connection">the player connection</param>
    private static void SetNewHost(NetworkConnection connection)
    {
        var newHostInfo = playerInfos[connection];
        newHostInfo.IsHost = true;
        playerInfos[connection] = newHostInfo;
        connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateHost});
    }

    /// <summary>
    /// Initialize local matchInfo, called when client quit/cancel.
    /// </summary>
    private void ResetLocalMatchInfo()
    {
        this.localHostedMatchId = string.Empty;
        this.localJoinedMatchId = string.Empty;
        this.enteredMatchId = string.Empty;
        LobbyPlayer.LocalPlayer.SetMatchIdGuid(string.Empty);
    }
}