using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomGUI : MonoBehaviour
{
    [Header("GUI References")]
    [SerializeField] private TMP_Text matchIDText;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private TMP_Text[] playerNameTexts;
    [SerializeField] private TMP_Text[] playerReadyTexts;

    public void SetRoomCode(string code)
    {
        matchIDText.text = code;
    }

    public void RefreshRoomPlayers(PlayerInfo[] playerInfos)
    {
        for (int i = 0; i < playerInfos.Length; i++)
        {
            playerNameTexts[i].text = string.Empty;
            playerReadyTexts[i].text = string.Empty;
        }

        bool isEveryoneReady = true;

        for (int i = 0; i < playerInfos.Length; i++)
        {
            playerNameTexts[i].text = playerInfos[i].DisplayName;
            playerReadyTexts[i].text = playerInfos[i].IsReady ? 
                "<color=green>Ready</color>" :
                "<color=red>Not Ready</color>";
            if (!playerInfos[i].IsReady)
                isEveryoneReady = false;
        }

        startGameButton.SetActive(isEveryoneReady);
    }
}
