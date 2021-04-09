using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerHUD : MonoBehaviour
{
    NetworkHUDMainButtons HUDMainButtons;

    private void Awake()
    {
        GameObject HUDMainButtonGameObject = GameObject.FindGameObjectWithTag("MainButtons");
        this.HUDMainButtons = HUDMainButtonGameObject.GetComponent<NetworkHUDMainButtons>();
        this.HUDMainButtons.SetPlayer(gameObject);
    }
}
