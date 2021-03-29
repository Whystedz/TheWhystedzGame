using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboHint
{
    public ComboPlayer OriginPlayer { get; set; }
    public ComboPlayer TargetPlayer { get; set; }
    public ComboType ComboType { get; set; }
    public bool MoveTowards { get; set; }
}
