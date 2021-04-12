using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

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
    [SerializeField] private PlayerScoreUI[] teamRedIndividualGUI; 

    [SerializeField] private GameObject teamBlueScoresGUI;
    [SerializeField] private PlayerScoreUI[] teamBlueIndividualGUI; 

    [Header("LeafyBoy Images")] 
    [SerializeField] private Image leafyBoyLeftImg;
    [SerializeField] private Image leafyBoyRightImg;
    [SerializeField] private Sprite[] teamRedLeafyBoysSprites;
    [SerializeField] private Sprite[] teamBlueLeafyBoysSprites;
    [SerializeField] private Image teamBG;
    [SerializeField] private Sprite teamRedBG;
    [SerializeField] private Sprite teamBlueBG;

    private NetworkPlayerScore[] winningPlayers;

    public void EndGame()
    {
        Time.timeScale = 0f;
        ShowEndScreen();
        AudioManager.PlayWinMusic();

        if (TeamScoreManager.Instance.redTeamScore == TeamScoreManager.Instance.blueTeamScore)
        {
            this.winningTeamTxt.text = $"TIE";
            this.leafyBoyLeftImg.sprite = this.teamRedLeafyBoysSprites[0];
            this.leafyBoyRightImg.sprite = this.teamBlueLeafyBoysSprites[1];
            return;
        }
        
        // Update UI.
        bool teamRedWon = TeamScoreManager.Instance.redTeamScore > TeamScoreManager.Instance.blueTeamScore;
        Team winningTeam = teamRedWon ? Team.RedTeam : Team.BlueTeam;
        string teamName = teamRedWon ? this.teamRedName : this.teamBlueName;
        
        string hexColor = ColorUtility.ToHtmlStringRGB
            (winningTeam == Team.RedTeam ? this.teamRedColor : this.teamBlueColor);
        this.winningTeamTxt.text = $"Team <color=#{hexColor}>{teamName}</color> won!!";

        winningPlayers = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == winningTeam)
            .Select(teammate => teammate.GetComponent<NetworkPlayerScore>())
            .ToArray();

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

            redTeam.UpdateScore(TeamScoreManager.Instance.redTeamScore);
            for (int i = 0; i < winningPlayers.Length; i++)
            {
                teamRedIndividualGUI[i].UpdateName(winningPlayers[i].displayName);
                teamRedIndividualGUI[i].UpdateScore(winningPlayers[i].currentScore);
            }
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

            blueTeam.UpdateScore(TeamScoreManager.Instance.redTeamScore);
            for (int i = 0; i < winningPlayers.Length; i++)
            {
                teamBlueIndividualGUI[i].UpdateName(winningPlayers[i].displayName);
                teamBlueIndividualGUI[i].UpdateScore(winningPlayers[i].currentScore);
            }
        }
    }
    
    public void ShowEndScreen() => this.endScreen.SetActive(true);
    
    // TODO: Return to lobby as well.
    public void CloseEndScreen()
    {
        Time.timeScale = 1f;
        MatchController.Instance.RequestExitGame();
        SceneManager.UnloadSceneAsync(LobbyNetworkManager.Instance.MainScene);
    }
}
