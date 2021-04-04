using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : Collectable
{
    private CrystalManager crystalManager;

    new protected void Awake()
    {
        this.crystalManager = FindObjectOfType<CrystalManager>();

        base.Awake();
    }


    public override void Collect()
    {
        this.crystalManager.OnCollectedCrystal(this);

        base.Collect();
    }
}
