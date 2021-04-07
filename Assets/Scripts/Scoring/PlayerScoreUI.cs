using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerScoreUI : MonoBehaviour
{
    [SerializeField] private Team team;

    public Team Team
    {
        get => this.team;
    }

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerScoreText;

    public string PlayerName { get; private set; }

    public void UpdateName(string newName)
    {
        PlayerName = newName;
        this.playerNameText.text = newName;
    }


public void UpdateScore(int score) => this.playerScoreText.text = score.ToString();
}