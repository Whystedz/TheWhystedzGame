using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayer : MonoBehaviour
{
    public TMP_Text PlayerNameText;
    private LobbyPlayer player;

    public void SetPlayer(LobbyPlayer player)
    {
        this.player = player;
        PlayerNameText.text = player.DisplayName;
    }
}
