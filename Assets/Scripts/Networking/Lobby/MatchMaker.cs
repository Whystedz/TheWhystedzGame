using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Security.Cryptography;
using System.Text;

public class MatchMaker : NetworkBehaviour
{
    public static MatchMaker Instance { get; set; }
    [SerializeField] private byte maxPlayers = 8;
    [SerializeField] private byte minPlayers = 2;

    public byte MaxPlayers
    {
        get
        {
            return this.maxPlayers;
        }
    }

    void Start() => Instance = this;

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
