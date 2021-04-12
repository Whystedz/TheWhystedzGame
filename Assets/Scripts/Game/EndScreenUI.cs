using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private GameObject teamRedScoresGUI;
    [SerializeField] private GameObject teamBlueScoresGUI;

    [Header("LeafyBoy Images")] 
    [SerializeField] private Image leafyBoyLeftImg;
    [SerializeField] private Image leafyBoyRightImg;
    [SerializeField] private Sprite[] teamRedLeafyBoysSprites;
    [SerializeField] private Sprite[] teamBlueLeafyBoysSprites;
    [SerializeField] private Image teamBG;
    [SerializeField] private Sprite teamRedBG;
    [SerializeField] private Sprite teamBlueBG;


    public void EndGame()
    {
        ShowEndScreen();
        AudioManager.PlayWinMusic();

        if (this.redTeam.Score == this.blueTeam.Score)
        {
            this.winningTeamTxt.text = $"TIE";
            this.leafyBoyLeftImg.sprite = this.teamRedLeafyBoysSprites[0];
            this.leafyBoyRightImg.sprite = this.teamBlueLeafyBoysSprites[1];
            return;
        }
        
        // Update UI.
        bool teamRedWon = this.redTeam.Score > this.blueTeam.Score;
        Team winningTeam = teamRedWon ? Team.RedTeam : Team.BlueTeam;
        string teamName = teamRedWon ? this.teamRedName : this.teamBlueName;
        
        string hexColor = ColorUtility.ToHtmlStringRGB
            (winningTeam == Team.RedTeam ? this.teamRedColor : this.teamBlueColor);
        this.winningTeamTxt.text = $"Team <color=#{hexColor}>{teamName}</color> won!!";

        ChangeLeafyBoyImage(winningTeam);
    }
    
    private void ChangeLeafyBoyImage(Team winnerTeam)
    {
        if (winnerTeam == Team.RedTeam)
        {
            this.teamBG.sprite = this.teamRedBG;
            
            this.leafyBoyLeftImg.sprite = this.teamRedLeafyBoysSprites[0];
            this.leafyBoyRightImg.sprite = this.teamRedLeafyBoysSprites[1];

            this.redTeam.gameObject.SetActive(true);
            this.blueTeam.gameObject.SetActive(false);
            this.teamRedScoresGUI.SetActive(true);
            this.teamBlueScoresGUI.SetActive(false);
        }
        else
        {
            this.teamBG.sprite = this.teamBlueBG;

            this.leafyBoyLeftImg.sprite = this.teamBlueLeafyBoysSprites[0];
            this.leafyBoyRightImg.sprite = this.teamBlueLeafyBoysSprites[1];
            
            this.redTeam.gameObject.SetActive(false);
            this.blueTeam.gameObject.SetActive(true);
            this.teamRedScoresGUI.SetActive(false);
            this.teamBlueScoresGUI.SetActive(true);
        }
    }
    
    public void ShowEndScreen() => this.endScreen.SetActive(true);
    
    // TODO: Return to lobby as well.
    public void CloseEndScreen() => this.endScreen.SetActive(false);
}
