using TMPro;
using UnityEngine;

public class NetworkPlayerScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerScoreText;

    public void UpdateName(string newName) =>  this.playerNameText.text = newName;
    public void UpdateScore(int score) => this.playerScoreText.text = score.ToString();
}
