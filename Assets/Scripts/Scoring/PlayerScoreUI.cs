using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerScoreUI : MonoBehaviour
{
    [SerializeField] private Team team;
    public Team Team { get => this.team; }
    
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerScoreText;

    public void UpdateName(string playerName) => this.playerNameText.text = playerName;
    
    public void UpdateScore(int score) => this.playerScoreText.text = score.ToString();
}
