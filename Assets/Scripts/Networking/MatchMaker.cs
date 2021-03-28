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
    private int maxPlayers = 8;
    public SyncListGameObject Players = new SyncListGameObject();

    public Match(string matchID, GameObject player)
    {
        this.MatchID = matchID;
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
public class SyncListGameObject : SyncList<GameObject> { }

[System.Serializable]
public class SyncListMatch : SyncList<Match> { }

public class MatchMaker : NetworkBehaviour
{
    public static MatchMaker Instance { get; set; }
    public SyncListMatch Matches = new SyncListMatch();
    public SyncList<string> MatchIDs = new SyncList<string>();
    [SerializeField] GameObject gameManagerPrefab;

    void Start() => Instance = this;

    public bool HostGame(string matchID, GameObject player, bool publicMatch, out int playerIndex)
    {
        playerIndex = -1;
        if (!MatchIDs.Contains(matchID))
        {
            MatchIDs.Add(matchID);
            Match match = new Match(matchID, player);
            match.IsPublicMatch = publicMatch;
            Matches.Add(match);
            Debug.Log($"Match generated");
            playerIndex = 1;
            return true;
        }
        else
        {
            Debug.Log($"Match ID already exists");
            return false;
        }
    }

    public bool JoinGame(string matchID, GameObject player, out int playerIndex)
    {
        playerIndex = -1;
        if (MatchIDs.Contains(matchID))
        {
            for (int i = 0; i < Matches.Count; i++)
            {
                if (Matches[i].MatchID == matchID)
                {
                    Matches[i].Players.Add(player);
                    playerIndex = Matches[i].Players.Count;
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

    public bool SearchGame(GameObject player, out int playerIndex, out string matchID)
    {
        playerIndex = -1;
        matchID = string.Empty;

        for(int i = 0; i < Matches.Count; i++)
        {
            if(Matches[i].IsPublicMatch && !Matches[i].isMatchFull() && !Matches[i].InMatch)
            {
                matchID = Matches[i].MatchID;
                if(JoinGame(matchID, player, out playerIndex))
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
                int playerIndex = Matches[i].Players.IndexOf(player.gameObject);
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
