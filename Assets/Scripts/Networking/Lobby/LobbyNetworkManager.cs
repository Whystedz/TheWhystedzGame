using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class LobbyNetworkManager : NetworkManager
{
    // ****** Server only data ********
    // from player connection to matchId
    internal static readonly Dictionary<NetworkConnection, string> playerMatches = new Dictionary<NetworkConnection, string>();

    // from matchId to player connections
    internal static readonly Dictionary<string, HashSet<NetworkConnection>> matchConnections = new Dictionary<string, HashSet<NetworkConnection>>();

    // from matchId to matchInfo, for opened matches
    internal static readonly Dictionary<string, MatchInfo> openMatches = new Dictionary<string, MatchInfo>();

    internal static readonly Dictionary<string, MatchController> matchControllers = new Dictionary<string, MatchController>();

    // from player connection to player info
    internal static readonly Dictionary<NetworkConnection, PlayerInfo> playerInfos = new Dictionary<NetworkConnection, PlayerInfo>();

    // Network Connections that have neither started nor joined a match yet
    internal static readonly HashSet<NetworkConnection> waitingConnections = new HashSet<NetworkConnection>();

    // Match Controllers listen for this to terminate their match and clean up
    public event Action<NetworkConnection> OnPlayerDisconnected;

    [Header("Lobby GUI")]
    [SerializeField] private LobbyCanvasController canvasController;

    [Header("Scenes")]
    [Scene] public string MainScene;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> playerPrefabs;
    [SerializeField] private GameObject matchControllerPrefab;

    public override void Awake()
    {
        base.Awake();
        InitializeData();
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
        if (!NetworkServer.active) return;

        // Invoke OnPlayerDisconnected on all instances of MatchController
        OnPlayerDisconnected?.Invoke(connection);

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
            entry.Value.Remove(connection);

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
                    if (playerConnection != connection)
                        playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos});
            }
        }

        SendMatchList();

        base.OnServerDisconnect(connection);
    }

    public override void OnClientConnect(NetworkConnection connection)
    {
        base.OnClientConnect(connection);
        this.canvasController = FindObjectOfType<LobbyCanvasController>();
        this.canvasController.InitializeData();
        this.canvasController.ShowLobbyView();

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
            //this.lobbyCanvas.SetActive(true);

        InitializeData();
        //this.canvasController.InitializeData();

        NetworkServer.RegisterHandler<ServerMatchMessage>(OnServerMatchMessage);
    }

    public override void OnStartClient()
    {
        if (!NetworkClient.active) return;

        InitializeData();
        NetworkClient.RegisterHandler<ClientMatchMessage>(OnClientMatchMessage);
    }

    public override void OnStopServer()
    {
        this.canvasController.ResetCanvas();
        //this.lobbyCanvas.SetActive(false);
    }

    public override void OnStopClient()
    {
        this.canvasController.ResetCanvas();
        // TODO: Add a check, if in match, remove from combo manager list
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
            case ServerMatchOperation.SceneLoaded:
            {
                OnServerSceneLoaded(connection);
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
        openMatches.Add(newMatchId, new MatchInfo
        {
            MatchId = newMatchId,
            Players = 1,
            MaxPlayers = MatchMaker.MaxPlayers,
            InProgress = false,
            IsPublic = true
        });
        // set playerInfo
        var playerInfo = playerInfos[connection];
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
        openMatches.Add(newMatchId, new MatchInfo
        {
            MatchId = newMatchId,
            Players = 1,
            MaxPlayers = 8,
            InProgress = false,
            IsPublic = false
        });

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

    public void OnServerStartMatch(NetworkConnection connection)
    {
        if (!NetworkServer.active || !playerMatches.ContainsKey(connection)) return;

        string matchId;
        if (playerMatches.TryGetValue(connection, out matchId))
        {
            foreach (var playerConn in matchConnections[matchId])
                playerConn.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.Started});
                
            playerMatches.Remove(connection);
            openMatches.Remove(matchId);
            matchConnections.Remove(matchId);
            SendMatchList();
        }
    }

    // TODO: this will be invoked by a client message many times
    // need test in multiple client cases
    public void OnServerSceneLoaded(NetworkConnection connection)
    {
        if (!NetworkServer.active || !playerMatches.ContainsKey(connection)) return;

        string matchId;
        if (playerMatches.TryGetValue(connection, out matchId))
        {
            // spawn or get a matchController
            MatchController matchController;
            if (!matchControllers.ContainsKey(matchId))
            {
                GameObject matchControllerObject = Instantiate(this.matchControllerPrefab);
                matchControllerObject.GetComponent<NetworkMatchChecker>().matchId = openMatches[matchId].MatchId.ToGuid();
                NetworkServer.Spawn(matchControllerObject);
                matchController = matchControllerObject.GetComponent<MatchController>();
                matchControllers.Add(matchId, matchController);
            }
            else
            {
                matchController = matchControllers[matchId];
            }

            // spawn new prefabs; add to matchController
            var prefabIndex = playerInfos[connection].Team == Team.RedTeam ? 0 : 1;
            var player = Instantiate(playerPrefabs[prefabIndex]);
            player.GetComponent<NetworkMatchChecker>().matchId = openMatches[matchId].MatchId.ToGuid();
            NetworkServer.AddPlayerForConnection(connection, player);

            matchController.playerIdentities.Add(connection.identity);

            // Reset ready state back to false
            var playerInfo = playerInfos[connection];
            playerInfo.IsReady = false;
            playerInfos[connection] = playerInfo;
            
            OnPlayerDisconnected += matchController.OnPlayerDisconnected;
        }
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
        // set player team: odd is red, even is blue
        playerInfo.Team = matchInfo.Players % 2 == 1 ? Team.RedTeam : Team.BlueTeam;
        playerInfos[connection] = playerInfo;

        SendMatchList();

        // get info of every player and send back 
        PlayerInfo[] infos = matchConnections[matchId].Select(playerConnection => playerInfos[playerConnection]).ToArray();
        connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.Joined, MatchId = matchId, PlayerInfos = infos});

        // send new playerInfo to everyone in the match
        foreach (NetworkConnection playerConnection in matchConnections[matchId])
            playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos});
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
            entry.Value.Remove(connection);

        var connections = matchConnections[matchId];

        // if the player removed is host, assign a new host
        if (isHosting)
            SetNewHost(connections.First());

        // get info of every player and send back 
        var infos = connections.Select(playerConnection => playerInfos[playerConnection]).ToArray();
        foreach (var playerConnection in matchConnections[matchId])
            playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos});

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
        Debug.Log($"ReadyChange message received: matchId = {matchId}");
        if (!NetworkServer.active) return;

        // change player ready state
        var playerInfo = playerInfos[connection];
        playerInfo.IsReady = !playerInfo.IsReady;
        playerInfos[connection] = playerInfo;

        // send back playInfos to clients
        var connections = matchConnections[matchId];
        var infos = connections.Select(playerConnection => playerInfos[playerConnection]).ToArray();

        foreach (var playerConnection in matchConnections[matchId])
            playerConnection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos});
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
                    openMatches.Add(matchInfo.MatchId, matchInfo);

                break;
            }
            case ClientMatchOperation.Created:
            {
                this.canvasController.EnterRoom(message.MatchId, message.PlayerInfos, true);
                break;
            }
            case ClientMatchOperation.Cancelled:
            {
                this.canvasController.ShowLobbyView();
                break;
            }
            case ClientMatchOperation.Joined:
            {
                this.canvasController.EnterRoom(message.MatchId, message.PlayerInfos, false);
                break;
            }
            case ClientMatchOperation.Departed:
            {
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
                OnClientStartMatch();
                break;
            }
        }
    }

    private void OnClientStartMatch()
    {
        Debug.Log($"ClientStartMatch received");
        SceneManager.LoadScene(this.MainScene, LoadSceneMode.Additive);
        NetworkClient.connection.Send(new ServerMatchMessage {ServerMatchOperation = ServerMatchOperation.SceneLoaded});
    }

    #endregion

    // Sends updated match list to all waiting connections or just one if specified
    internal static void SendMatchList(NetworkConnection connection = null)
    {
        if (!NetworkServer.active) return;

        if (connection != null)
            connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.RefreshList, MatchInfos = openMatches.Values.ToArray()});
        else
            foreach (var waiter in waitingConnections)
                waiter.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.RefreshList, MatchInfos = openMatches.Values.ToArray()});
    }

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

    private static void SetNewHost(NetworkConnection connection)
    {
        var newHostInfo = playerInfos[connection];
        newHostInfo.IsHost = true;
        playerInfos[connection] = newHostInfo;
        connection.Send(new ClientMatchMessage {ClientMatchOperation = ClientMatchOperation.UpdateHost});
    }
}
