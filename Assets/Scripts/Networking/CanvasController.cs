using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CanvasController : MonoBehaviour
{
    public event Action<NetworkConnection> OnPlayerDisconnected;

    // Cross-reference of client that created the corresponding match in openMatches below
    internal static readonly Dictionary<NetworkConnection, string> playerMatches = new Dictionary<NetworkConnection, string>();
    internal static readonly Dictionary<string, MatchInfo> openMatches = new Dictionary<string, MatchInfo>();
    // Network Connections of all players in a match
    internal static readonly Dictionary<string, HashSet<NetworkConnection>> matchConnections = new Dictionary<string, HashSet<NetworkConnection>>();
    internal static readonly Dictionary<NetworkConnection, PlayerInfo> playerInfos = new Dictionary<NetworkConnection, PlayerInfo>();
    // Network Connections that have neither started nor joined a match yet
    internal static readonly List<NetworkConnection> waitingConnections = new List<NetworkConnection>();

    internal string localHostedMatchId = string.Empty;
    internal string localJoinedMatchId = string.Empty;
    internal string enteredMatchId = string.Empty;

    string displayName = string.Empty;

    [Header("GUI References")]
    [SerializeField] private GameObject matchList;
    [SerializeField] private GameObject matchPrefab;
    [SerializeField] private GameObject matchControllerPrefab;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private GameObject lobbyView;
    [SerializeField] private GameObject roomView;
    //[SerializeField] private RoomGUI roomGUI;
    [SerializeField] private ToggleGroup toggleGroup;

    #region UI Functions

    // Called from several places to ensure a clean reset
    //  - MatchNetworkManager.Awake
    //  - OnStartServer
    //  - OnStartClient
    //  - OnClientDisconnect
    //  - ResetCanvas
    internal void InitializeData()
    {
        playerMatches.Clear();
        openMatches.Clear();
        matchConnections.Clear();
        waitingConnections.Clear();
        this.localHostedMatchId = string.Empty;
        this.localJoinedMatchId = string.Empty;
    }

    // Called from OnStopServer and OnStopClient when shutting down
    void ResetCanvas()
    {
        InitializeData();
        this.lobbyView.SetActive(false);
        this.roomView.SetActive(false);
        this.gameObject.SetActive(false);
    }

    #endregion

    #region Button Calls

    // Assigned in inspector to Host Public button
    public void RequestCreatePublicMatch()
    {
        if (!NetworkClient.active) return;

        NetworkClient.connection.Send(new ServerMatchMessage { ServerMatchOperation = ServerMatchOperation.CreatePublic, PlayerName = this.displayName });
    }

    // Assigned in inspector to Host Private button
    public void RequestCreatePrivateMatch()
    {
        if (!NetworkClient.active) return;

        NetworkClient.connection.Send(new ServerMatchMessage { ServerMatchOperation = ServerMatchOperation.CreatePrivate, PlayerName = this.displayName });
    }

    // Assigned in inspector to Join button
    public void RequestJoinMatch()
    {
        if (!NetworkClient.active || this.enteredMatchId == string.Empty) return;

        NetworkClient.connection.Send(new ServerMatchMessage { ServerMatchOperation = ServerMatchOperation.Join, MatchId = this.enteredMatchId, PlayerName = this.displayName });
    }

    // Assigned in inspector to Leave button
    public void RequestLeaveMatch()
    {
        if (!NetworkClient.active || this.localJoinedMatchId == string.Empty) return;

        NetworkClient.connection.Send(new ServerMatchMessage { ServerMatchOperation = ServerMatchOperation.Leave, MatchId = this.localJoinedMatchId });
    }

    // Assigned in inspector to Cancel button
    public void RequestCancelMatch()
    {
        if (!NetworkClient.active || this.localHostedMatchId == string.Empty) return;

        NetworkClient.connection.Send(new ServerMatchMessage { ServerMatchOperation = ServerMatchOperation.Cancel });
    }

    // Assigned in inspector to Ready button
    public void RequestReadyChange()
    {
        if (!NetworkClient.active || (this.localHostedMatchId == string.Empty && this.localJoinedMatchId == string.Empty)) return;

        string matchId = this.localJoinedMatchId == string.Empty ? this.localJoinedMatchId : this.localHostedMatchId;

        NetworkClient.connection.Send(new ServerMatchMessage { ServerMatchOperation = ServerMatchOperation.Ready, MatchId = matchId });
    }

    // Assigned in inspector to Start button
    public void RequestStartMatch()
    {
        if (!NetworkClient.active || this.localHostedMatchId == string.Empty) return;

        NetworkClient.connection.Send(new ServerMatchMessage { ServerMatchOperation = ServerMatchOperation.Start });
    }

    // TODO: Modify this
    public void OnMatchEnded()
    {
        if (!NetworkClient.active) return;

        this.localHostedMatchId = string.Empty;
        this.localJoinedMatchId = string.Empty;
        ShowLobbyView();
    }

    // Sends updated match list to all waiting connections or just one if specified
    internal void SendMatchList(NetworkConnection connection = null)
    {
        if (!NetworkServer.active) return;

        if (connection != null)
        {
            connection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.RefreshList, MatchInfos = openMatches.Values.ToArray() });
        }
        else
        {
            foreach (var waiter in waitingConnections)
            {
                waiter.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.RefreshList, MatchInfos = openMatches.Values.ToArray() });
            }
        }
    }

    #endregion

    #region Server & Client Callbacks

    // Methods in this section are called from MatchNetworkManager's corresponding methods

    internal void OnStartServer()
    {
        if (!NetworkServer.active) return;

        InitializeData();
        NetworkServer.RegisterHandler<ServerMatchMessage>(OnServerMatchMessage);
    }

    internal void OnServerReady(NetworkConnection connection)
    {
        if (!NetworkServer.active) return;

        waitingConnections.Add(connection);
        playerInfos.Add(connection, new PlayerInfo { DisplayName = this.displayName, IsReady = false, IsHost = false});

        SendMatchList();
    }

    internal void OnServerDisconnect(NetworkConnection connection)
    {
        if (!NetworkServer.active) return;

        // Invoke OnPlayerDisconnected on all instances of MatchController
        OnPlayerDisconnected?.Invoke(connection);

        string matchId;
        if (playerMatches.TryGetValue(connection, out matchId))
        {
            playerMatches.Remove(connection);
            openMatches.Remove(matchId);

            foreach (NetworkConnection playerConnection in matchConnections[matchId])
            {
                PlayerInfo _playerInfo = playerInfos[playerConnection];
                _playerInfo.IsReady = false;
                _playerInfo.MatchId = string.Empty;
                playerInfos[playerConnection] = _playerInfo;
                playerConnection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.Departed });
            }
        }

        foreach (KeyValuePair<string, HashSet<NetworkConnection>> element in matchConnections)
        {
            element.Value.Remove(connection);
        }

        PlayerInfo playerInfo = playerInfos[connection];
        if (playerInfo.MatchId != string.Empty)
        {
            MatchInfo matchInfo;
            if (openMatches.TryGetValue(playerInfo.MatchId, out matchInfo))
            {
                matchInfo.Players--;
                openMatches[playerInfo.MatchId] = matchInfo;
            }

            HashSet<NetworkConnection> connections;
            if (matchConnections.TryGetValue(playerInfo.MatchId, out connections))
            {
                PlayerInfo[] infos = connections.Select(playerConnection => playerInfos[playerConnection]).ToArray();

                foreach (NetworkConnection playerConnection in matchConnections[playerInfo.MatchId])
                {
                    if (playerConnection != connection)
                    {
                        playerConnection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos });
                    }
                }
            }
        }

        SendMatchList();
    }

    internal void OnStopServer()
    {
        ResetCanvas();
    }

    internal void OnClientConnect(NetworkConnection connection)
    {
        playerInfos.Add(connection, new PlayerInfo { DisplayName = this.displayName, IsReady = false, IsHost = false });
    }

    // TODO: Fix this
    internal void OnStartClient()
    {
        if (!NetworkClient.active) return;

        InitializeData();
        ShowLobbyView();
        createButton.gameObject.SetActive(true);
        joinButton.gameObject.SetActive(true);
        NetworkClient.RegisterHandler<ClientMatchMessage>(OnClientMatchMessage);
    }

    internal void OnClientDisconnect()
    {
        if (!NetworkClient.active) return;

        InitializeData();
    }

    internal void OnStopClient()
    {
        ResetCanvas();
    }

    #endregion

    #region Server Match Message Handlers

    void OnServerMatchMessage(NetworkConnection connection, ServerMatchMessage message)
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
                    OnServerCancelMatch(connection);
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
            case ServerMatchOperation.Ready:
                {
                    OnServerPlayerReady(connection, message.MatchId);
                    break;
                }
        }
    }

    void OnServerPlayerReady(NetworkConnection connection, string matchId)
    {
        if (!NetworkServer.active) return;

        PlayerInfo playerInfo = playerInfos[connection];
        playerInfo.IsReady = !playerInfo.IsReady;
        playerInfos[connection] = playerInfo;

        HashSet<NetworkConnection> connections = matchConnections[matchId];
        PlayerInfo[] infos = connections.Select(playerConnection => playerInfos[playerConnection]).ToArray();

        foreach (NetworkConnection playerConnection in matchConnections[matchId])
        {
            playerConnection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos });
        }
    }

    void OnServerLeaveMatch(NetworkConnection connection, string matchId)
    {
        if (!NetworkServer.active) return;

        MatchInfo matchInfo = openMatches[matchId];
        matchInfo.Players--;
        if (matchInfo.Players == 0)
            openMatches.Remove(matchId);
        else
            openMatches[matchId] = matchInfo;

        PlayerInfo playerInfo = playerInfos[connection];
        playerInfo.IsReady = false;
        playerInfo.IsHost = false;
        playerInfo.MatchId = string.Empty;
        playerInfos[connection] = playerInfo;

        foreach (KeyValuePair<string, HashSet<NetworkConnection>> element in matchConnections)
        {
            element.Value.Remove(connection);
        }

        HashSet<NetworkConnection> connections = matchConnections[matchId];
        PlayerInfo[] infos = connections.Select(playerConnection => playerInfos[playerConnection]).ToArray();

        foreach (NetworkConnection playerConnection in matchConnections[matchId])
        {
            playerConnection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos });
        }

        SendMatchList();

        connection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.Departed });
    }

    void OnServerCreatePublicMatch(NetworkConnection connection, string playerName)
    {
        if (!NetworkServer.active || playerMatches.ContainsKey(connection)) return;

        string newMatchId = MatchMaker.GetRandomMatchID();
        matchConnections.Add(newMatchId, new HashSet<NetworkConnection>());
        matchConnections[newMatchId].Add(connection);
        playerMatches.Add(connection, newMatchId);
        // TODO: Change to matchmaker's parameters
        openMatches.Add(newMatchId, new MatchInfo { MatchId = newMatchId, Players = 1, MaxPlayers = 8, InProgress = false, IsPublic = true });

        PlayerInfo playerInfo = playerInfos[connection];
        playerInfo.IsReady = false;
        playerInfo.IsHost = true;
        playerInfo.MatchId = newMatchId;
        playerInfo.DisplayName = playerName;
        playerInfos[connection] = playerInfo;

        PlayerInfo[] infos = matchConnections[newMatchId].Select(playerConnection => playerInfos[playerConnection]).ToArray();

        connection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.Created, MatchId = newMatchId, PlayerInfos = infos });

        SendMatchList();
    }

    void OnServerCreatePrivateMatch(NetworkConnection connection, string playerName)
    {
        if (!NetworkServer.active || playerMatches.ContainsKey(connection)) return;

        string newMatchId = MatchMaker.GetRandomMatchID();
        matchConnections.Add(newMatchId, new HashSet<NetworkConnection>());
        matchConnections[newMatchId].Add(connection);
        playerMatches.Add(connection, newMatchId);
        // TODO: Change to matchmaker's parameters
        openMatches.Add(newMatchId, new MatchInfo { MatchId = newMatchId, Players = 1, MaxPlayers = 8, InProgress = false, IsPublic = false });

        PlayerInfo playerInfo = playerInfos[connection];
        playerInfo.IsReady = false;
        playerInfo.IsHost = true;
        playerInfo.MatchId = newMatchId;
        playerInfos[connection] = playerInfo;

        PlayerInfo[] infos = matchConnections[newMatchId].Select(playerConnection => playerInfos[playerConnection]).ToArray();

        connection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.Created, MatchId = newMatchId, PlayerInfos = infos });

        SendMatchList();
    }

    void OnServerCancelMatch(NetworkConnection connection)
    {
        if (!NetworkServer.active || !playerMatches.ContainsKey(connection)) return;

        connection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.Cancelled });

        string matchId;
        if (playerMatches.TryGetValue(connection, out matchId))
        {
            playerMatches.Remove(connection);
            openMatches.Remove(matchId);

            foreach (NetworkConnection playerConnection in matchConnections[matchId])
            {
                PlayerInfo playerInfo = playerInfos[playerConnection];
                playerInfo.IsReady = false;
                playerInfo.MatchId = string.Empty;
                playerInfos[playerConnection] = playerInfo;
                playerConnection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.Departed });
            }

            SendMatchList();
        }
    }

    // TODO: this.
    void OnServerStartMatch(NetworkConnection connection)
    {
        /*
        if (!NetworkServer.active || !playerMatches.ContainsKey(connection)) return;

        string matchId;
        if (playerMatches.TryGetValue(connection, out matchId))
        {
            GameObject matchControllerObject = Instantiate(matchControllerPrefab);
            matchControllerObject.GetComponent<NetworkMatchChecker>().matchId = matchId;
            NetworkServer.Spawn(matchControllerObject);

            MatchController matchController = matchControllerObject.GetComponent<MatchController>();

            foreach (NetworkConnection playerConn in matchConnections[matchId])
            {
                playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Started });

                GameObject player = Instantiate(NetworkManager.singleton.playerPrefab);
                player.GetComponent<NetworkMatchChecker>().matchId = matchId;
                NetworkServer.AddPlayerForConnection(playerConn, player);

                if (matchController.player1 == null)
                {
                    matchController.player1 = playerConn.identity;
                }
                else
                {
                    matchController.player2 = playerConn.identity;
                }

                // Reset ready state for after the match.
                PlayerInfo playerInfo = playerInfos[playerConn];
                playerInfo.ready = false;
                playerInfos[playerConn] = playerInfo;
            }

            matchController.startingPlayer = matchController.player1;
            matchController.currentPlayer = matchController.player1;

            playerMatches.Remove(conn);
            openMatches.Remove(matchId);
            matchConnections.Remove(matchId);
            SendMatchList();

            OnPlayerDisconnected += matchController.OnPlayerDisconnected;
        }
        */
    }

    void OnServerJoinMatch(NetworkConnection connection, string matchId, string playerName)
    {
        if (!NetworkServer.active || !matchConnections.ContainsKey(matchId) || !openMatches.ContainsKey(matchId)) return;

        MatchInfo matchInfo = openMatches[matchId];
        matchInfo.Players++;
        if (matchInfo.Players == matchInfo.MaxPlayers)
            openMatches.Remove(matchId);
        else
            openMatches[matchId] = matchInfo;
        matchConnections[matchId].Add(connection);

        PlayerInfo playerInfo = playerInfos[connection];
        playerInfo.DisplayName = playerName;
        playerInfo.IsReady = false;
        playerInfo.IsHost = false;
        playerInfo.MatchId = matchId;
        playerInfos[connection] = playerInfo;

        PlayerInfo[] infos = matchConnections[matchId].Select(playerConnection => playerInfos[playerConnection]).ToArray();
        SendMatchList();

        connection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.Joined, MatchId = matchId, PlayerInfos = infos });

        foreach (NetworkConnection playerConnection in matchConnections[matchId])
        {
            playerConnection.Send(new ClientMatchMessage { ClientMatchOperation = ClientMatchOperation.UpdateRoom, PlayerInfos = infos });
        }
    }

    #endregion

    #region Client Match Message Handler

    void OnClientMatchMessage(NetworkConnection connection, ClientMatchMessage message)
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
                    openMatches.Clear();
                    foreach (MatchInfo matchInfo in message.MatchInfos)
                    {
                        openMatches.Add(matchInfo.MatchId, matchInfo);
                    }
                    break;
                }
            case ClientMatchOperation.Created:
                {
                    this.localHostedMatchId = message.MatchId;
                    ShowRoomView();
                    //roomGUI.RefreshRoomPlayers(message.PlayerInfos);
                    //roomGUI.SetOwner(true);
                    break;
                }
            case ClientMatchOperation.Cancelled:
                {
                    this.localHostedMatchId = string.Empty;
                    ShowLobbyView();
                    break;
                }
            case ClientMatchOperation.Joined:
                {
                    this.localJoinedMatchId = message.MatchId;
                    ShowRoomView();
                    //roomGUI.RefreshRoomPlayers(message.playerInfos);
                    //roomGUI.SetOwner(false);
                    break;
                }
            case ClientMatchOperation.Departed:
                {
                    this.localJoinedMatchId = string.Empty;
                    ShowLobbyView();
                    break;
                }
            case ClientMatchOperation.UpdateRoom:
                {
                    //roomGUI.RefreshRoomPlayers(message.PlayerInfos);
                    break;
                }
            case ClientMatchOperation.Started:
                {
                    lobbyView.SetActive(false);
                    roomView.SetActive(false);
                    break;
                }
        }
    }

    // TODO: this.
    void ShowLobbyView()
    {
        /*
        lobbyView.SetActive(true);
        roomView.SetActive(false);

        foreach (Transform child in matchList.transform)
        {
            if (child.gameObject.GetComponent<MatchGUI>().GetMatchId() == enteredMatchId)
            {
                Toggle toggle = child.gameObject.GetComponent<Toggle>();
                toggle.isOn = true;
            }
        }
        */
    }

    void ShowRoomView()
    {
        lobbyView.SetActive(false);
        roomView.SetActive(true);
    }

    #endregion
}
