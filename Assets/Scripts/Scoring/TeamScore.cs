using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TeamScore : MonoBehaviour
{
    private TeamScoreUI teamScoreUI;
    [SerializeField] private Team team;
    public Team Team { get => this.team; }

    private int score;

    private void Awake()
    { 
        teamScoreUI = FindObjectsOfType<TeamScoreUI>()
            .Single(ui => ui.Team == this.team);
    }

    public void Add(int amountToAdd)
    {
        this.score += amountToAdd;

        this.teamScoreUI.UpdateScore(this.score);
    }

    public void Substract(int amountToSubstract)
    {
        this.score -= amountToSubstract;

        if (this.score < 0)
            this.score = 0;

        this.teamScoreUI.UpdateScore(this.score);
    }
}
