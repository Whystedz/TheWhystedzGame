using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamScore : MonoBehaviour
{
    [SerializeField] private TeamScoreUI teamScoreUI;
    [SerializeField] private Team team;
    public Team Team { get => this.team; }

    private int score;

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
