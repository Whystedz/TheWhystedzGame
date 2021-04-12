using TMPro;
using UnityEngine;

public class NetworkTeamScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    public void UpdateScore(int score) => this.scoreText.text = score.ToString();
}
