using Mirror;
using UnityEngine;
using TMPro;

public class TeamScoreManager : NetworkBehaviour
{
    [SerializeField] private GameObject redTeamScorePrefab;
    [SerializeField] private GameObject blueTeamScorePrefab;

    private NetworkTeamScore redTeamScoreUI;
    private NetworkTeamScore blueTeamScoreUI;

    [SyncVar(hook = nameof(OnRedScoreChanged))]
    public int redTeamScore = 0;

    [SyncVar(hook = nameof(OnBlueScoreChanged))]
    public int blueTeamScore = 0;

    private void OnBlueScoreChanged(int oldScore, int newScore) =>
        this.blueTeamScoreUI.UpdateScore(newScore);

    private void OnRedScoreChanged(int oldScore, int newScore) => 
        this.redTeamScoreUI.UpdateScore(newScore);

    private void Start()
    {
        if (isServer) return;

        GameObject HUDGameObject = GameObject.Find("Game HUD");

        var redTeamScoreObject = Instantiate(this.redTeamScorePrefab, HUDGameObject.transform);
        this.redTeamScoreUI = redTeamScoreObject.GetComponent<NetworkTeamScore>();

        var blueTeamScoreObject = Instantiate(this.blueTeamScorePrefab, HUDGameObject.transform);
        this.blueTeamScoreUI = blueTeamScoreObject.GetComponent<NetworkTeamScore>();

        redTeamScoreUI.UpdateScore(0);
        blueTeamScoreUI.UpdateScore(0);
    }

    public void AddScore(Team team, int amount)
    {
        if(team == Team.BlueTeam)
        {
            int newScore = this.blueTeamScore + amount;
            CmdUpdateScore(team, newScore);
            blueTeamScoreUI.UpdateScore(newScore);
        }

        else if(team == Team.RedTeam)
        {
            int newScore = this.redTeamScore + amount;
            CmdUpdateScore(team, newScore);
            redTeamScoreUI.UpdateScore(newScore);
        }
    }

    public void SubstractScore(Team team, int amount)
    {
        if(team == Team.BlueTeam)
        {
            int newScore = this.blueTeamScore - amount;

            if (newScore < 0)
                newScore = 0;

            CmdUpdateScore(team, newScore);
            blueTeamScoreUI.UpdateScore(newScore);
        }

        else if(team == Team.RedTeam)
        {
            int newScore = this.redTeamScore - amount;

            if (newScore < 0)
                newScore = 0;

            CmdUpdateScore(team, newScore);
            redTeamScoreUI.UpdateScore(newScore);
        }
    }

    [Command(ignoreAuthority = true)]
    public void CmdUpdateScore(Team team, int amount)
    {
        if(team == Team.BlueTeam)
            this.blueTeamScore = amount;

        else if(team == Team.RedTeam)
            this.redTeamScore = amount;
    }
}
