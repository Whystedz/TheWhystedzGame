using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI class that updates the count of items collected by the player.
/// TODO: Maybe make a list of Teams if we want to show the score of all teams.
/// 
/// Last updated 19-03-2021 by Shifat
/// </summary>
public class UIScore : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Team team;

    public void UpdateScoreText() => this.scoreText.text = this.team.GetTotalScore().ToString();
}
