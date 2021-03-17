using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayer : MonoBehaviour
{
    [SerializeField] public TMP_Text playerNameText;
    LobbyPlayer player;

    public void SetPlayer(LobbyPlayer player)
    {
        this.player = player;
        playerNameText.text = player.DisplayName;
    }
}
