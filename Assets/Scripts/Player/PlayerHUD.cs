using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    HUDMainButtons HUDMainButtons;

    private void Awake()
    {
        GameObject HUDMainButtonGameObject = GameObject.FindGameObjectWithTag("MainButtons");
        this.HUDMainButtons = HUDMainButtonGameObject.GetComponent<HUDMainButtons>();
        this.HUDMainButtons.SetPlayer(gameObject);
    }
}
