using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkCollectable : NetworkBehaviour
{
    [SerializeField] private int pointsWorth;
    public int PointsWorth { get => this.pointsWorth; }
    
    public virtual void Collect() 
    {
        Destroy(this.gameObject);
    }
}
