using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private int pointsWorth;
    public int PointsWorth { get => this.pointsWorth; }
}
