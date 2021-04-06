using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Security.Cryptography;
using System.Text;

public class MatchMaker : NetworkBehaviour
{
    // private static MatchMaker instance;
    // [SerializeField]
    //
    // public static MatchMaker Instance
    // {
    //     get
    //     {
    //
    //         if (instance == null)
    //         {
    //             instance = 
    //         }
    //
    //         return instance;
    //     }
    // }

    private static byte maxPlayers = 8;

    private static byte minPlayers = 2;

    public static byte MaxPlayers => maxPlayers;

    // void Start() => Instance = this;

    public static string GetRandomMatchID()
    {
        string id = string.Empty;

        for (int i = 0; i < 5; i++)
        {
            int random = UnityEngine.Random.Range(0, 36);
            if (random < 26)
            {
                // Converts to capital letter
                id += (char) (random + 65);
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
