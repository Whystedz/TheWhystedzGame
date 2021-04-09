using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamScoreUI : MonoBehaviour
{
    [SerializeField] private Team team;
    public Team Team { get => this.team; }

    [SerializeField] private TextMeshProUGUI text;

    public void UpdateScore(int score) => this.text.text = score.ToString();

    public int Score => int.Parse(text.text);
}
