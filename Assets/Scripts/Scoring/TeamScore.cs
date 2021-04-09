using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TeamScore : MonoBehaviour
{
    [SerializeField] private List<TeamScoreUI> teamScoreUIs;
    [SerializeField] private Team team;
    public Team Team { get => this.team; }

    [SerializeField] private int goalScore;
    private int score;

    private EndScreenUI endScreen;

    private void Awake() => this.endScreen = GameObject.FindWithTag("EndScreen").GetComponent<EndScreenUI>();
    
    public void Add(int amountToAdd)
    {
        this.score += amountToAdd;

        foreach (var teamScoreUI in teamScoreUIs)
            teamScoreUI.UpdateScore(this.score);
        
        if (this.score >= this.goalScore)
            this.endScreen.EndGame();
    }

    public void Substract(int amountToSubstract)
    {
        this.score -= amountToSubstract;

        if (this.score < 0)
            this.score = 0;

        foreach (var teamScoreUI in teamScoreUIs)
            teamScoreUI.UpdateScore(this.score);
    }
}
