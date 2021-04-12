using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class NetworkComboManager : NetworkBehaviour
{
    public static NetworkComboManager Instance;

    [Header("General")]
    [Range(0, 64)] public float HighlightTolerance;
    public bool ShowExtendedTeamCombos = true;
    public bool TriggerExtendedTeamCombos = true;

    [Header("Line Combo")]
    [Range(0, 22)] public float LineDistance = 5;
    [Range(0, 20)] public float LineThickness = 1;
    [Range(0, 20)] public int MaxTilesInLineCombo = 20;

    [Header("Triangle Combo")]
    [Range(0, 42)] public float TriangleDistance = 10;
    [Tooltip("Ensure the maxTilesInTriangleCombo is set to at least twice the amount of the line combo's!")]
    [Range(0, 64)] public int MaxTilesInTriangleCombo = 64;

    internal NetworkComboPlayer localPlayer;

    private void Awake() => Instance = this;

    public void RegisterPlayer()
    {
        RpcRefreshPlayersTeammates();
    }

    [ClientRpc]
    public void RpcRefreshPlayersTeammates()
    {
        localPlayer.GetTeammates();
    }
}
 