using System.Collections.Generic;
using UnityEngine;

public enum ComboType
{
    Line,
    Triangle
}

public struct Combo
{
    public List<Tile> Tiles { get; set; }
    public List<ComboPlayer> Players { get; set; }
    public Vector3 Center { get; set; }
    public ComboType ComboType { get; set; }
    public bool IsTriggered { get; set; }
}
