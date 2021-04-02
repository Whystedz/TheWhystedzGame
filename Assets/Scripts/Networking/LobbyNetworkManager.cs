using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbyNetworkManager : NetworkManager
{
    [Header("Lobby GUI")]
    [SerializeField] private GameObject lobbyCanvas;
    [SerializeField] private CanvasController canvasController;

    public override void Awake()
    {
        base.Awake();
        canvasController.InitializeData();
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        canvasController.OnServerReady(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        canvasController.OnServerDisconnect(conn);
        base.OnServerDisconnect(conn);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        canvasController.OnClientConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        canvasController.OnClientDisconnect();
        base.OnClientDisconnect(conn);
    }

    public override void OnStartServer()
    {
        if (mode == NetworkManagerMode.ServerOnly)
            lobbyCanvas.SetActive(true);

        canvasController.OnStartServer();
    }

    public override void OnStartClient()
    {
        lobbyCanvas.SetActive(true);
        canvasController.OnStartClient();
    }

    public override void OnStopServer()
    {
        canvasController.OnStopServer();
        lobbyCanvas.SetActive(false);
    }

    public override void OnStopClient()
    {
        canvasController.OnStopClient();
    }
}
