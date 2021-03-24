using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class will take care of all players that belong this team.
/// 
/// <para>
/// We have functions to Add new players because a team will start out with
/// 0 players. A player is added when this player joins the lobby and chooses a color.
/// </para>
/// 
/// Last updated 19-03-2021 by Shifat
/// </summary>
public class Team : MonoBehaviour
{
    public enum TeamColor
    {
        Red,
        Blue,
        Green
    }

    [SerializeField] private List<Score> playerScores;

    public TeamColor Color;

    public int GetTotalScore()
    {
        var totalScore = 0;

        int playerScoresLength = this.playerScores.Count;
        for (int i = 0; i < playerScoresLength; i++)
            totalScore += this.playerScores[i].CurrentScore;

        return totalScore;
    }

    public void AddPlayerScoreManager(Score playerScoreToAdd)
    {
        if (playerScoreToAdd == null)
            return;

        this.playerScores.Add(playerScoreToAdd);
    }

    public void RemovePlayerScoreManager(Score playerScoreToRemove)
    {
        if (this.playerScores.Count == 0 || !this.playerScores.Contains(playerScoreToRemove))
            return;

        this.playerScores.Remove(playerScoreToRemove);
    }
}
