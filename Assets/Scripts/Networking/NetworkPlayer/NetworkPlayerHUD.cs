using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkPlayerHUD : NetworkBehaviour
{
    NetworkHUDMainButtons HUDMainButtons;

    public override void OnStartAuthority()
    {
        GameObject HUDMainButtonGameObject = GameObject.FindGameObjectWithTag("MainButtons");
        this.HUDMainButtons = HUDMainButtonGameObject.GetComponent<NetworkHUDMainButtons>();
        this.HUDMainButtons.SetPlayer(gameObject);
    }
}
