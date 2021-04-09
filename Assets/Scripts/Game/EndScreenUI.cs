using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndScreenUI : MonoBehaviour
{
    [SerializeField] private Color teamRedColor;
    [SerializeField] private Color teamBlueColor;

    [Header("UI")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI winningTeamTxt;

    [SerializeField] private TeamScoreUI redTeam;
    [SerializeField] private TeamScoreUI blueTeam;

    [SerializeField] private string teamRedName;
    [SerializeField] private string teamBlueName;
    
    public void EndGame()
    {
        ShowEndScreen();

        if (this.redTeam.Score == this.blueTeam.Score)
        {
            this.winningTeamTxt.text = $"TIE";
            return;
        }
        
        // Update UI.
        bool teamRedWon = this.redTeam.Score > this.blueTeam.Score;
        Team winningTeam = teamRedWon ? Team.RedTeam : Team.BlueTeam;
        string teamName = teamRedWon ? teamRedName : teamBlueName;
        
        string hexColor = ColorUtility.ToHtmlStringRGB
            (winningTeam == Team.RedTeam ? this.teamRedColor : this.teamBlueColor);
        this.winningTeamTxt.text = $"Team <color=#{hexColor}>{teamName}</color> Won!";
    }
    
    public void ShowEndScreen() => this.endScreen.SetActive(true);
    
    public void CloseEndScreen() => this.endScreen.SetActive(false);
}
