using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;

public class NetworkGameTimer : MonoBehaviour
{
    [SerializeField] private double gameTime;
    private bool gameEnded;

    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private EndScreenUI endScreen;

    private void Start()
    {
        this.gameEnded = false;
        StartCoroutine(DoHalfTimeEvent((float) gameTime / 2));
    }

    void Update()
    {
        if (!this.gameEnded)
        {
            if (this.gameTime > 0)
            {
                this.gameTime -= Time.deltaTime;
                DisplayTimer((float) this.gameTime);
            }
            else
            {
                Debug.Log("Time ran out. Game ended!");
                this.gameTime = 0;
                DisplayTimer((float) this.gameTime);
                this.gameEnded = true;
                this.endScreen.EndGame();
                AudioManager.StopSpeedupMusic();
            }
        }
    }

    private IEnumerator DoHalfTimeEvent(float time)
    {
        yield return new WaitForSeconds(time);
        AudioManager.PlaySpeedupMusic();
    }

    private void DisplayTimer(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);

        this.timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
