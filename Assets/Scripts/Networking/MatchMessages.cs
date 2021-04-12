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
    public PlayerInfo ThisPlayerInfo;
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
    public Team Team;
}

[Serializable]
public struct MatchPlayerData
{
    public string playerName;
    public int currentScore;
    public Team team;
}

[Serializable]
public struct TileInfo
{
    public float Progress;
    public byte XIndex;
    public byte ZIndex;
    public TileState TileState;
}

[Serializable]
public class ComboHintInfo
{
    public NetworkComboPlayer OriginPlayer;
    public NetworkComboPlayer TargetPlayer;
    public ComboType ComboType;
    public bool MoveTowards;
}

[Serializable]
public struct ComboInfo
{
    public List<TileInfo> Tiles;
    public List<NetworkComboPlayer> Players;
    public Vector3 Center;
    public ComboType ComboType;
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
    Ready,
    SceneLoaded
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

public enum Dummy : byte
{
    IgnoreThis,
    Again
}

public struct MatchLoadInfo
{
    public NetworkConnection Connection;
    public string MatchId;
}