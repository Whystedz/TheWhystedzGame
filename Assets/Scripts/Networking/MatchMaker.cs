using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Security.Cryptography;
using System.Text;

[System.Serializable]
public class Match 
{
    public string MatchID;
    public bool IsPublicMatch;
    public bool InMatch;
    private int maxPlayers;
    public SyncListLobbyPlayer Players = new SyncListLobbyPlayer();

    public Match(string matchID, LobbyPlayer player, int maxPlayers)
    {
        this.MatchID = matchID;
        this.maxPlayers = maxPlayers;
        Players.Add(player);
    }

    public Match() {}

    public bool isMatchFull()
    {
        if(Players.Count == maxPlayers)
            return true;
        
        return false;
    }
}

[System.Serializable]
public class SyncListLobbyPlayer : SyncList<LobbyPlayer> { }

[System.Serializable]
public class SyncListMatch : SyncList<Match> { }

public class MatchMaker : NetworkBehaviour
{
    public static MatchMaker Instance { get; set; }
    public SyncListMatch Matches = new SyncListMatch();
    public SyncList<string> MatchIDs = new SyncList<string>();
    [SerializeField] GameObject gameManagerPrefab;
    [SerializeField] private int maxPlayers = 8;
    [SerializeField] private int minPlayers = 2;

    void Start() => Instance = this;

    public bool HostGame(string matchID, LobbyPlayer player, bool publicMatch, out Match match)
    {
        match = null;
        if (!MatchIDs.Contains(matchID))
        {
            MatchIDs.Add(matchID);
            Match newMatch = new Match(matchID, player, this.maxPlayers);
            newMatch.IsPublicMatch = publicMatch;
            Matches.Add(newMatch);
            Debug.Log($"Match generated");
            match = newMatch;
            return true;
        }
        else
        {
            Debug.Log($"Match ID already exists");
            return false;
        }
    }

    public bool JoinGame(string matchID, LobbyPlayer player, out Match match)
    {
        match = null;
        if (MatchIDs.Contains(matchID))
        {
            for (int i = 0; i < Matches.Count; i++)
            {
                if (Matches[i].MatchID == matchID)
                {
                    Matches[i].Players.Add(player);
                    match = Matches[i];
                    break;
                }
            }

            Debug.Log($"Match joined");
            return true;
        }
        else
        {
            Debug.Log($"Match ID does not exist");
            return false;
        }
    }

    public bool SearchGame(LobbyPlayer player, out Match match, out string matchID)
    {
        match = null;
        matchID = string.Empty;

        for(int i = 0; i < Matches.Count; i++)
        {
            if(Matches[i].IsPublicMatch && !Matches[i].isMatchFull() && !Matches[i].InMatch)
            {
                matchID = Matches[i].MatchID;
                if(JoinGame(matchID, player, out match))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void StartGame(string matchID)
    {
        GameObject newTurnManager = Instantiate(gameManagerPrefab);
        NetworkServer.Spawn(newTurnManager);
        newTurnManager.GetComponent<NetworkMatchChecker>().matchId = matchID.ToGuid();
        TurnManager turnManager = newTurnManager.GetComponent<TurnManager>();

        for(int i = 0; i < Matches.Count; i++)
        {
            if (Matches[i].MatchID == matchID)
            {
                foreach (var player in Matches[i].Players)
                {
                    LobbyPlayer lobbyPlayer = player.GetComponent<LobbyPlayer>();
                    turnManager.AddPlayer(lobbyPlayer);
                    lobbyPlayer.StartMatch();
                }
                break;
            }
        }
    }

    public static string GetRandomMatchID()
    {
        string id = string.Empty;

        for (int i = 0; i < 5; i++)
        {
            int random = UnityEngine.Random.Range(0, 36);
            if (random < 26) 
            {
                // Converts to capital letter
                id += (char)(random + 65);
            }
            else
            {
                id += (random - 26).ToString();
            }
        }

        return id;
    }

    public void PlayerDisconnect(LobbyPlayer player, string matchID)
    {
        for (int i = 0; i < Matches.Count; i++)
        {
            if(Matches[i].MatchID == matchID)
            {
                int playerIndex = Matches[i].Players.IndexOf(player);
                Matches[i].Players.RemoveAt(playerIndex);
                Debug.Log($"Player disconnected from match {matchID} | {Matches[i].Players.Count} players remainig");

                if (Matches[i].Players.Count == 0)
                {
                    Debug.Log($"No more players in match. Terminating {matchID}");
                    Matches.RemoveAt(i);
                    MatchIDs.Remove(matchID);
                }
                break;
            }
        }
    }

    public bool IsReadyToStart(Match match)
    {

        if(match.Players.Count < minPlayers)
            return false;

        foreach (var player in match.Players)
        {
            if(!player.IsReady)
                 return false;
        }

        return true;
    }

    public void NotifyPlayersOfReadyState(string matchID)
    {
        for(int i = 0; i < Matches.Count; i++)
        {
            if (Matches[i].MatchID == matchID)
            {
                foreach (var player in Matches[i].Players)
                {
                    player.HandleReadyToStart(IsReadyToStart(Matches[i]));
                }
            }
        }
    }

    public void UpdateLobbyUI(string matchID)
    {  
        Match currentRoom = null;
        for(int i = 0; i < Matches.Count; i++)
        {
            if (Matches[i].MatchID == matchID)
            {
                currentRoom = Matches[i];
                foreach (var player in currentRoom.Players)
                {
                    player.UpdateLocalUI(currentRoom);
                }
            }
        }
    }
}

public static class MatchExtensions 
{
    public static System.Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);

        return new System.Guid(hashBytes);
    }
}
