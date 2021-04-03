using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct ServerMatchMessage : NetworkMessage
{
    public ServerMatchOperation ServerMatchOperation;
    public string MatchId;
    public string PlayerName;
}

public struct ClientMatchMessage : NetworkMessage
{
    public ClientMatchOperation ClientMatchOperation;
    public string MatchId;
    public MatchInfo[] MatchInfos;
    public PlayerInfo[] PlayerInfos;
}

[Serializable]
public struct MatchInfo
{
    public string MatchId;
    public byte Players;
    public byte MaxPlayers;
    public bool InProgress;
    public bool IsPublic;
}

[Serializable]
public struct PlayerInfo
{
    public string MatchId;
    public string DisplayName;
    public bool IsReady;
    public bool IsHost;
}

[Serializable]
public struct TileInfo
{
    public float TimeToRespawn;
    public float TimeOfBreakingAnimation;
    public float Progress;
    public int XIndex;
    public int ZIndex;
    public TileState TileState;
    public TileHighlightState TileHighlightState;
}

[Serializable]
public struct ComboHintInfo
{
    public NetworkComboPlayer OriginPlayer;
    public NetworkComboPlayer TargetPlayer;
    public ComboType ComboType;
    public bool MoveTowards;
}

[Serializable]
public struct ComboInfo
{
    public List<NetworkTile> Tiles;
    public List<NetworkComboPlayer> Players;
    public Vector3 Center;
    public ComboType ComboType;
    public NetworkComboPlayer InitiatingPlayer;
    public bool IsTriggered;
}

public enum ServerMatchOperation : byte
{
    None,
    CreatePublic,
    CreatePrivate,
    Cancel,
    Start,
    Join,
    Leave,
    Search,
    Ready
}

public enum ClientMatchOperation : byte
{
    None,
    RefreshList,
    Created,
    Cancelled,
    Joined,
    Departed,
    UpdateRoom,
    UpdateHost,
    Started
}

public enum ServerTileOperation : byte
{
    None,
    Generate,
    UpdateState
}