using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamScoreUI : MonoBehaviour
{
    [SerializeField] private Team team;

    private Text text;

    private void Awake()
    {
        this.text = GetComponent<Text>();
    }

    public void UpdateScore(int score)
    {
        this.text.text = $"Team {team} has {score} points!";
    }
}
